using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Common;
using AguasNico_Api.Models.DTO.Routes;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class RouteService(APIContext context, TokenService tokenService, CartService cartService)
{
    private readonly APIContext _db = context;
    private readonly TokenService _tokenService = tokenService;
    private readonly CartService _cartService = cartService;

    public async Task<BaseResponse<GetRoutesResponse>> GetAll(GetRoutesRequest rq)
    {
        var token = _tokenService.GetToken();

        var query = _db.Routes.AsNoTracking().AsQueryable();
        if (token.Role == Roles.Admin)
        {
            query = rq.Day == 0
                ? query.Where(x => x.IsStatic)
                : query.Where(x => x.IsStatic && x.DayOfWeek == (Day)rq.Day);

            if (!string.IsNullOrEmpty(rq.UserId))
                query = query.Where(x => x.UserID == rq.UserId);
        }
        else
            query = query.Where(x => x.IsStatic && x.UserID == token.UserId);

        return new BaseResponse<GetRoutesResponse>
        {
            Data = new GetRoutesResponse
            {
                Routes = await ProjectRoutes(query).ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetRouteResponse>> GetOne(GetRouteRequest rq)
    {
        var rs = new BaseResponse<GetRouteResponse>();
        var token = _tokenService.GetToken();

        var route = await _db.Routes
            .AsNoTracking()
            .Where(x => x.ID == rq.Id)
            .Select(x => new GetRouteResponse
            {
                Id = x.ID,
                UserId = x.UserID,
                DealerName = x.User.Name,
                TruckNumber = x.User.TruckNumber ?? 0,
                DayOfWeek = x.DayOfWeek,
                IsStatic = x.IsStatic,
                IsClosed = x.IsClosed,
                DispenserPrice = x.DispenserPrice,
                CreatedAt = x.CreatedAt,
                TotalCarts = x.Carts.Count,
                CompletedCarts = x.Carts.Count(y => y.State != State.Pending),
                PendingCarts = x.Carts.Count(y => y.State == State.Pending),
                Carts = x.Carts.Select(c => new RouteCartItem
                {
                    Id = c.ID,
                    ClientId = c.ClientID,
                    ClientName = c.Client.Name,
                    ClientAddress = c.Client.Address,
                    ClientDebt = c.Client.Debt,
                    Priority = c.Priority,
                    State = c.State,
                    Collected = c.PaymentMethods.Sum(p => p.Amount)
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));
        if ((route.UserId != token.UserId && token.Role != Roles.Admin) || (route.IsStatic && token.Role != Roles.Admin))
            return rs.SetError(Messages.Error.Unauthorized(), 403);

        // Dealers see just the carts
        if (token.Role == Roles.Admin)
        {
            route.TotalExpenses = await _db.Expenses.Where(x => x.CreatedAt.Date == route.CreatedAt.Date && x.UserID == route.UserId).SumAsync(x => x.Amount);
            route.TotalSold = await GetTotalSoldByRoute(route.Id);
            route.SoldProducts = await GetSoldProductsByRoute(route.Id);
            route.Payments = await GetTotalCollected(route.Id);
            route.Transfers = await _db.Transfers
                .AsNoTracking()
                .Where(x => x.UserID == route.UserId && x.Date.Date == route.CreatedAt.Date)
                .Select(x => new Models.DTO.Routes.TransferItem
                {
                    Id = x.ID,
                    ClientId = x.ClientID,
                    ClientName = x.Client.Name,
                    Amount = x.Amount,
                    Date = x.Date
                })
                .ToListAsync();
        }

        rs.Data = route;
        return rs;
    }

    public async Task<BaseResponse<CreateRouteResponse>> Create(CreateRouteRequest rq)
    {
        var rs = new BaseResponse<CreateRouteResponse>();
        if (!await _db.User.AnyAsync(x => x.Id == rq.UserId))
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));
        if (await _db.Routes.AnyAsync(x => x.DayOfWeek == rq.DayOfWeek && x.IsStatic && x.UserID == rq.UserId))
            return rs.SetError("El repartidor ya tiene una planilla para ese día");

        var route = new Models.Route
        {
            UserID = rq.UserId,
            DayOfWeek = rq.DayOfWeek,
            IsStatic = true
        };
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();

        rs.Data = new CreateRouteResponse { Id = route.ID };
        rs.Message = Messages.CRUD.EntityCreated("Planilla", true);
        return rs;
    }

    public async Task<BaseResponse<CreateRouteByDealerResponse>> CreateByDealer(CreateRouteByDealerRequest rq)
    {
        var rs = new BaseResponse<CreateRouteByDealerResponse>();
        var route = await _db.Routes.Include(x => x.Carts).FirstOrDefaultAsync(x => x.ID == rq.RouteId && x.IsStatic);
        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        var newRoute = new Models.Route
        {
            UserID = route.UserID,
            DayOfWeek = route.DayOfWeek,
            IsStatic = false
        };
        _db.Routes.Add(newRoute);

        foreach (var cart in route.Carts)
        {
            _db.Carts.Add(new Cart
            {
                ClientID = cart.ClientID,
                Route = newRoute,
                Priority = cart.Priority,
                State = State.Pending,
                IsStatic = false
            });
        }

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new CreateRouteByDealerResponse { Id = newRoute.ID };
        rs.Message = Messages.CRUD.EntityCreated("Planilla", true);
        return rs;
    }

    public async Task<BaseResponse> UpdateClients(UpdateRouteClientsRequest rq)
    {
        var rs = new BaseResponse();
        var route = await _db.Routes.Include(x => x.Carts).FirstOrDefaultAsync(x => x.ID == rq.RouteId);
        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        foreach (var cart in route.Carts)
            cart.DeletedAt = LocalClock.Now;

        var clients = await _db.Clients.Where(x => rq.ClientIds.Contains(x.ID)).ToListAsync();
        if (clients.Count != rq.ClientIds.Distinct().Count())
            return rs.SetError(Messages.Error.EntitiesNotFound("clientes"));

        var priority = 1;
        foreach (var clientId in rq.ClientIds)
        {
            var client = clients.First(x => x.ID == clientId);
            client.UpdatedAt = LocalClock.Now;
            client.DealerID = route.UserID;
            client.DeliveryDay = route.DayOfWeek;

            _db.Carts.Add(new Cart
            {
                ClientID = clientId,
                RouteID = rq.RouteId,
                IsStatic = true,
                Priority = priority++,
                State = State.Pending
            });
        }

        route.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = Messages.CRUD.EntityUpdated("Planilla", true);
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteRouteRequest rq)
    {
        var rs = new BaseResponse();
        var route = await _db.Routes.Include(x => x.Carts).Include(x => x.DispatchedProducts).FirstOrDefaultAsync(x => x.ID == rq.RouteId);
        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        try
        {
            await _db.Database.BeginTransactionAsync();
            foreach (var product in route.DispatchedProducts)
                product.DeletedAt = LocalClock.Now;
            foreach (var cart in route.Carts)
                await _cartService.SoftDeleteEffectsInCurrentTransaction(cart.ID);
            route.DeletedAt = LocalClock.Now;
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackIfNeeded();
            return rs.SetError(ex.Message);
        }

        rs.Message = Messages.CRUD.EntityDeleted("Planilla", true);
        return rs;
    }

    public async Task<BaseResponse> Close(CloseRouteRequest rq)
    {
        var rs = new BaseResponse();
        var route = await _db.Routes.Include(x => x.Carts).FirstOrDefaultAsync(x => x.ID == rq.RouteId);
        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        foreach (var cart in route.Carts.Where(x => x.State == State.Pending))
            cart.DeletedAt = LocalClock.Now;
        route.IsClosed = true;
        route.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = "La planilla se cerró correctamente";
        return rs;
    }

    public async Task<BaseResponse<GetRoutesResponse>> SearchByDate(SearchRoutesByDateRequest rq)
    {
        var routes = await ProjectRoutes(_db.Routes.AsNoTracking().Where(x => x.CreatedAt.Date == rq.Date.Date && !x.IsStatic)).ToListAsync();
        foreach (var route in routes)
        {
            route.Collected = await GetTotalSoldByRoute(route.Id);
            route.SoldProducts = await GetSoldProductsByRoute(route.Id, onlySold: true);
        }

        return new BaseResponse<GetRoutesResponse>
        {
            Data = new GetRoutesResponse { Routes = routes }
        };
    }

    public async Task<BaseResponse<GetRoutesResponse>> SearchByDay(SearchRoutesByDayRequest rq)
    {
        var token = _tokenService.GetToken();
        var query = _db.Routes.AsNoTracking().AsQueryable();
        if (token.Role == Roles.Admin)
            query = query.Where(x => x.DayOfWeek == rq.Day && x.IsStatic);
        else
            query = query.Where(x => x.DayOfWeek == rq.Day && !x.IsStatic && x.UserID == token.UserId);

        return new BaseResponse<GetRoutesResponse>
        {
            Data = new GetRoutesResponse
            {
                Routes = await ProjectRoutes(query).ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<SearchSoldProductsResponse>> SearchSoldProducts(SearchSoldProductsRequest rq)
    {
        return new BaseResponse<SearchSoldProductsResponse>
        {
            Data = new SearchSoldProductsResponse
            {
                Items = rq.RouteId > 0 ? await GetSoldProductsByDateAndRoute(rq.Date, rq.RouteId) : await GetSoldProductsByDate(rq.Date)
            }
        };
    }

    public async Task<BaseResponse<ClientsNotInRouteResponse>> ClientsByIDNotInRoute(ClientsByIdNotInRouteRequest rq)
    {
        var client = await ClientsNotInRouteQuery(rq.RouteId).Where(x => x.ID == rq.ClientId).ToListAsync();
        return new BaseResponse<ClientsNotInRouteResponse>
        {
            Data = new ClientsNotInRouteResponse { Items = client.Select(MapClientSummary).ToList() }
        };
    }

    public async Task<BaseResponse<ClientsNotInRouteResponse>> ClientsByNameNotInRoute(ClientsByNameNotInRouteRequest rq)
    {
        var clients = await ClientsNotInRouteQuery(rq.RouteId)
            .Where(x => x.Name.ToLower().Contains(rq.Name.ToLower()) || x.Address.ToLower().Contains(rq.Name.ToLower()))
            .ToListAsync();

        return new BaseResponse<ClientsNotInRouteResponse>
        {
            Data = new ClientsNotInRouteResponse { Items = clients.Select(MapClientSummary).ToList() }
        };
    }

    public async Task<BaseResponse<GetDispatchedResponse>> GetDispatched(GetDispatchedRequest rq)
    {
        var dispatched = await _db.DispatchedProducts.AsNoTracking().Where(x => x.RouteID == rq.RouteId).ToListAsync();
        return new BaseResponse<GetDispatchedResponse>
        {
            Data = new GetDispatchedResponse
            {
                Items = Enum.GetValues<ProductType>()
                    .Where(x => x != ProductType.Maquina)
                    .Select(type => new ProductQuantityItem
                    {
                        Type = type,
                        TypeName = type.GetDisplayName(),
                        Quantity = dispatched.FirstOrDefault(x => x.Type == type)?.Quantity ?? 0
                    })
                    .ToList()
            }
        };
    }

    public async Task<BaseResponse> UpdateDispatched(UpdateDispatchedRequest rq)
    {
        var rs = new BaseResponse();
        var route = await _db.Routes.Include(x => x.DispatchedProducts).FirstOrDefaultAsync(x => x.ID == rq.RouteId);
        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        foreach (var product in route.DispatchedProducts)
            product.DeletedAt = LocalClock.Now;

        var oldProducts = await _db.DispatchedProducts.IgnoreQueryFilters().Where(x => x.RouteID == rq.RouteId).ToListAsync();
        foreach (var product in rq.Products)
        {
            var oldProduct = oldProducts.FirstOrDefault(x => x.Type == product.Type);
            if (oldProduct != null)
            {
                oldProduct.DeletedAt = null;
                oldProduct.Quantity = product.Quantity;
                oldProduct.UpdatedAt = LocalClock.Now;
            }
            else
            {
                _db.DispatchedProducts.Add(new DispatchedProduct { RouteID = rq.RouteId, Type = product.Type, Quantity = product.Quantity });
            }
        }

        try
        {
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch
        {
            await RollbackIfNeeded();
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Message = Messages.CRUD.EntitiesUpdated("Productos");
        return rs;
    }

    public async Task<BaseResponse> SetDispenserPrice(SetDispenserPriceRequest rq)
    {
        var rs = new BaseResponse();
        if (rq.Price < 0)
            return rs.SetError("El precio no puede ser menor a 0");

        var route = await _db.Routes.FirstOrDefaultAsync(x => x.ID == rq.RouteId);
        if (route == null)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        route.DispenserPrice = rq.Price;
        route.UpdatedAt = LocalClock.Now;
        await _db.SaveChangesAsync();

        rs.Message = Messages.CRUD.EntityUpdated("Precio");
        return rs;
    }

    public async Task<BaseResponse<ManualCartDataResponse>> GetManualCartData(ManualCartDataRequest rq)
    {
        var rs = new BaseResponse<ManualCartDataResponse>();
        var route = await ProjectRoutes(_db.Routes.AsNoTracking().Where(x => x.ID == rq.RouteId)).FirstOrDefaultAsync();
        if (route == null || route.IsStatic)
            return rs.SetError(Messages.Error.EntityNotFound("Planilla", true));

        rs.Data = new ManualCartDataResponse
        {
            Route = route,
            PaymentMethods = await _db.PaymentMethods.AsNoTracking().Select(x => new PaymentMethodItem { Id = x.ID, Name = x.Name }).ToListAsync()
        };
        return rs;
    }

    public async Task<decimal> GetTotalSoldByRoute(long routeId)
    {
        var route = await _db.Routes.AsNoTracking().FirstOrDefaultAsync(x => x.ID == routeId) ?? throw new Exception("No se ha encontrado la planilla");
        return await _db.CartPaymentMethods.Where(x => x.Cart.RouteID == routeId).SumAsync(x => x.Amount)
            + await _db.Transfers.Where(x => x.Date.Date == route.CreatedAt.Date && x.UserID == route.UserID).SumAsync(x => x.Amount)
            + route.DispenserPrice;
    }

    public async Task<List<PaymentAmountItem>> GetTotalCollected(long routeId)
    {
        var methods = await _db.PaymentMethods.AsNoTracking().Where(x => x.Name == "Efectivo").ToListAsync();
        var result = new List<PaymentAmountItem>();
        foreach (var method in methods)
        {
            result.Add(new PaymentAmountItem
            {
                PaymentMethodId = method.ID,
                PaymentMethodName = method.Name,
                Amount = await _db.CartPaymentMethods.Where(x => x.Cart.RouteID == routeId && x.PaymentMethodID == method.ID).SumAsync(x => x.Amount)
            });
        }
        return result;
    }

    public async Task<List<SoldProductsItem>> GetSoldProductsByDate(DateTime date)
    {
        return await BuildSoldProducts(
            _db.CartProducts.Where(x => x.Cart.CreatedAt.Date == date.Date),
            _db.CartAbonoProducts.Where(x => x.Cart.CreatedAt.Date == date.Date),
            _db.DispatchedProducts.Where(x => x.Route.CreatedAt.Date == date.Date),
            _db.ReturnedProducts.Where(x => x.Cart.CreatedAt.Date == date.Date));
    }

    public async Task<List<SoldProductsItem>> GetSoldProductsByDateAndRoute(DateTime date, long routeId)
    {
        return await BuildSoldProducts(
            _db.CartProducts.Where(x => x.CreatedAt.Date == date.Date && x.Cart.RouteID == routeId),
            _db.CartAbonoProducts.Where(x => x.CreatedAt.Date == date.Date && x.Cart.RouteID == routeId),
            _db.DispatchedProducts.Where(x => x.CreatedAt.Date == date.Date && x.RouteID == routeId),
            _db.ReturnedProducts.Where(x => x.CreatedAt.Date == date.Date && x.Cart.RouteID == routeId));
    }

    public async Task<List<SoldProductsItem>> GetSoldProductsByRoute(long routeId, bool onlySold = false)
    {
        var sold = await BuildSoldProducts(
            _db.CartProducts.Where(x => x.Cart.RouteID == routeId),
            _db.CartAbonoProducts.Where(x => x.Cart.RouteID == routeId),
            _db.DispatchedProducts.Where(x => x.RouteID == routeId),
            _db.ReturnedProducts.Where(x => x.Cart.RouteID == routeId));

        if (onlySold)
            sold = [.. sold.Where(x => x.Sold > 0)];

        var clientStock = await _db.Carts
            .Where(x => x.RouteID == routeId)
            .SelectMany(x => x.Client.Products)
            .Select(x => new { x.Product.Type, x.Stock })
            .ToListAsync();

        foreach (var item in sold)
        {
            var type = Enum.GetValues<ProductType>().First(x => x.GetDisplayName() == item.Name);
            item.ClientStock = clientStock.Where(x => x.Type == type).Sum(x => x.Stock);
        }

        return sold;
    }

    public async Task<List<SoldProductsItem>> GetSoldProductsBetweenDates(DateTime dateFrom, DateTime dateTo, string dealerId)
    {
        var cartProducts = await _db.CartProducts
            .AsNoTracking()
            .Where(x => x.Cart.CreatedAt.Date >= dateFrom.Date && x.Cart.CreatedAt.Date <= dateTo.Date && x.Cart.Route.UserID == dealerId)
            .Select(x => new { x.Type, x.Quantity, Total = x.SettedPrice * x.Quantity })
            .ToListAsync();

        var cartAbonoProducts = await _db.CartAbonoProducts
            .AsNoTracking()
            .Where(x => x.Cart.CreatedAt.Date >= dateFrom.Date && x.Cart.CreatedAt.Date <= dateTo.Date && x.Cart.Route.UserID == dealerId)
            .Select(x => new { x.Type, x.Quantity })
            .ToListAsync();

        return Enum.GetValues<ProductType>().Select(type => new SoldProductsItem
        {
            Name = type.GetDisplayName(),
            Sold = cartProducts.Where(x => x.Type == type).Sum(x => x.Quantity) + cartAbonoProducts.Where(x => x.Type == type).Sum(x => x.Quantity),
            Total = cartProducts.Where(x => x.Type == type).Sum(x => x.Total)
        }).ToList();
    }

    private static IQueryable<RouteItem> ProjectRoutes(IQueryable<Models.Route> query)
    {
        return query.Select(x => new RouteItem
        {
            Id = x.ID,
            UserId = x.UserID,
            DealerName = x.User.Name,
            TruckNumber = x.User.TruckNumber ?? 0,
            DayOfWeek = x.DayOfWeek,
            IsStatic = x.IsStatic,
            IsClosed = x.IsClosed,
            DispenserPrice = x.DispenserPrice,
            CreatedAt = x.CreatedAt,
            TotalCarts = x.Carts.Count,
            CompletedCarts = x.Carts.Count(y => y.State != State.Pending),
            PendingCarts = x.Carts.Count(y => y.State == State.Pending)
        }).OrderBy(x => x.DealerName);
    }

    private IQueryable<Client> ClientsNotInRouteQuery(long routeId)
    {
        return _db.Clients
            .AsNoTracking()
            .Include(x => x.Dealer)
            .Where(x => x.IsActive && !x.Carts.Any(c => c.RouteID == routeId));
    }

    private static ClientSummaryItem MapClientSummary(Client x)
    {
        return new ClientSummaryItem
        {
            Id = x.ID,
            Name = x.Name,
            Address = x.Address,
            Phone = x.Phone,
            Email = x.Email ?? "",
            DealerId = x.DealerID,
            DealerName = x.Dealer != null ? x.Dealer.Name : "",
            DeliveryDay = x.DeliveryDay ?? 0,
            Debt = x.Debt,
            HasInvoice = x.HasInvoice,
            OnlyAbonos = x.OnlyAbonos,
            IsActive = x.IsActive
        };
    }

    private async Task<List<SoldProductsItem>> BuildSoldProducts(IQueryable<CartProduct> cartProductsQuery, IQueryable<CartAbonoProduct> cartAbonoProductsQuery, IQueryable<DispatchedProduct> dispatchedProductsQuery, IQueryable<ReturnedProduct> returnedProductsQuery)
    {
        var cartProducts = await cartProductsQuery.AsNoTracking().Select(x => new { x.Type, x.Quantity, Total = x.SettedPrice * x.Quantity }).ToListAsync();
        var cartAbonoProducts = await cartAbonoProductsQuery.AsNoTracking().Select(x => new { x.Type, x.Quantity }).ToListAsync();
        var dispatchedProducts = await dispatchedProductsQuery.AsNoTracking().Select(x => new { x.Type, x.Quantity }).ToListAsync();
        var returnedProducts = await returnedProductsQuery.AsNoTracking().Select(x => new { x.Type, x.Quantity }).ToListAsync();

        var result = new List<SoldProductsItem>();
        foreach (var type in Enum.GetValues<ProductType>())
        {
            result.Add(new SoldProductsItem
            {
                Name = type.GetDisplayName(),
                Sold = cartProducts.Where(x => x.Type == type).Sum(x => x.Quantity) + cartAbonoProducts.Where(x => x.Type == type).Sum(x => x.Quantity),
                Total = cartProducts.Where(x => x.Type == type).Sum(x => x.Total),
                Dispatched = dispatchedProducts.Where(x => x.Type == type).Sum(x => x.Quantity),
                Returned = returnedProducts.Where(x => x.Type == type).Sum(x => x.Quantity)
            });
        }

        return result;
    }

    private async Task RollbackIfNeeded()
    {
        if (_db.Database.CurrentTransaction != null)
            await _db.Database.RollbackTransactionAsync();
    }
}


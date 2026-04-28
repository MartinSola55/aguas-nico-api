using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Clients;
using AguasNico_Api.Models.DTO.Common;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class ClientService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetAllClientsResponse>> GetAll(GetAllClientsRequest rq)
    {
        var query = _db.Clients.AsNoTracking().AsQueryable();
        if (rq.ActiveOnly)
            query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(rq.Search))
            query = query.Where(x => x.Name.Contains(rq.Search) || x.Address.Contains(rq.Search));
        if (!string.IsNullOrEmpty(rq.DealerId))
            query = query.Where(x => x.DealerID == rq.DealerId);
        if (rq.DeliveryDay != 0)
            query = query.Where(x => x.DeliveryDay == rq.DeliveryDay);

        return new BaseResponse<GetAllClientsResponse>
        {
            Data = new GetAllClientsResponse
            {
                Items = await query
                    .Select(x => new ClientSummaryItem
                    {
                        Id = x.ID,
                        Name = x.Name,
                        Address = x.Address,
                        Phone = x.Phone,
                        Email = x.Email ?? "",
                        DealerId = x.DealerID ?? "",
                        DealerName = x.Dealer != null ? x.Dealer.Name : "",
                        DeliveryDay = x.DeliveryDay ?? 0,
                        Debt = x.Debt,
                        HasInvoice = x.HasInvoice,
                        OnlyAbonos = x.OnlyAbonos,
                        IsActive = x.IsActive
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetClientResponse>> GetOne(GetClientRequest rq)
    {
        var rs = new BaseResponse<GetClientResponse>();
        var client = await _db.Clients
            .AsNoTracking()
            .Where(x => x.ID == rq.Id)
            .Select(x => new GetClientResponse
            {
                Id = x.ID,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Email = x.Email ?? "",
                DealerId = x.DealerID ?? "",
                DealerName = x.Dealer != null ? x.Dealer.Name : "",
                DeliveryDay = x.DeliveryDay ?? 0,
                Debt = x.Debt,
                HasInvoice = x.HasInvoice,
                OnlyAbonos = x.OnlyAbonos,
                IsActive = x.IsActive,
                Observations = x.Observations ?? "",
                Notes = x.Notes ?? "",
                InvoiceType = x.InvoiceType ?? 0,
                TaxCondition = x.TaxCondition ?? 0,
                CUIT = x.CUIT ?? "",
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (client == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        if (rq.IncludeDetails)
        {
            client.Products = await GetAllProductsForClient(rq.Id);
            client.Abonos = await GetAllAbonosForClient(rq.Id);
            client.CartsTransfersHistory = await GetCartsTransfersHistory(rq.Id);
            client.ProductsHistory = await GetProductsHistoryItems(rq.Id);
        }

        rs.Data = client;
        return rs;
    }

    public async Task<BaseResponse<CreateClientResponse>> Create(CreateClientRequest rq)
    {
        var rs = new BaseResponse<CreateClientResponse>();
        if (!rs.Attach(await ValidateClient<CreateClientResponse>(rq)).Success)
            return rs;

        var productTypes = await _db.Products
            .Where(x => rq.Products.Select(y => y.ProductId).Contains(x.ID))
            .Select(x => new { x.ID, x.Type })
            .ToListAsync();

        if (productTypes.GroupBy(x => x.Type).Any(x => x.Count() > 1))
            return rs.SetError("No se pueden agregar dos productos del mismo tipo");

        var client = new Client
        {
            Name = rq.Name!.Trim(),
            Address = rq.Address!.Trim(),
            Phone = rq.Phone!.Trim(),
            Email = rq.Email,
            Observations = rq.Observations,
            Notes = rq.Notes,
            Debt = rq.Debt,
            DealerID = rq.DealerId,
            HasInvoice = rq.HasInvoice,
            OnlyAbonos = rq.OnlyAbonos,
            InvoiceType = rq.InvoiceType,
            TaxCondition = rq.TaxCondition,
            CUIT = rq.CUIT,
            DeliveryDay = rq.DeliveryDay,
            Products = rq.Products.Select(x => new ClientProduct { ProductID = x.ProductId, Stock = x.Stock }).ToList(),
            Abonos = rq.AbonoIds.Select(x => new ClientAbono { AbonoID = x }).ToList()
        };

        _db.Clients.Add(client);

        if (client.DealerID is not null && client.DeliveryDay is not null)
        {
            var route = await _db.Routes
                .Include(x => x.Carts)
                .FirstOrDefaultAsync(x => x.UserID == client.DealerID && x.DayOfWeek == client.DeliveryDay && x.IsStatic);

            if (route is not null)
            {
                _db.Carts.Add(new Cart
                {
                    RouteID = route.ID,
                    Client = client,
                    Priority = route.Carts.Any() ? route.Carts.Max(x => x.Priority) + 1 : 1,
                    State = State.Pending,
                    IsStatic = true,
                });
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

        rs.Data = new CreateClientResponse { Id = client.ID };
        rs.Message = Messages.CRUD.EntityCreated("Cliente");
        return rs;
    }

    public async Task<BaseResponse<UpdateClientResponse>> Update(UpdateClientRequest rq)
    {
        var rs = new BaseResponse<UpdateClientResponse>();
        var oldClient = await _db.Clients.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (oldClient == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        if (!rs.Attach(await ValidateClient<UpdateClientResponse>(rq)).Success)
            return rs;

        oldClient.Name = rq.Name!.Trim();
        oldClient.Address = rq.Address!.Trim();
        oldClient.Phone = rq.Phone!.Trim();
        oldClient.Email = rq.Email;
        oldClient.Observations = rq.Observations;
        oldClient.Notes = rq.Notes;
        oldClient.Debt = rq.Debt;
        oldClient.HasInvoice = rq.HasInvoice;
        oldClient.OnlyAbonos = rq.OnlyAbonos;
        oldClient.UpdatedAt = LocalClock.Now;

        if (oldClient.DealerID != rq.DealerId || oldClient.DeliveryDay != rq.DeliveryDay)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(x => x.ClientID == rq.Id && x.IsStatic);
            if (cart is not null)
                cart.DeletedAt = LocalClock.Now;

            var route = await _db.Routes
                .Include(x => x.Carts)
                .FirstOrDefaultAsync(x => x.UserID == rq.DealerId && x.DayOfWeek == rq.DeliveryDay && x.IsStatic);

            if (route is not null)
            {
                _db.Carts.Add(new Cart
                {
                    RouteID = route.ID,
                    ClientID = rq.Id,
                    IsStatic = true,
                    State = State.Pending,
                    Priority = route.Carts.Any() ? route.Carts.Max(x => x.Priority) + 1 : 1,
                });
            }

            oldClient.DealerID = rq.DealerId;
            oldClient.DeliveryDay = rq.DeliveryDay;
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

        rs.Data = new UpdateClientResponse { Id = oldClient.ID };
        rs.Message = Messages.CRUD.EntityUpdated("Cliente");
        return rs;
    }

    public async Task<BaseResponse> UpdateInvoiceData(UpdateInvoiceDataRequest rq)
    {
        var rs = new BaseResponse();
        var oldClient = await _db.Clients.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (oldClient == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        if (!string.IsNullOrEmpty(rq.CUIT) && rq.CUIT.Length > 11)
            return rs.SetError(Messages.Error.InvalidField("CUIT"));

        oldClient.InvoiceType = rq.InvoiceType;
        oldClient.TaxCondition = rq.TaxCondition;
        oldClient.CUIT = rq.CUIT;
        oldClient.UpdatedAt = LocalClock.Now;

        await _db.SaveChangesAsync();
        rs.Message = Messages.CRUD.EntitiesUpdated("datos de facturación");
        return rs;
    }

    public async Task<BaseResponse> UpdateProducts(UpdateClientProductsRequest rq)
    {
        var rs = new BaseResponse();
        if (!await _db.Clients.AnyAsync(x => x.ID == rq.ClientId))
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        var productIds = rq.Products.Select(x => x.ProductId).ToList();
        var products = await _db.Products.Where(x => productIds.Contains(x.ID)).ToListAsync();
        if (products.Count != productIds.Distinct().Count())
            return rs.SetError(Messages.Error.EntitiesNotFound("productos"));
        if (products.GroupBy(x => x.Type).Any(x => x.Count() > 1))
            return rs.SetError("No se pueden agregar dos productos del mismo tipo");
        if (rq.Products.Any(x => x.Stock < 0 || x.Stock > 200))
            return rs.SetError(Messages.Error.InvalidField("stock"));

        var clientProducts = await _db.ClientProducts
            .IgnoreQueryFilters()
            .Where(x => x.ClientID == rq.ClientId)
            .ToListAsync();

        foreach (var product in rq.Products)
        {
            var clientProduct = clientProducts.FirstOrDefault(x => x.ProductID == product.ProductId);
            if (clientProduct != null)
            {
                clientProduct.Stock = product.Stock;
                clientProduct.UpdatedAt = LocalClock.Now;
                clientProduct.DeletedAt = null;
            }
            else
            {
                _db.ClientProducts.Add(new ClientProduct
                {
                    ClientID = rq.ClientId,
                    ProductID = product.ProductId,
                    Stock = product.Stock
                });
            }
        }

        foreach (var existingProduct in clientProducts.Where(x => !rq.Products.Any(y => y.ProductId == x.ProductID)))
            existingProduct.DeletedAt = LocalClock.Now;

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

    public async Task<BaseResponse> UpdateAbonos(UpdateClientAbonosRequest rq)
    {
        var rs = new BaseResponse();
        if (!await _db.Clients.AnyAsync(x => x.ID == rq.ClientId))
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        var abonoCount = await _db.Abonos.CountAsync(x => rq.AbonoIds.Contains(x.ID));
        if (abonoCount != rq.AbonoIds.Distinct().Count())
            return rs.SetError(Messages.Error.EntitiesNotFound("abonos"));

        var clientAbonos = await _db.ClientAbonos
            .IgnoreQueryFilters()
            .Where(x => x.ClientID == rq.ClientId)
            .ToListAsync();

        foreach (var abonoId in rq.AbonoIds.Distinct())
        {
            var clientAbono = clientAbonos.FirstOrDefault(x => x.AbonoID == abonoId);
            if (clientAbono != null)
            {
                clientAbono.UpdatedAt = LocalClock.Now;
                clientAbono.DeletedAt = null;
            }
            else
            {
                _db.ClientAbonos.Add(new ClientAbono { ClientID = rq.ClientId, AbonoID = abonoId });
            }
        }

        foreach (var existingAbono in clientAbonos.Where(x => !rq.AbonoIds.Contains(x.AbonoID)))
            existingAbono.DeletedAt = LocalClock.Now;

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

        rs.Message = Messages.CRUD.EntitiesUpdated("Abonos");
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteClientRequest rq)
    {
        var rs = new BaseResponse();
        var client = await _db.Clients
            .Include(x => x.Carts)
            .Include(x => x.Products)
            .Include(x => x.Abonos)
            .Include(x => x.Transfers)
            .Include(x => x.AbonosRenewed)
            .FirstOrDefaultAsync(x => x.ID == rq.Id);

        if (client == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        client.IsActive = false;
        foreach (var cart in client.Carts.Where(x => x.IsStatic))
            cart.DeletedAt = LocalClock.Now;
        foreach (var clientProduct in client.Products)
            clientProduct.DeletedAt = LocalClock.Now;
        foreach (var abono in client.Abonos)
            abono.DeletedAt = LocalClock.Now;
        foreach (var transfer in client.Transfers)
            transfer.DeletedAt = LocalClock.Now;
        foreach (var abonoRenewed in client.AbonosRenewed)
            abonoRenewed.DeletedAt = LocalClock.Now;

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

        rs.Message = Messages.CRUD.EntityDeleted("Cliente");
        return rs;
    }

    public async Task<BaseResponse<GetProductsAndAbonoResponse>> GetProductsAndAbono(GetProductsAndAbonoRequest rq)
    {
        var rs = new BaseResponse<GetProductsAndAbonoResponse>();
        if (!await _db.Clients.AnyAsync(x => x.ID == rq.Id))
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        var today = LocalClock.Now;
        var products = await _db.ClientProducts
            .AsNoTracking()
            .Where(x => x.ClientID == rq.Id && x.Product.Type != ProductType.Maquina)
            .Select(x => new GetProductsAndAbonoResponse.ProductOptionItem
            {
                Type = x.Product.Type,
                Name = x.Product.Name,
                Price = x.Product.Price
            })
            .ToListAsync();

        var abonoProducts = await _db.AbonoRenewalProducts
            .AsNoTracking()
            .Where(x => x.Type != ProductType.Maquina && x.AbonoRenewal.ClientID == rq.Id && x.CreatedAt.Month == today.Month && x.CreatedAt.Year == today.Year)
            .GroupBy(x => x.Type)
            .Select(x => new GetProductsAndAbonoResponse.AbonoProductOptionItem
            {
                Type = x.Key,
                Name = x.Key.GetDisplayName(),
                Available = x.Sum(y => y.Available)
            })
            .ToListAsync();

        rs.Data = new GetProductsAndAbonoResponse { Products = products, AbonoProducts = abonoProducts };
        return rs;
    }

    public async Task<BaseResponse<GetProductsHistoryResponse>> GetProductsHistory(GetProductsHistoryRequest rq)
    {
        return new BaseResponse<GetProductsHistoryResponse>
        {
            Data = new GetProductsHistoryResponse
            {
                Items = await GetProductsHistoryItems(rq.Id)
            }
        };
    }

    public async Task<BaseResponse<GetUnassignedClientsResponse>> GetUnassigned()
    {
        return new BaseResponse<GetUnassignedClientsResponse>
        {
            Data = new GetUnassignedClientsResponse
            {
                Items = await _db.Clients
                    .AsNoTracking()
                    .Where(x => x.IsActive && string.IsNullOrEmpty(x.DealerID) && x.DeliveryDay == null)
                    .Select(x => new ClientSummaryItem
                    {
                        Id = x.ID,
                        Name = x.Name,
                        Address = x.Address,
                        Phone = x.Phone,
                        Email = x.Email ?? "",
                        DealerId = x.DealerID ?? "",
                        DealerName = "",
                        DeliveryDay = x.DeliveryDay ?? 0,
                        Debt = x.Debt,
                        HasInvoice = x.HasInvoice,
                        OnlyAbonos = x.OnlyAbonos,
                        IsActive = x.IsActive
                    })
                    .ToListAsync()
            }
        };
    }

    private async Task<List<ClientProductItem>> GetAllProductsForClient(long clientId)
    {
        var products = await _db.Products.AsNoTracking().Where(x => x.IsActive).ToListAsync();
        var clientProducts = await _db.ClientProducts.AsNoTracking().Where(x => x.ClientID == clientId).ToListAsync();

        return products
            .Select(product =>
            {
                var assigned = clientProducts.FirstOrDefault(x => x.ProductID == product.ID);
                return new ClientProductItem
                {
                    ProductId = product.ID,
                    ProductName = product.Name,
                    Type = product.Type,
                    TypeName = product.Type.GetDisplayName(),
                    Price = product.Price,
                    Stock = assigned?.Stock ?? -1,
                    Assigned = assigned != null
                };
            })
            .ToList();
    }

    private async Task<List<ClientAbonoItem>> GetAllAbonosForClient(long clientId)
    {
        var abonos = await _db.Abonos.AsNoTracking().ToListAsync();
        var clientAbonos = await _db.ClientAbonos.AsNoTracking().Where(x => x.ClientID == clientId).ToListAsync();

        return abonos.Select(x => new ClientAbonoItem
        {
            AbonoId = x.ID,
            AbonoName = x.Name,
            Price = x.Price,
            Assigned = clientAbonos.Any(y => y.AbonoID == x.ID)
        }).ToList();
    }

    private async Task<List<ProductHistoryItem>> GetProductsHistoryItems(long clientId)
    {
        var soldProducts = await _db.CartProducts
            .AsNoTracking()
            .Where(x => x.Cart.ClientID == clientId && x.Type != ProductType.Maquina)
            .Select(x => new ProductHistoryItem
            {
                ProductType = x.Type,
                ProductTypeName = x.Type.GetDisplayName(),
                ActionType = ProductActionType.Baja,
                ActionTypeName = ProductActionType.Baja.GetDisplayName(),
                Quantity = x.Quantity,
                Date = x.CreatedAt
            })
            .ToListAsync();

        var abonoProducts = await _db.CartAbonoProducts
            .AsNoTracking()
            .Where(x => x.Cart.ClientID == clientId && x.Type != ProductType.Maquina)
            .Select(x => new ProductHistoryItem
            {
                ProductType = x.Type,
                ProductTypeName = x.Type.GetDisplayName(),
                ActionType = ProductActionType.Abono,
                ActionTypeName = ProductActionType.Abono.GetDisplayName(),
                Quantity = x.Quantity,
                Date = x.CreatedAt
            })
            .ToListAsync();

        var returnedProducts = await _db.ReturnedProducts
            .AsNoTracking()
            .Where(x => x.Cart.ClientID == clientId && x.Type != ProductType.Maquina)
            .Select(x => new ProductHistoryItem
            {
                ProductType = x.Type,
                ProductTypeName = x.Type.GetDisplayName(),
                ActionType = ProductActionType.Devuelve,
                ActionTypeName = ProductActionType.Devuelve.GetDisplayName(),
                Quantity = x.Quantity,
                Date = x.CreatedAt
            })
            .ToListAsync();

        return soldProducts.Concat(abonoProducts).Concat(returnedProducts).ToList();
    }

    private async Task<List<CartsTransfersHistoryItem>> GetCartsTransfersHistory(long clientId)
    {
        var transfers = await _db.Transfers
            .AsNoTracking()
            .Where(x => x.ClientID == clientId)
            .Select(x => new CartsTransfersHistoryItem
            {
                Date = x.Date,
                Type = CartsTransfersType.Transfer,
                TransferAmount = x.Amount
            })
            .ToListAsync();

        var abonos = await _db.AbonoRenewals
            .AsNoTracking()
            .Where(x => x.ClientID == clientId)
            .Select(x => new CartsTransfersHistoryItem
            {
                Date = x.CreatedAt,
                Type = CartsTransfersType.Abono,
                AbonoName = x.Abono.Name,
                AbonoPrice = x.Abono.Price
            })
            .ToListAsync();

        var carts = await _db.Carts
            .AsNoTracking()
            .Where(x => x.ClientID == clientId && !x.IsStatic && x.State != State.Pending)
            .Select(x => new CartsTransfersHistoryItem
            {
                Date = x.CreatedAt,
                Type = CartsTransfersType.Cart,
                CartState = x.State,
                PaymentMethods = x.PaymentMethods.Select(p => new PaymentAmountItem
                {
                    PaymentMethodId = p.PaymentMethodID,
                    PaymentMethodName = p.PaymentMethod.Name,
                    Amount = p.Amount
                }).ToList(),
                Products = x.Products.Select(p => new ProductQuantityItem
                {
                    Type = p.Type,
                    TypeName = p.Type.GetDisplayName(),
                    Quantity = p.Quantity,
                    SettedPrice = p.SettedPrice
                }).ToList(),
                AbonoProducts = x.AbonoProducts.Select(p => new ProductQuantityItem
                {
                    Type = p.Type,
                    TypeName = p.Type.GetDisplayName(),
                    Quantity = p.Quantity
                }).ToList()
            })
            .ToListAsync();

        return transfers.Concat(abonos).Concat(carts).ToList();
    }

    private async Task<BaseResponse<T>> ValidateClient<T>(CreateClientRequest rq)
    {
        var rs = new BaseResponse<T>();
        if (string.IsNullOrWhiteSpace(rq.Name))
            return rs.SetError(Messages.Error.FieldRequired("nombre"));
        if (rq.Name.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("nombre"));
        if (string.IsNullOrWhiteSpace(rq.Address))
            return rs.SetError(Messages.Error.FieldRequired("dirección"));
        if (rq.Address.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("dirección"));
        if (string.IsNullOrWhiteSpace(rq.Phone))
            return rs.SetError(Messages.Error.FieldRequired("teléfono"));
        if (rq.Phone.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("teléfono"));
        if (!string.IsNullOrEmpty(rq.Email) && rq.Email.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("email"));
        if (!string.IsNullOrEmpty(rq.Observations) && rq.Observations.Length > 300)
            return rs.SetError(Messages.Error.InvalidField("observaciones"));
        if (!string.IsNullOrEmpty(rq.Notes) && rq.Notes.Length > 300)
            return rs.SetError(Messages.Error.InvalidField("notas"));
        if (!string.IsNullOrEmpty(rq.CUIT) && rq.CUIT.Length > 11)
            return rs.SetError(Messages.Error.InvalidField("CUIT"));
        if (!string.IsNullOrEmpty(rq.DealerId) && !await _db.User.AnyAsync(x => x.Id == rq.DealerId))
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));
        return rs;
    }

    private async Task RollbackIfNeeded()
    {
        if (_db.Database.CurrentTransaction != null)
            await _db.Database.RollbackTransactionAsync();
    }
}


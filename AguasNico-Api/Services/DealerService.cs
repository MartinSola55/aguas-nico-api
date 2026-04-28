using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Common;
using AguasNico_Api.Models.DTO.Dealers;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class DealerService(APIContext context, RouteService routeService)
{
    private readonly APIContext _db = context;
    private readonly RouteService _routeService = routeService;

    public async Task<BaseResponse<GetDealersResponse>> GetAll()
    {
        var dealerRole = await _db.Roles.FirstAsync(x => x.Name == Roles.Dealer);
        var dealerIds = await _db.UserRoles.Where(x => x.RoleId == dealerRole.Id).Select(x => x.UserId).ToListAsync();

        return new BaseResponse<GetDealersResponse>
        {
            Data = new GetDealersResponse
            {
                Items = await _db.User
                    .AsNoTracking()
                    .Where(x => dealerIds.Contains(x.Id))
                    .Select(x => new DealerItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Email = x.Email ?? "",
                        TruckNumber = x.TruckNumber ?? 0
                    })
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetDealerResponse>> GetOne(GetDealerRequest rq)
    {
        var rs = new BaseResponse<GetDealerResponse>();
        var dealer = await _db.User.AsNoTracking().Where(x => x.Id == rq.Id).Select(x => new DealerItem
        {
            Id = x.Id,
            Name = x.Name,
            Email = x.Email ?? "",
            TruckNumber = x.TruckNumber ?? 0
        }).FirstOrDefaultAsync();

        if (dealer == null)
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));

        var date = new DateTime(rq.Year == 0 ? LocalClock.Now.Year : rq.Year, rq.Month == 0 ? LocalClock.Now.Month : rq.Month, 1);
        var dispensers = await _db.Routes.Where(x => x.UserID == rq.Id).SumAsync(x => x.DispenserPrice);

        rs.Data = new GetDealerResponse
        {
            Dealer = dealer,
            TotalCarts = await _db.Carts.Where(x => x.Route.UserID == rq.Id && x.CreatedAt.Month == date.Month && x.CreatedAt.Year == date.Year && !x.IsStatic).CountAsync(),
            CompletedCarts = await _db.Carts.Where(x => x.Route.UserID == rq.Id && x.State == State.Confirmed && x.CreatedAt.Month == date.Month && x.CreatedAt.Year == date.Year && !x.IsStatic).CountAsync(),
            PendingCarts = await _db.Carts.Where(x => x.Route.UserID == rq.Id && x.State != State.Confirmed && x.CreatedAt.Month == date.Month && x.CreatedAt.Year == date.Year && !x.IsStatic).CountAsync(),
            TotalCollected = await _db.CartPaymentMethods.Where(x => x.Cart.Route.UserID == rq.Id && x.CreatedAt.Month == date.Month && x.CreatedAt.Year == date.Year).SumAsync(x => x.Amount)
                + await _db.Transfers.Where(x => x.UserID == rq.Id && x.Date.Month == date.Month && x.Date.Year == date.Year).SumAsync(x => x.Amount)
                + dispensers,
            TotalDebt = await _db.Clients.Where(x => x.DealerID == rq.Id).SumAsync(x => x.Debt),
            ClientsStock = await _db.Clients
                .Where(x => x.DealerID == rq.Id)
                .SelectMany(x => x.Products)
                .GroupBy(x => x.Product.Type)
                .Select(x => new ClientStockItem { Product = x.Key.GetDisplayName(), Stock = x.Sum(y => y.Stock) })
                .ToListAsync()
        };

        return rs;
    }

    public async Task<BaseResponse<GetDealerSheetsResponse>> GetSheets(GetDealerSheetsRequest rq)
    {
        var rs = new BaseResponse<GetDealerSheetsResponse>();
        if (!await _db.User.AnyAsync(x => x.Id == rq.DealerId))
            return rs.SetError(Messages.Error.EntityNotFound("Repartidor"));

        var today = LocalClock.Now;
        var staticCarts = await _db.Carts
            .AsNoTracking()
            .Where(x => x.Route.UserID == rq.DealerId && x.IsStatic && x.Client.IsActive)
            .Select(x => new
            {
                Day = x.Route.DayOfWeek,
                x.ClientID,
                ClientName = x.Client.Name,
                ClientPhone = x.Client.Phone,
                ClientAddress = x.Client.Address,
                ClientObservations = x.Client.Observations,
                ClientDebt = x.Client.Debt,
                x.Client.OnlyAbonos,
                Products = x.Client.Products.Select(p => new { p.Product.Type, p.Product.Price, p.Stock }).ToList(),
                Abonos = x.Client.AbonosRenewed
                    .Where(a => a.CreatedAt.Month == today.Month && a.CreatedAt.Year == today.Year)
                    .Select(a => new
                    {
                        a.ID,
                        a.Abono.Name,
                        a.Abono.Price,
                        Products = a.ProductsAvailables.Select(p => new { p.AbonoRenewalID, p.Type, p.Available }).ToList()
                    }).ToList()
            })
            .ToListAsync();

        rs.Data = new GetDealerSheetsResponse
        {
            Sheets = staticCarts.Select(cart => new DealerSheetItem
            {
                Day = cart.Day,
                ClientId = cart.ClientID,
                ClientName = cart.ClientName,
                ClientPhone = cart.ClientPhone,
                ClientAddress = cart.ClientAddress,
                ClientObservations = cart.ClientObservations,
                ClientDebt = cart.ClientDebt,
                Products = cart.OnlyAbonos ? [] : cart.Products.Select(p => new DealerSheetItem.ProductSheetItem
                {
                    Type = p.Type,
                    TypeName = p.Type.GetDisplayName(),
                    Price = p.Price,
                    Stock = p.Stock
                }).ToList(),
                Abonos = cart.Abonos.Select(a => new DealerSheetItem.AbonoSheetItem
                {
                    Id = a.ID,
                    Name = a.Name,
                    Price = a.Price
                }).ToList(),
                AbonoProducts = cart.Abonos
                    .SelectMany(a => a.Products)
                    .Where(p => p.Type != ProductType.Maquina)
                    .Select(p => new DealerSheetItem.AbonoProductSheetItem
                    {
                        AbonoId = p.AbonoRenewalID,
                        Type = p.Type,
                        TypeName = p.Type.GetDisplayName(),
                        Available = p.Available,
                        Stock = cart.Products.FirstOrDefault(y => y.Type == p.Type)?.Stock ?? 0
                    }).ToList()
            }).ToList()
        };

        return rs;
    }

    public async Task<BaseResponse<GetClientsByDayResponse>> GetClientsByDay(GetClientsByDayRequest rq)
    {
        return new BaseResponse<GetClientsByDayResponse>
        {
            Data = new GetClientsByDayResponse
            {
                Items = await _db.Clients
                    .AsNoTracking()
                    .Where(x => x.DeliveryDay == rq.Day && x.DealerID == rq.DealerId && x.IsActive)
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
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetClientsNotVisitedResponse>> GetClientsNotVisited(GetClientsNotVisitedRequest rq)
    {
        var clients = await _db.Clients
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.DealerID == rq.DealerId &&
                x.Carts.Where(y => !y.IsStatic && y.CreatedAt.Date >= rq.DateFrom.Date && y.CreatedAt.Date <= rq.DateTo.Date).All(y => y.State != State.Confirmed))
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
            .ToListAsync();

        return new BaseResponse<GetClientsNotVisitedResponse>
        {
            Data = new GetClientsNotVisitedResponse
            {
                TotalClients = await _db.Clients.CountAsync(x => x.DealerID == rq.DealerId && x.IsActive),
                TotalNotVisited = clients.Count,
                Clients = clients
            }
        };
    }

    public async Task<BaseResponse<GetDealerSoldProductsResponse>> GetSoldProducts(GetDealerSoldProductsRequest rq)
    {
        return new BaseResponse<GetDealerSoldProductsResponse>
        {
            Data = new GetDealerSoldProductsResponse
            {
                Items = await _routeService.GetSoldProductsBetweenDates(rq.DateFrom, rq.DateTo, rq.DealerId)
            }
        };
    }
}


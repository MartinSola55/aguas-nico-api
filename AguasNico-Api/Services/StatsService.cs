using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Stats;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class StatsService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetYearsResponse>> GetYears()
    {
        return new BaseResponse<GetYearsResponse>
        {
            Data = new GetYearsResponse
            {
                Years = await _db.Routes.AsNoTracking().Select(x => x.CreatedAt.Year).Distinct().OrderByDescending(x => x).ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetAnnualProfitsResponse>> GetAnnualProfits(GetAnnualProfitsRequest rq)
    {
        var cartsByMonth = await _db.Carts
            .AsNoTracking()
            .Where(x => !x.IsStatic && x.CreatedAt.Year == rq.Year)
            .Select(x => new { x.CreatedAt.Month, Profit = x.PaymentMethods.Sum(y => y.Amount) })
            .GroupBy(x => x.Month)
            .Select(x => new { Month = x.Key, Profit = x.Sum(y => y.Profit) })
            .ToListAsync();

        return new BaseResponse<GetAnnualProfitsResponse>
        {
            Data = new GetAnnualProfitsResponse
            {
                Items = [.. Enumerable.Range(1, 12)
                    .Select(month => new PeriodProfitItem
                    {
                        Period = $"{rq.Year}-{month.ToString().PadLeft(2, '0')}",
                        Sold = cartsByMonth.FirstOrDefault(x => x.Month == month)?.Profit ?? 0
                    })]
            }
        };
    }

    public async Task<BaseResponse<GetMonthlyProfitsResponse>> GetMonthlyProfits(GetMonthlyProfitsRequest rq)
    {
        var cartsByDay = await _db.Carts
            .AsNoTracking()
            .Where(x => !x.IsStatic && x.CreatedAt.Year == rq.Year && x.CreatedAt.Month == rq.Month)
            .Select(x => new { x.CreatedAt.Day, Profit = x.PaymentMethods.Sum(y => y.Amount) })
            .GroupBy(x => x.Day)
            .Select(x => new { Day = x.Key, Profit = x.Sum(y => y.Profit) })
            .ToListAsync();

        var daily = Enumerable.Range(1, DateTime.DaysInMonth(rq.Year, rq.Month))
            .Select(day => new PeriodProfitItem
            {
                Period = $"{rq.Year}-{rq.Month.ToString().PadLeft(2, '0')}-{day.ToString().PadLeft(2, '0')}",
                Sold = cartsByDay.FirstOrDefault(x => x.Day == day)?.Profit ?? 0
            })
            .ToList();

        return new BaseResponse<GetMonthlyProfitsResponse>
        {
            Data = new GetMonthlyProfitsResponse
            {
                Total = daily.Sum(x => x.Sold),
                Daily = daily
            }
        };
    }

    public async Task<BaseResponse<GetProductsSoldResponse>> GetProductsSold(GetProductsSoldRequest rq)
    {
        var products = await _db.CartProducts
            .AsNoTracking()
            .Where(x => x.Cart.CreatedAt.Year == rq.Year && x.Cart.CreatedAt.Month == rq.Month)
            .GroupBy(x => x.Type)
            .Select(x => new { Type = x.Key, Quantity = x.Sum(y => y.Quantity) })
            .ToListAsync();

        var items = Enum.GetValues<ProductType>()
            .Select(type => new ProductSoldItem
            {
                Type = type.GetDisplayName(),
                Quantity = products.FirstOrDefault(x => x.Type == type)?.Quantity ?? 0
            })
            .OrderBy(x => x.Type)
            .ToList();

        return new BaseResponse<GetProductsSoldResponse>
        {
            Data = new GetProductsSoldResponse { Items = items }
        };
    }

    public async Task<BaseResponse<GetProductsSoldByDealerResponse>> GetProductsSoldByDealer(GetProductsSoldByDealerRequest rq)
    {
        var query = _db.CartProducts
            .AsNoTracking()
            .Where(x => x.Cart.CreatedAt.Date >= rq.StartDate.Date && x.Cart.CreatedAt.Date <= rq.EndDate.Date);

        if (!string.IsNullOrEmpty(rq.DealerId))
            query = query.Where(x => x.Cart.Route.UserID == rq.DealerId);

        var raw = await query
            .GroupBy(x => new { x.Cart.Route.UserID, x.Cart.Route.User.Name, x.Type })
            .Select(g => new { g.Key.UserID, g.Key.Name, g.Key.Type, Quantity = g.Sum(y => y.Quantity) })
            .ToListAsync();

        var items = raw
            .GroupBy(x => new { x.UserID, x.Name })
            .Select(g => new DealerProductsSoldItem
            {
                DealerId = g.Key.UserID,
                DealerName = g.Key.Name,
                Quantity = g.Sum(y => y.Quantity),
                Products = g
                    .Select(y => new ProductSoldItem { Type = y.Type.GetDisplayName(), Quantity = y.Quantity })
                    .OrderByDescending(p => p.Quantity)
                    .ToList()
            })
            .OrderByDescending(x => x.Quantity)
            .ToList();

        return new BaseResponse<GetProductsSoldByDealerResponse>
        {
            Data = new GetProductsSoldByDealerResponse { Items = items }
        };
    }

    public async Task<BaseResponse<GetBalanceByDateResponse>> GetBalanceByDate(GetBalanceByDateRequest rq)
    {
        var cartPaymentMethods = await _db.CartPaymentMethods.Where(x => x.CreatedAt.Date == rq.Date.Date).SumAsync(x => x.Amount);
        var transfers = await _db.Transfers.Where(x => x.Date.Date == rq.Date.Date).SumAsync(x => x.Amount);
        var expenses = await _db.Expenses.Where(x => x.CreatedAt.Date == rq.Date.Date).SumAsync(x => x.Amount);
        var dispenserPrice = await _db.Routes.Where(x => x.CreatedAt.Date == rq.Date.Date).SumAsync(x => x.DispenserPrice);

        return new BaseResponse<GetBalanceByDateResponse>
        {
            Data = new GetBalanceByDateResponse
            {
                Total = cartPaymentMethods + transfers + dispenserPrice - expenses,
                CartPaymentMethods = cartPaymentMethods,
                Transfers = transfers,
                Expenses = expenses,
                DispenserPrice = dispenserPrice
            }
        };
    }
}


using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Home;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class HomeService(APIContext context, TokenService tokenService)
{
    private readonly APIContext _db = context;
    private readonly Token _token = tokenService.GetToken();

    public async Task<BaseResponse<GetDashboardResponse>> GetDashboard(GetDashboardRequest rq)
    {
        return new BaseResponse<GetDashboardResponse>
        {
            Data = new GetDashboardResponse
            {
                DealerRoutes = await _db
                    .Routes
                    .AsNoTracking()
                    .Where(x => x.UserID == _token.UserId && !x.IsStatic && x.DayOfWeek == rq.Day)
                    .Select(x => new Models.DTO.Routes.RouteItem
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
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync()
            }
        };
    }
}


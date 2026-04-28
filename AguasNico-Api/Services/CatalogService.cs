using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Catalog;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class CatalogService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetCatalogResponse>> GetAll()
    {
        return new BaseResponse<GetCatalogResponse>
        {
            Data = new GetCatalogResponse
            {
                States = Enum.GetValues<State>().Select(x => new EnumItem<int> { Id = (int)x, Description = x.GetDisplayName() }).ToList(),
                ProductTypes = Enum.GetValues<ProductType>().Select(x => new EnumItem<int> { Id = (int)x, Description = x.GetDisplayName() }).ToList(),
                Days = Enum.GetValues<Day>().Select(x => new EnumItem<int> { Id = (int)x, Description = x.ToString() }).ToList(),
                InvoiceTypes = Enum.GetValues<InvoiceType>().Select(x => new EnumItem<int> { Id = (int)x, Description = x.GetDisplayName() }).ToList(),
                TaxConditions = Enum.GetValues<TaxCondition>().Select(x => new EnumItem<int> { Id = (int)x, Description = x.GetDisplayName() }).ToList(),
                PaymentMethods = await _db.PaymentMethods.AsNoTracking().Select(x => new PaymentMethodCatalogItem
                {
                    Id = x.ID,
                    Description = x.Name
                }).ToListAsync()
            }
        };
    }
}


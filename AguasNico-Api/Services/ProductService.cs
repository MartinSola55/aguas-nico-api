using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Common;
using AguasNico_Api.Models.DTO.Products;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class ProductService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetAllProductsResponse>> GetAll(GetAllProductsRequest rq)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();
        if (rq.ActiveOnly)
            query = query.Where(x => x.IsActive);

        return new BaseResponse<GetAllProductsResponse>
        {
            Data = new GetAllProductsResponse
            {
                Items = await query
                    .Select(x => new ProductItem
                    {
                        Id = x.ID,
                        Name = x.Name,
                        Price = x.Price,
                        Type = x.Type,
                        TypeName = x.Type.GetDisplayName(),
                        SortOrder = x.SortOrder,
                        IsActive = x.IsActive
                    })
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetProductResponse>> GetOne(GetProductRequest rq)
    {
        var rs = new BaseResponse<GetProductResponse>();

        var product = await _db.Products
            .AsNoTracking()
            .Where(x => x.ID == rq.Id)
            .Select(x => new GetProductResponse
            {
                Id = x.ID,
                Name = x.Name,
                Price = x.Price,
                Type = x.Type,
                TypeName = x.Type.GetDisplayName(),
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return rs.SetError(Messages.Error.EntityNotFound("Producto"));

        rs.Data = product;
        return rs;
    }

    public async Task<BaseResponse<CreateProductResponse>> Create(CreateProductRequest rq)
    {
        var rs = new BaseResponse<CreateProductResponse>();
        if (!rs.Attach(await ValidateProduct<CreateProductResponse>(rq.Name, rq.Price, rq.Type)).Success)
            return rs;

        var product = new Product
        {
            Name = rq.Name!.Trim(),
            Price = rq.Price,
            Type = rq.Type,
            SortOrder = rq.SortOrder,
        };

        _db.Products.Add(product);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new CreateProductResponse { Id = product.ID };
        rs.Message = Messages.CRUD.EntityCreated("Producto");
        return rs;
    }

    public async Task<BaseResponse<UpdateProductResponse>> Update(UpdateProductRequest rq)
    {
        var rs = new BaseResponse<UpdateProductResponse>();
        var product = await _db.Products.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (product == null)
            return rs.SetError(Messages.Error.EntityNotFound("Producto"));

        if (!rs.Attach(await ValidateProduct<UpdateProductResponse>(rq.Name, rq.Price, rq.Type, rq.Id)).Success)
            return rs;

        product.Name = rq.Name!.Trim();
        product.Price = rq.Price;
        product.Type = rq.Type;
        product.SortOrder = rq.SortOrder;
        product.UpdatedAt = LocalClock.Now;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            return rs.SetError(Messages.Error.Exception());
        }

        rs.Data = new UpdateProductResponse { Id = product.ID };
        rs.Message = Messages.CRUD.EntityUpdated("Producto");
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteProductRequest rq)
    {
        var rs = new BaseResponse();
        var product = await _db.Products.FirstOrDefaultAsync(x => x.ID == rq.Id);
        if (product == null)
            return rs.SetError(Messages.Error.EntityNotFound("Producto"));

        product.IsActive = false;
        var clientProducts = await _db.ClientProducts.Where(x => x.ProductID == rq.Id).ToListAsync();
        foreach (var clientProduct in clientProducts)
            clientProduct.DeletedAt = LocalClock.Now;

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

        rs.Message = Messages.CRUD.EntityDeleted("Producto");
        return rs;
    }

    public async Task<BaseResponse<GetProductClientsResponse>> GetClients(GetProductClientsRequest rq)
    {
        return new BaseResponse<GetProductClientsResponse>
        {
            Data = new GetProductClientsResponse
            {
                Items = await _db.ClientProducts
                    .AsNoTracking()
                    .Where(x => x.ProductID == rq.ProductId && x.Client.IsActive)
                    .Select(x => new ClientSummaryItem
                    {
                        Id = x.Client.ID,
                        Name = x.Client.Name,
                        Address = x.Client.Address,
                        Phone = x.Client.Phone,
                        Email = x.Client.Email ?? "",
                        DealerId = x.Client.DealerID ?? "",
                        DealerName = x.Client.Dealer != null ? x.Client.Dealer.Name : "",
                        DeliveryDay = x.Client.DeliveryDay ?? 0,
                        Debt = x.Client.Debt,
                        HasInvoice = x.Client.HasInvoice,
                        OnlyAbonos = x.Client.OnlyAbonos,
                        IsActive = x.Client.IsActive
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            }
        };
    }

    public async Task<BaseResponse<GetProductStatsResponse>> GetStats(GetProductStatsRequest rq)
    {
        var rs = new BaseResponse<GetProductStatsResponse>();
        var year = rq.Year == 0 ? LocalClock.Now.Year : rq.Year;

        var product = await _db.Products
            .AsNoTracking()
            .Where(x => x.ID == rq.Id)
            .Select(x => new ProductItem
            {
                Id = x.ID,
                Name = x.Name,
                Price = x.Price,
                Type = x.Type,
                TypeName = x.Type.GetDisplayName(),
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return rs.SetError(Messages.Error.EntityNotFound("Producto"));

        var salesByMonth = await _db.CartProducts
            .AsNoTracking()
            .Where(x => x.Type == product.Type && x.CreatedAt.Year == year)
            .GroupBy(x => x.CreatedAt.Month)
            .Select(x => new { Month = x.Key, Total = x.Sum(y => y.Quantity) })
            .ToListAsync();

        var annualSales = new int[12];
        foreach (var sale in salesByMonth)
            annualSales[sale.Month - 1] = sale.Total;

        rs.Data = new GetProductStatsResponse
        {
            Product = product,
            ClientStock = await _db.ClientProducts.Where(x => x.ProductID == rq.Id && x.Client.IsActive).SumAsync(x => x.Stock),
            TotalSold = await _db.CartProducts.Where(x => x.Type == product.Type && x.CreatedAt.Year == year).SumAsync(x => x.Quantity * x.SettedPrice),
            AnnualSales = annualSales
        };

        return rs;
    }

    private async Task<BaseResponse<T>> ValidateProduct<T>(string? name, decimal price, ProductType type, long id = 0)
    {
        var rs = new BaseResponse<T>();

        if (string.IsNullOrWhiteSpace(name))
            return rs.SetError(Messages.Error.FieldRequired("nombre"));

        if (name.Length > 200)
            return rs.SetError(Messages.Error.InvalidField("nombre"));

        if (price < 0 || price > 1000000)
            return rs.SetError(Messages.Error.InvalidField("precio"));

        if (type == 0)
            return rs.SetError(Messages.Error.InvalidField("tipo de producto"));

        if (await _db.Products.AnyAsync(x => x.Name == name.Trim() && x.Price == price && x.IsActive && x.ID != id))
            return rs.SetError("Ya existe uno con el mismo nombre y precio");

        return rs;
    }

    private async Task RollbackIfNeeded()
    {
        if (_db.Database.CurrentTransaction != null)
            await _db.Database.RollbackTransactionAsync();
    }
}


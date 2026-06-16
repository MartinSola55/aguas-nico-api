using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Carts;
using AguasNico_Api.Models.DTO.Common;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class CartService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetCartForEditResponse>> GetForEdit(GetCartForEditRequest rq)
    {
        var rs = new BaseResponse<GetCartForEditResponse>();
        var cart = await _db.Carts
            .AsNoTracking()
            .Where(x => x.ID == rq.Id)
            .Select(x => new
            {
                x.ID,
                x.ClientID,
                x.RouteID,
                x.State,
                ClientName = x.Client.Name
            })
            .FirstOrDefaultAsync();

        if (cart == null)
            return rs.SetError(Messages.Error.EntityNotFound("Bajada", true));
        if (cart.State != State.Confirmed)
            return rs.SetError("No se puede editar una bajada que no esté confirmada");

        var clientProducts = await _db.ClientProducts
            .AsNoTracking()
            .Where(x => x.ClientID == cart.ClientID && x.Product.Type != ProductType.Maquina)
            .Select(x => new { x.Product.Type, x.Product.Name, x.Product.Price })
            .ToListAsync();

        var cartProducts = await _db.CartProducts.AsNoTracking().Where(x => x.CartID == cart.ID).ToListAsync();
        var cartAbonoProducts = await _db.CartAbonoProducts.AsNoTracking().Where(x => x.CartID == cart.ID).ToListAsync();
        var returnedProducts = await _db.ReturnedProducts.AsNoTracking().Where(x => x.CartID == cart.ID).ToListAsync();
        var selectedPaymentMethods = await _db.CartPaymentMethods.AsNoTracking().Where(x => x.CartID == cart.ID).ToListAsync();
        var paymentMethods = await _db.PaymentMethods.AsNoTracking().ToListAsync();

        var abonoTypes = await _db.ClientAbonos
            .AsNoTracking()
            .Where(x => x.ClientID == cart.ClientID)
            .SelectMany(x => x.Abono.Products.Select(p => p.Type))
            .Where(x => x != ProductType.Maquina)
            .Distinct()
            .ToListAsync();

        rs.Data = new GetCartForEditResponse
        {
            Id = cart.ID,
            ClientId = cart.ClientID,
            RouteId = cart.RouteID,
            ClientName = cart.ClientName,
            State = cart.State,
            Products = [.. clientProducts.Select(x =>
            {
                var selected = cartProducts.FirstOrDefault(p => p.Type == x.Type);
                return new ProductQuantityItem
                {
                    Type = x.Type,
                    TypeName = x.Type.GetDisplayName(),
                    Quantity = selected?.Quantity ?? 0,
                    SettedPrice = selected?.SettedPrice ?? x.Price
                };
            })],
            AbonoProducts = [.. abonoTypes.Select(x =>
            {
                var selected = cartAbonoProducts.FirstOrDefault(p => p.Type == x);
                return new ProductQuantityItem
                {
                    Type = x,
                    TypeName = x.GetDisplayName(),
                    Quantity = selected?.Quantity ?? 0
                };
            })],
            ReturnedProducts = [.. clientProducts.Select(x =>
            {
                var selected = returnedProducts.FirstOrDefault(p => p.Type == x.Type);
                return new ProductQuantityItem
                {
                    Type = x.Type,
                    TypeName = x.Type.GetDisplayName(),
                    Quantity = selected?.Quantity ?? 0
                };
            })],
            PaymentMethods = [.. paymentMethods.Select(x =>
            {
                var selected = selectedPaymentMethods.FirstOrDefault(p => p.PaymentMethodID == x.ID);
                return new GetCartForEditResponse.PaymentMethodOptionItem
                {
                    Id = x.ID,
                    Name = x.Name,
                    Selected = selected != null,
                    Amount = selected?.Amount ?? 0
                };
            })]
        };

        return rs;
    }

    public async Task<BaseResponse> Update(UpdateCartRequest rq)
    {
        var rs = new BaseResponse();
        try
        {
            await _db.Database.BeginTransactionAsync();
            await SoftDeleteEffects(rq.Id);
            await ApplyCart(rq.Id, rq.ClientId, rq.Products, rq.AbonoProducts, rq.ReturnedProducts, rq.PaymentMethods, useCartCreatedAtForAbonos: true);

            var cart = await _db.Carts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ID == rq.Id);
            if (cart == null)
                return rs.SetError(Messages.Error.EntityNotFound("Bajada", true));
            cart.DeletedAt = null;
            cart.UpdatedAt = LocalClock.Now;

            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackIfNeeded();
            return rs.SetError(ex.Message);
        }

        rs.Message = Messages.CRUD.EntityUpdated("Bajada", true);
        return rs;
    }

    public async Task<BaseResponse> Confirm(ConfirmCartRequest rq)
    {
        var rs = new BaseResponse();
        var cartState = await _db.Carts.AsNoTracking().Where(x => x.ID == rq.Id).Select(x => x.State).FirstOrDefaultAsync();
        if (cartState != State.Pending)
            return rs.SetError("La bajada ya ha sido efectuada. Recargue la página y vuelva a intentar");

        try
        {
            await _db.Database.BeginTransactionAsync();
            await ApplyConfirm(rq.Id, rq.ClientId, rq.Products, rq.AbonoProducts, rq.PaymentMethods);

            var cart = await _db.Carts.FirstOrDefaultAsync(x => x.ID == rq.Id);
            if (cart == null)
                return rs.SetError(Messages.Error.EntityNotFound("Bajada", true));
            cart.State = State.Confirmed;
            cart.UpdatedAt = LocalClock.Now;

            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackIfNeeded();
            return rs.SetError(ex.Message);
        }

        rs.Message = "Se ha confirmado la bajada";
        return rs;
    }

    public async Task<BaseResponse<ConfirmManualCartResponse>> ConfirmManual(ConfirmManualCartRequest rq)
    {
        var rs = new BaseResponse<ConfirmManualCartResponse>();
        var existing = await _db.Carts.AsNoTracking().FirstOrDefaultAsync(x => x.RouteID == rq.RouteId && x.ClientID == rq.ClientId && !x.IsStatic);
        if (existing != null && existing.State != State.Pending)
            return rs.SetError("La bajada ya ha sido efectuada. Recargue la página y vuelva a intentar");

        var maxPriority = await _db.Carts.Where(x => x.RouteID == rq.RouteId).Select(x => (int?)x.Priority).MaxAsync() ?? 0;
        var cart = new Cart
        {
            ClientID = rq.ClientId,
            RouteID = rq.RouteId,
            State = State.Confirmed,
            IsStatic = false,
            Priority = maxPriority + 1,
            Products = [.. rq.Products.Where(x => x.Quantity > 0).Select(x => new CartProduct { Type = x.Type, Quantity = x.Quantity })],
            AbonoProducts = [.. rq.AbonoProducts.Where(x => x.Quantity > 0).Select(x => new CartAbonoProduct { Type = x.Type, Quantity = x.Quantity })],
            PaymentMethods = [.. rq.PaymentMethods.Select(x => new CartPaymentMethod { PaymentMethodID = x.PaymentMethodId, Amount = x.Amount })]
        };

        try
        {
            _db.Carts.Add(cart);
            await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await ApplyConfirm(cart.ID, rq.ClientId, rq.Products, rq.AbonoProducts, rq.PaymentMethods, cart);
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackIfNeeded();
            return rs.SetError(ex.Message);
        }

        rs.Data = new ConfirmManualCartResponse { CartId = cart.ID, ClientId = cart.ClientID };
        rs.Message = "Se ha confirmado la bajada";
        return rs;
    }

    public async Task<BaseResponse> SetState(SetCartStateRequest rq)
    {
        var rs = new BaseResponse();
        var cart = await _db.Carts.FirstOrDefaultAsync(x => x.ID == rq.CartId);
        if (cart == null)
            return rs.SetError(Messages.Error.EntityNotFound("Bajada", true));

        cart.State = rq.State;
        cart.UpdatedAt = LocalClock.Now;
        await _db.SaveChangesAsync();
        rs.Message = "Se ha actualizado el estado de la bajada";
        return rs;
    }

    public async Task<BaseResponse> ResetState(ResetCartStateRequest rq)
    {
        return await SetState(new SetCartStateRequest { CartId = rq.CartId, State = State.Pending });
    }

    public async Task<BaseResponse<GetReturnedProductsResponse>> GetReturnedProducts(GetReturnedProductsRequest rq)
    {
        var rs = new BaseResponse<GetReturnedProductsResponse>();
        var cart = await _db.Carts.AsNoTracking().Where(x => x.ID == rq.CartId).Select(x => new { x.ID, x.ClientID }).FirstOrDefaultAsync();
        if (cart == null)
            return rs.SetError(Messages.Error.EntityNotFound("Bajada", true));

        var returnedProducts = await _db.ReturnedProducts.AsNoTracking().Where(x => x.CartID == rq.CartId).ToListAsync();
        var clientProducts = await _db.ClientProducts.AsNoTracking().Where(x => x.ClientID == cart.ClientID).Select(x => x.Product.Type).ToListAsync();

        rs.Data = new GetReturnedProductsResponse
        {
            Items = [.. clientProducts.Select(type =>
            {
                var returned = returnedProducts.FirstOrDefault(x => x.Type == type);
                return new ProductQuantityItem
                {
                    Type = type,
                    TypeName = type.GetDisplayName(),
                    Quantity = returned?.Quantity ?? 0
                };
            })]
        };

        return rs;
    }

    public async Task<BaseResponse> ReturnProducts(ReturnProductsRequest rq)
    {
        var rs = new BaseResponse();
        var cart = await _db.Carts.Include(x => x.ReturnedProducts).FirstOrDefaultAsync(x => x.ID == rq.CartId);
        if (cart == null)
            return rs.SetError(Messages.Error.EntityNotFound("Bajada", true));

        var client = await _db.Clients.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.ID == cart.ClientID);
        if (client == null)
            return rs.SetError(Messages.Error.EntityNotFound("Cliente"));

        try
        {
            await _db.Database.BeginTransactionAsync();

            foreach (var product in cart.ReturnedProducts)
            {
                var clientProduct = client.Products.First(x => x.Product.Type == product.Type);
                clientProduct.Stock += product.Quantity;
                product.DeletedAt = LocalClock.Now;
            }

            foreach (var product in rq.Products)
            {
                if (product.Quantity <= 0)
                    continue;

                var clientProduct = client.Products.First(x => x.Product.Type == product.Type);
                if (clientProduct.Stock < product.Quantity)
                    throw new Exception("El cliente no posee stock suficiente de: " + product.Type.GetDisplayName());

                clientProduct.Stock -= product.Quantity;

                var existing = await _db.ReturnedProducts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CartID == cart.ID && x.Type == product.Type);
                if (existing != null)
                {
                    existing.Quantity = product.Quantity;
                    existing.UpdatedAt = LocalClock.Now;
                    existing.DeletedAt = null;
                }
                else
                {
                    _db.ReturnedProducts.Add(new ReturnedProduct
                    {
                        Cart = cart,
                        Type = product.Type,
                        Quantity = product.Quantity
                    });
                }
            }

            cart.UpdatedAt = LocalClock.Now;
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackIfNeeded();
            return rs.SetError(ex.Message);
        }

        rs.Message = "Se han devuelto los productos";
        return rs;
    }

    public async Task<BaseResponse> Delete(DeleteCartRequest rq)
    {
        var rs = new BaseResponse();
        try
        {
            await _db.Database.BeginTransactionAsync();
            await SoftDeleteEffects(rq.CartId);
            await _db.SaveChangesAsync();
            await _db.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackIfNeeded();
            return rs.SetError(ex.Message);
        }

        rs.Message = Messages.CRUD.EntityDeleted("Bajada", true);
        return rs;
    }

    private async Task ApplyCart(
        long cartId,
        long clientId,
        List<CartProductRequestItem> products,
        List<CartProductRequestItem> abonoProducts,
        List<ReturnedProductRequestItem> returnedProducts,
        List<CartPaymentRequestItem> paymentMethods,
        bool useCartCreatedAtForAbonos)
    {
        var client = await _db.Clients.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.ID == clientId)
            ?? throw new Exception("No se ha encontrado el cliente");
        var cart = await _db.Carts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ID == cartId)
            ?? throw new Exception("No se ha encontrado la bajada");
        var abonoDate = useCartCreatedAtForAbonos ? cart.CreatedAt : LocalClock.Now;

        foreach (var product in products.Where(x => x.Quantity > 0))
        {
            var clientProduct = client.Products.First(x => x.Product.Type == product.Type);
            clientProduct.Stock += product.Quantity;
            client.Debt += product.Quantity * clientProduct.Product.Price;

            var existing = await _db.CartProducts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CartID == cartId && x.Type == product.Type);
            if (existing != null)
            {
                existing.Quantity = product.Quantity;
                existing.SettedPrice = clientProduct.Product.Price;
                existing.UpdatedAt = LocalClock.Now;
                existing.DeletedAt = null;
            }
            else
            {
                _db.CartProducts.Add(new CartProduct { CartID = cartId, Type = product.Type, Quantity = product.Quantity, SettedPrice = clientProduct.Product.Price });
            }
        }

        foreach (var product in abonoProducts.Where(x => x.Quantity > 0))
        {
            var clientProduct = client.Products.First(x => x.Product.Type == product.Type);
            clientProduct.Stock += product.Quantity;

            await ConsumeAbonoProducts(clientId, product.Type, product.Quantity, abonoDate);

            var existing = await _db.CartAbonoProducts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CartID == cartId && x.Type == product.Type);
            if (existing != null)
            {
                existing.Quantity = product.Quantity;
                existing.UpdatedAt = LocalClock.Now;
                existing.DeletedAt = null;
            }
            else
            {
                _db.CartAbonoProducts.Add(new CartAbonoProduct { CartID = cartId, Type = product.Type, Quantity = product.Quantity });
            }
        }

        foreach (var product in returnedProducts.Where(x => x.Quantity > 0))
        {
            var clientProduct = client.Products.First(x => x.Product.Type == product.Type);
            if (clientProduct.Stock < product.Quantity)
                throw new Exception("El cliente no posee stock suficiente de: " + product.Type.GetDisplayName());

            clientProduct.Stock -= product.Quantity;

            var existing = await _db.ReturnedProducts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CartID == cartId && x.Type == product.Type);
            if (existing != null)
            {
                existing.Quantity = product.Quantity;
                existing.UpdatedAt = LocalClock.Now;
                existing.DeletedAt = null;
            }
            else
            {
                _db.ReturnedProducts.Add(new ReturnedProduct { CartID = cartId, Type = product.Type, Quantity = product.Quantity });
            }
        }

        foreach (var paymentMethod in paymentMethods)
        {
            client.Debt -= paymentMethod.Amount;

            var existing = await _db.CartPaymentMethods.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CartID == cartId && x.PaymentMethodID == paymentMethod.PaymentMethodId);
            if (existing != null)
            {
                existing.Amount = paymentMethod.Amount;
                existing.UpdatedAt = LocalClock.Now;
                existing.DeletedAt = null;
            }
            else
            {
                _db.CartPaymentMethods.Add(new CartPaymentMethod { CartID = cartId, PaymentMethodID = paymentMethod.PaymentMethodId, Amount = paymentMethod.Amount });
            }
        }
    }

    private async Task ApplyConfirm(long cartId, long clientId, List<CartProductRequestItem> products, List<CartProductRequestItem> abonoProducts, List<CartPaymentRequestItem> paymentMethods, Cart newCart = null)
    {
        var client = await _db.Clients.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.ID == clientId)
            ?? throw new Exception("No se ha encontrado el cliente");
        var returnedProducts = Enum.GetValues<ProductType>().Select(x => new ReturnedProduct { CartID = cartId, Cart = newCart!, Type = x, Quantity = 0 }).ToList();

        decimal total = 0;
        foreach (var product in products.Where(x => x.Quantity > 0))
        {
            var clientProduct = client.Products.FirstOrDefault(x => x.Product.Type == product.Type)
                ?? throw new Exception("No se ha encontrado un producto del cliente");

            clientProduct.Stock += product.Quantity;
            var settedPrice = clientProduct.Product.Price;
            total += product.Quantity * settedPrice;

            if (newCart == null)
                _db.CartProducts.Add(new CartProduct { CartID = cartId, Type = product.Type, Quantity = product.Quantity, SettedPrice = settedPrice });
            else
                newCart.Products.First(x => x.Type == product.Type).SettedPrice = settedPrice;

            returnedProducts.First(x => x.Type == product.Type).Quantity += product.Quantity;
        }
        client.Debt += total;

        foreach (var product in abonoProducts.Where(x => x.Quantity > 0))
        {
            await ConsumeAbonoProducts(clientId, product.Type, product.Quantity, LocalClock.Now);

            var clientProduct = client.Products.FirstOrDefault(x => x.Product.Type == product.Type)
                ?? throw new Exception("No se ha encontrado un producto del cliente");
            clientProduct.Stock += product.Quantity;

            if (newCart == null)
                _db.CartAbonoProducts.Add(new CartAbonoProduct { CartID = cartId, Type = product.Type, Quantity = product.Quantity });

            returnedProducts.First(x => x.Type == product.Type).Quantity += product.Quantity;
        }

        total = 0;
        foreach (var paymentMethod in paymentMethods)
        {
            total += paymentMethod.Amount;
            if (newCart == null)
                _db.CartPaymentMethods.Add(new CartPaymentMethod { CartID = cartId, PaymentMethodID = paymentMethod.PaymentMethodId, Amount = paymentMethod.Amount });
        }
        client.Debt -= total;

        foreach (var product in returnedProducts.Where(x => x.Quantity > 0))
        {
            var clientProduct = client.Products.FirstOrDefault(x => x.Product.Type == product.Type)
                ?? throw new Exception("No se ha encontrado un producto del cliente");
            clientProduct.Stock -= product.Quantity;

            if (newCart == null)
                _db.ReturnedProducts.Add(product);
            else
                newCart.ReturnedProducts.Add(new ReturnedProduct { Type = product.Type, Quantity = product.Quantity });
        }
    }

    private async Task SoftDeleteEffects(long id)
    {
        var cart = await _db.Carts
            .Include(x => x.Client)
            .Include(x => x.Products)
            .Include(x => x.AbonoProducts)
            .Include(x => x.ReturnedProducts)
            .Include(x => x.PaymentMethods)
            .FirstOrDefaultAsync(x => x.ID == id) ?? throw new Exception("No se ha encontrado la bajada");

        foreach (var product in cart.Products)
        {
            var clientProduct = await _db.ClientProducts.FirstOrDefaultAsync(x => x.ClientID == cart.ClientID && x.Product.Type == product.Type);
            if (clientProduct != null)
                clientProduct.Stock -= product.Quantity;
            cart.Client.Debt -= product.Quantity * product.SettedPrice;
            product.DeletedAt = LocalClock.Now;
        }

        foreach (var abonoProductInCart in cart.AbonoProducts)
        {
            var abonoProduct = await _db.AbonoRenewalProducts.FirstOrDefaultAsync(x =>
                x.AbonoRenewal.ClientID == cart.ClientID &&
                x.CreatedAt.Month == cart.CreatedAt.Month &&
                x.CreatedAt.Year == cart.CreatedAt.Year &&
                x.Type == abonoProductInCart.Type) ?? throw new Exception("No se ha encontrado un producto del abono del cliente");

            abonoProduct.Available += abonoProductInCart.Quantity;

            var clientProduct = await _db.ClientProducts.FirstOrDefaultAsync(x => x.ClientID == cart.ClientID && x.Product.Type == abonoProductInCart.Type);
            if (clientProduct != null)
                clientProduct.Stock -= abonoProductInCart.Quantity;

            abonoProductInCart.DeletedAt = LocalClock.Now;
        }

        foreach (var product in cart.ReturnedProducts)
        {
            var clientProduct = await _db.ClientProducts.FirstOrDefaultAsync(x => x.ClientID == cart.ClientID && x.Product.Type == product.Type)
                ?? throw new Exception("No se ha encontrado un producto del cliente");
            clientProduct.Stock += product.Quantity;
            product.DeletedAt = LocalClock.Now;
        }

        foreach (var paymentMethod in cart.PaymentMethods)
        {
            cart.Client.Debt += paymentMethod.Amount;
            paymentMethod.DeletedAt = LocalClock.Now;
        }

        cart.DeletedAt = LocalClock.Now;
    }

    public async Task SoftDeleteEffectsInCurrentTransaction(long id)
    {
        await SoftDeleteEffects(id);
    }

    private async Task ConsumeAbonoProducts(long clientId, ProductType type, int quantity, DateTime date)
    {
        var abonoProducts = await _db.AbonoRenewalProducts
            .Where(x => x.AbonoRenewal.ClientID == clientId && x.CreatedAt.Month == date.Month && x.CreatedAt.Year == date.Year && x.Type == type)
            .ToListAsync();

        if (abonoProducts.Count == 0)
            throw new Exception("No se ha encontrado un producto del abono del cliente");
        if (abonoProducts.Sum(x => x.Available) < quantity)
            throw new Exception("El cliente no posee stock suficiente de: " + type.GetDisplayName());

        var pending = quantity;
        foreach (var abonoProduct in abonoProducts)
        {
            if (abonoProduct.Available >= pending)
            {
                abonoProduct.Available -= pending;
                break;
            }

            pending -= abonoProduct.Available;
            abonoProduct.Available = 0;
        }
    }

    private async Task RollbackIfNeeded()
    {
        if (_db.Database.CurrentTransaction != null)
            await _db.Database.RollbackTransactionAsync();
    }
}


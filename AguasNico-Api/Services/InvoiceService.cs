using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Invoices;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AguasNico_Api.Services;

public class InvoiceService(APIContext context)
{
    private readonly APIContext _db = context;

    public async Task<BaseResponse<GetInvoicesResponse>> GetInvoices(GetInvoicesRequest rq)
    {
        var clients = await GetInvoiceClientsQuery(rq.InvoiceDay, rq.InvoiceDealer)
            .Select(x => new { x.ID, x.Name, x.Address, x.CUIT })
            .ToListAsync();

        var clientIds = clients.Select(x => x.ID).ToList();
        var cartProducts = await _db.CartProducts
            .AsNoTracking()
            .Where(x =>
                clientIds.Contains(x.Cart.ClientID) &&
                x.CreatedAt.Date >= rq.StartDate.Date &&
                x.CreatedAt.Date <= rq.EndDate.Date &&
                x.SettedPrice > 0)
            .Select(x => new { x.Cart.ClientID, x.Type, x.Quantity, x.SettedPrice })
            .ToListAsync();

        var abonoRenewals = await _db.AbonoRenewals
            .AsNoTracking()
            .Where(x =>
                clientIds.Contains(x.ClientID) &&
                x.CreatedAt.Date >= rq.StartDate.Date &&
                x.CreatedAt.Date <= rq.EndDate.Date)
            .Select(x => new { x.ClientID, AbonoName = x.Abono.Name, x.SettedPrice, x.CreatedAt })
            .ToListAsync();

        var invoices = new List<InvoiceItem>();
        foreach (var client in clients)
        {
            var products = cartProducts
                .Where(x => x.ClientID == client.ID)
                .GroupBy(x => x.Type)
                .Select(group => new InvoiceProductItem
                {
                    Type = group.Key.GetDisplayName(),
                    Quantity = group.Sum(x => x.Quantity),
                    Total = group.Sum(x => x.SettedPrice * x.Quantity)
                })
                .ToList();

            products.AddRange(abonoRenewals
                .Where(x => x.ClientID == client.ID)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new InvoiceProductItem
                {
                    Type = x.AbonoName,
                    Quantity = 1,
                    Total = x.SettedPrice
                }));

            if (products.Count == 0)
                continue;

            invoices.Add(new InvoiceItem
            {
                ClientId = client.ID,
                ClientName = client.Name,
                ClientAddress = client.Address,
                ClientCuit = client.CUIT ?? "",
                Products = products
            });
        }

        return new BaseResponse<GetInvoicesResponse>
        {
            Data = new GetInvoicesResponse
            {
                Items = invoices
            }
        };
    }

    public async Task<BaseResponse<GetInvoicesCsvResponse>> GetCsvRows(GetInvoicesCsvRequest rq)
    {
        return new BaseResponse<GetInvoicesCsvResponse>
        {
            Data = new GetInvoicesCsvResponse
            {
                Rows = await BuildCsvRows(rq)
            }
        };
    }

    public async Task<byte[]> BuildCsv(GetInvoicesCsvRequest rq)
    {
        var rows = await BuildCsvRows(rq);
        var sb = new StringBuilder();
        sb.AppendLine("external_id,cuit_cliente,punto_venta,tipo_comprobante,neto,iva_alicuota,total,tax_condition_type_id,client_name,client_address,description,email");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", [
                EscapeCsvField(row.ExternalId),
                EscapeCsvField(row.ClientCuit),
                BusinessConstants.InvoiceSalesPoint.ToString(),
                EscapeCsvField(row.InvoiceTypeId),
                ((int)row.Neto).ToString(),
                row.IvaRate.ToString(),
                ((int)row.Total).ToString(),
                row.TaxConditionTypeId.ToString(),
                EscapeCsvField(row.ClientName),
                EscapeCsvField(row.ClientAddress),
                EscapeCsvField(row.Description),
                EscapeCsvField(row.Email),
            ]));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private async Task<List<InvoiceCsvRowItem>> BuildCsvRows(GetInvoicesRequest rq)
    {
        var clients = await GetInvoiceClientsQuery(rq.InvoiceDay, rq.InvoiceDealer)
            .Select(x => new
            {
                x.ID,
                x.Name,
                x.Address,
                x.Email,
                x.CUIT,
                x.InvoiceType,
                x.TaxCondition,
            })
            .ToListAsync();

        var clientIds = clients.Select(x => x.ID).ToList();

        var cartProducts = await _db.CartProducts
            .AsNoTracking()
            .Where(x =>
                clientIds.Contains(x.Cart.ClientID) &&
                x.CreatedAt.Date >= rq.StartDate.Date &&
                x.CreatedAt.Date <= rq.EndDate.Date &&
                x.SettedPrice > 0)
            .Select(x => new { x.Cart.ClientID, x.Type, x.Quantity, x.SettedPrice })
            .ToListAsync();

        var abonoRenewals = await _db.AbonoRenewals
            .AsNoTracking()
            .Where(x =>
                clientIds.Contains(x.ClientID) &&
                x.CreatedAt.Date >= rq.StartDate.Date &&
                x.CreatedAt.Date <= rq.EndDate.Date)
            .Select(x => new { x.ClientID, AbonoName = x.Abono.Name, x.SettedPrice, x.CreatedAt })
            .ToListAsync();

        var result = new List<InvoiceCsvRowItem>();
        foreach (var client in clients)
        {
            var products = cartProducts
                .Where(x => x.ClientID == client.ID)
                .GroupBy(x => x.Type)
                .Select(group => new InvoiceProductCsv
                {
                    Type = group.Key.GetDisplayName(),
                    Quantity = group.Sum(x => x.Quantity),
                    Subtotal = group.Sum(x => x.SettedPrice * x.Quantity)
                })
                .ToList();

            products.AddRange(abonoRenewals
                .Where(x => x.ClientID == client.ID)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new InvoiceProductCsv
                {
                    Type = x.AbonoName,
                    Quantity = 1,
                    Subtotal = x.SettedPrice
                }));

            if (products.Count == 0)
                continue;

            var total = products.Sum(x => x.Subtotal);
            result.Add(new InvoiceCsvRowItem
            {
                ExternalId = $"SLN-{client.ID}-{DateTime.Now:yyyyMMddHHmmss}",
                ClientCuit = client.CUIT ?? "",
                InvoiceTypeId = client.InvoiceType == InvoiceType.A ? "1" : client.InvoiceType == InvoiceType.B ? "6" : "",
                Neto = total / 1.21m,
                IvaRate = 21,
                Total = total,
                TaxConditionTypeId = (int)client.TaxCondition.GetValueOrDefault(),
                ClientName = client.Name,
                ClientAddress = client.Address,
                Description = string.Join(",", products.Select(p => $"[{p.Type}, {p.Quantity}, {BusinessConstants.InvoiceUnitType}, {(int)p.Subtotal}]")),
                Email = client.Email ?? "",
            });
        }

        return result;
    }

    private IQueryable<Client> GetInvoiceClientsQuery(Day? invoiceDay, string invoiceDealer)
    {
        var query = _db.Carts
            .AsNoTracking()
            .Where(x =>
                x.IsStatic &&
                x.Client.IsActive &&
                x.Client.HasInvoice &&
                x.Client.InvoiceType.HasValue &&
                x.Client.TaxCondition.HasValue &&
                !string.IsNullOrEmpty(x.Client.CUIT));

        if (!string.IsNullOrEmpty(invoiceDealer))
            query = query.Where(x => x.Route.UserID == invoiceDealer);

        if (invoiceDay.HasValue && Enum.IsDefined(invoiceDay.Value))
            query = query.Where(x => x.Route.DayOfWeek == invoiceDay.Value);

        return query
            .OrderBy(x => x.Route.User.Name)
            .ThenBy(x => x.Priority)
            .Select(x => x.Client);
    }

    private static string EscapeCsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private class InvoiceProductCsv
    {
        public string Type { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}



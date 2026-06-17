using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models.Constants;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Services;

public class CajaService(APIContext context)
{
    private readonly APIContext _db = context;

    private const string MoneyFormat = "$ #,##0.00";
    private const int ManualExpenseRows = 6;

    public async Task<byte[]> GenerateDailyClose(DateTime date)
    {
        var day = date.Date;

        var sales = await _db.CartProducts
            .AsNoTracking()
            .Where(x => x.Cart.Route.CreatedAt.Date == day && !x.Cart.Route.IsStatic)
            .GroupBy(x => new { x.Cart.Route.UserID, x.Cart.Route.User.Name, x.Type })
            .Select(g => new { g.Key.UserID, g.Key.Name, g.Key.Type, Quantity = g.Sum(y => y.Quantity), Amount = g.Sum(y => y.Quantity * y.SettedPrice) })
            .ToListAsync();

        var repartos = sales
            .GroupBy(x => new { x.UserID, x.Name })
            .Select(g => new SaleRow
            {
                Name = g.Key.Name,
                SodaQty = g.Where(x => x.Type == ProductType.Soda).Sum(x => x.Quantity),
                SodaAmount = g.Where(x => x.Type == ProductType.Soda).Sum(x => x.Amount),
                B12Qty = g.Where(x => x.Type == ProductType.B12L).Sum(x => x.Quantity),
                B12Amount = g.Where(x => x.Type == ProductType.B12L).Sum(x => x.Amount),
                B20Qty = g.Where(x => x.Type == ProductType.B20L).Sum(x => x.Quantity),
                B20Amount = g.Where(x => x.Type == ProductType.B20L).Sum(x => x.Amount),
            })
            .OrderBy(x => x.Name)
            .ToList();

        var terceros = await _db.Terceros
            .AsNoTracking()
            .Where(x => x.Date.Date == day)
            .OrderBy(x => x.Name)
            .Select(x => new SaleRow
            {
                Name = x.Name,
                SodaQty = x.SodaQuantity,
                SodaAmount = x.SodaAmount,
                B12Qty = x.B12LQuantity,
                B12Amount = x.B12LAmount,
                B20Qty = x.B20LQuantity,
                B20Amount = x.B20LAmount,
            })
            .ToListAsync();

        var expenses = await _db.Expenses
            .AsNoTracking()
            .Where(x => x.CreatedAt.Date == day)
            .OrderBy(x => x.Description)
            .Select(x => new { x.Description, x.Amount })
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Caja Diaria");

        ws.Cell(1, 1).Value = "SODERIA LA NUEVA S.A";
        ws.Cell(2, 1).Value = "AGUA NICO";
        ws.Cell(1, 4).Value = "CAJA DIARIA";
        ws.Cell(2, 4).Value = day.ToString("dd-MM-yyyy");
        ws.Range(1, 1, 2, 1).Style.Font.Bold = true;
        ws.Range(1, 4, 2, 4).Style.Font.Bold = true;

        var row = 4;
        ws.Cell(row, 1).Value = "INGRESOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var headerRow = row;
        string[] headers = ["Distribuidora / Reparto", "Soda Cant.", "Soda Importe", "Bidón 12L Cant.", "Bidón 12L Importe", "Bidón 20L Cant.", "Bidón 20L Importe"];
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(headerRow, i + 1).Value = headers[i];
        ws.Range(headerRow, 1, headerRow, headers.Length).Style.Font.Bold = true;
        row++;

        var firstDataRow = row;
        foreach (var sale in terceros.Concat(repartos))
        {
            WriteSaleRow(ws, row, sale);
            row++;
        }
        var lastDataRow = row - 1;

        var totalsRow = row;
        ws.Cell(totalsRow, 1).Value = "TOTALES";
        if (lastDataRow >= firstDataRow)
            foreach (var col in new[] { 2, 3, 4, 5, 6, 7 })
                ws.Cell(totalsRow, col).FormulaA1 = $"SUM({Col(col)}{firstDataRow}:{Col(col)}{lastDataRow})";
        ws.Range(totalsRow, 1, totalsRow, 7).Style.Font.Bold = true;
        FormatMoney(ws, totalsRow, 3);
        FormatMoney(ws, totalsRow, 5);
        FormatMoney(ws, totalsRow, 7);
        row++;

        var ingresosRow = row;
        ws.Cell(ingresosRow, 1).Value = "TOTAL INGRESOS";
        ws.Cell(ingresosRow, 3).FormulaA1 = $"C{totalsRow}+E{totalsRow}+G{totalsRow}";
        ws.Range(ingresosRow, 1, ingresosRow, 3).Style.Font.Bold = true;
        FormatMoney(ws, ingresosRow, 3);
        row += 2;

        ws.Cell(row, 1).Value = "GASTOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "Detalle";
        ws.Cell(row, 2).Value = "Importe";
        ws.Range(row, 1, row, 2).Style.Font.Bold = true;
        row++;

        var firstExpenseRow = row;
        foreach (var expense in expenses)
        {
            ws.Cell(row, 1).Value = expense.Description;
            ws.Cell(row, 2).Value = expense.Amount;
            FormatMoney(ws, row, 2);
            row++;
        }
        // Filas vacías para gastos extra cargados a mano.
        for (var i = 0; i < ManualExpenseRows; i++)
        {
            FormatMoney(ws, row, 2);
            row++;
        }
        var lastExpenseRow = row - 1;

        var egresoRow = row;
        ws.Cell(egresoRow, 1).Value = "TOTAL EGRESO";
        ws.Cell(egresoRow, 2).FormulaA1 = $"SUM(B{firstExpenseRow}:B{lastExpenseRow})";
        ws.Range(egresoRow, 1, egresoRow, 2).Style.Font.Bold = true;
        FormatMoney(ws, egresoRow, 2);
        row += 2;

        ws.Cell(row, 1).Value = "SALDO NETO";
        ws.Cell(row, 2).FormulaA1 = $"C{ingresosRow}-B{egresoRow}";
        ws.Range(row, 1, row, 2).Style.Font.Bold = true;
        FormatMoney(ws, row, 2);

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void WriteSaleRow(IXLWorksheet ws, int row, SaleRow sale)
    {
        ws.Cell(row, 1).Value = sale.Name;
        ws.Cell(row, 2).Value = sale.SodaQty;
        ws.Cell(row, 3).Value = sale.SodaAmount;
        ws.Cell(row, 4).Value = sale.B12Qty;
        ws.Cell(row, 5).Value = sale.B12Amount;
        ws.Cell(row, 6).Value = sale.B20Qty;
        ws.Cell(row, 7).Value = sale.B20Amount;
        FormatMoney(ws, row, 3);
        FormatMoney(ws, row, 5);
        FormatMoney(ws, row, 7);
    }

    private static void FormatMoney(IXLWorksheet ws, int row, int col) => ws.Cell(row, col).Style.NumberFormat.Format = MoneyFormat;

    private static string Col(int col) => XLHelper.GetColumnLetterFromNumber(col);

    private class SaleRow
    {
        public string Name { get; set; }
        public int SodaQty { get; set; }
        public decimal SodaAmount { get; set; }
        public int B12Qty { get; set; }
        public decimal B12Amount { get; set; }
        public int B20Qty { get; set; }
        public decimal B20Amount { get; set; }
    }
}

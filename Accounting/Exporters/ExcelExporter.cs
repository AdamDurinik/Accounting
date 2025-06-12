using Accounting.Models;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;

namespace Accounting.Exporters
{
    public class ExcelExporter
    {
        public void Export(string filePath, List<ColumnModel> columns)
        {
            ExcelPackage.License.SetNonCommercialOrganization("FoxHint");

            var sheetName = Path.GetFileNameWithoutExtension(filePath);

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add(sheetName);

                int startCol = 1;

                var maxRows = columns.Max(c => c.Items.Count);

                foreach (var column in columns)
                {
                    ws.Cells[1, startCol].Value = "Cena bez DPH";
                    ws.Cells[1, startCol + 1].Value = $"{column.Tax} + DPH ";
                    ws.Cells[1, startCol + 2].Value = "Celková cena";
                    ws.Cells[1, startCol, 1, startCol + 2].Style.Font.Bold = true;
                    ws.Cells[1, startCol, 1, startCol + 2].Style.Font.Italic = true;

                    int rowStart = 2;
                    for (int i = 0; i < column.Items.Count; i++)
                    {
                        var item = column.Items[i];
                        int r = rowStart + i;
                        ws.Cells[r, startCol].Value = item.PriceWithoutTax;
                        ws.Cells[r, startCol + 1].Value = item.Tax;
                        ws.Cells[r, startCol + 2].Value = item.PriceWithTax;
                    }

                    int rowEnd = rowStart + column.Items.Count - 1;
                    int blankRows = 5;
                    int labelRow = maxRows + blankRows + 1;   
                    int sumRow = labelRow + 1;             

                    ws.Cells[labelRow, startCol].Value = "Spolu";
                    ws.Cells[labelRow, startCol].Style.Font.Bold = true;

                    for (int offset = 0; offset <= 2; offset++)
                    {
                        var cell = ws.Cells[sumRow, startCol + offset];
                        string fromAddr = ws.Cells[rowStart, startCol + offset].Address;
                        string toAddr = ws.Cells[rowEnd, startCol + offset].Address;
                        cell.Formula = $"SUM({fromAddr}:{toAddr})";
                        cell.Style.Font.Bold = true;
                    }

                    ws.Cells[rowStart, startCol, sumRow, startCol + 2]
                      .Style.Numberformat.Format = "0.00";
                    ws.Cells[1, startCol, sumRow, startCol + 2]
                      .AutoFitColumns();

                    startCol += 5;
                }

                package.SaveAs(new FileInfo(filePath));
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using CsvHelper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApexWorld_Backend.Features.Reports.Services{
    public class DocumentGeneratorService : ApexWorld_Backend.Features.Reports.Services.IDocumentGeneratorService
    {
        public byte[] GenerateExcel<T>(IEnumerable<T> data, string reportTitle)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");
            
            // Title
            worksheet.Cell(1, 1).Value = reportTitle;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            
            // Data
            if (data != null && data.Any())
            {
                worksheet.Cell(3, 1).InsertTable(data);
            }
            
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateCsv<T>(IEnumerable<T> data)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.WriteRecords(data);
            writer.Flush();
            return stream.ToArray();
        }

        public byte[] GeneratePdf<T>(IEnumerable<T> data, string reportTitle)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text(reportTitle).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        if (data != null && data.Any())
                        {
                            var properties = typeof(T).GetProperties();
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var prop in properties)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (var prop in properties)
                                {
                                    header.Cell().Element(CellStyle).Text(prop.Name).SemiBold();
                                }
                                
                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var item in data)
                            {
                                foreach (var prop in properties)
                                {
                                    var val = prop.GetValue(item)?.ToString() ?? string.Empty;
                                    table.Cell().Element(CellStyle).Text(val);
                                }
                                
                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateExcel(System.Data.DataTable data, string reportTitle)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");
            
            worksheet.Cell(1, 1).Value = reportTitle;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            
            if (data != null && data.Rows.Count > 0)
            {
                worksheet.Cell(3, 1).InsertTable(data);
            }
            
            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateCsv(System.Data.DataTable data)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            foreach (System.Data.DataColumn col in data.Columns)
            {
                csv.WriteField(col.ColumnName);
            }
            csv.NextRecord();
            
            foreach (System.Data.DataRow row in data.Rows)
            {
                for (var i = 0; i < data.Columns.Count; i++)
                {
                    csv.WriteField(row[i]);
                }
                csv.NextRecord();
            }
            
            writer.Flush();
            return stream.ToArray();
        }

        public byte[] GeneratePdf(System.Data.DataTable data, string reportTitle)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text(reportTitle).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        if (data == null || data.Columns.Count == 0 || data.Rows.Count == 0)
                        {
                            col.Item().Text("No data available for this report.").FontSize(12).Italic().FontColor(Colors.Grey.Medium);
                        }
                        else
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    foreach (System.Data.DataColumn dtCol in data.Columns)
                                    {
                                        columns.RelativeColumn();
                                    }
                                });

                                table.Header(header =>
                                {
                                    foreach (System.Data.DataColumn dtCol in data.Columns)
                                    {
                                        header.Cell().Element(CellStyle).Text(dtCol.ColumnName).SemiBold();
                                    }
                                    
                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (System.Data.DataRow row in data.Rows)
                                {
                                    foreach (System.Data.DataColumn dtCol in data.Columns)
                                    {
                                        var val = row[dtCol]?.ToString() ?? string.Empty;
                                        table.Cell().Element(CellStyle).Text(val);
                                    }
                                    
                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}




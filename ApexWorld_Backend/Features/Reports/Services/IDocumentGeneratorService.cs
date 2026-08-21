using System.Collections.Generic;
using System.Data;

namespace ApexWorld_Backend.Features.Reports.Services{
    public interface IDocumentGeneratorService
    {
        byte[] GenerateExcel<T>(IEnumerable<T> data, string title);
        byte[] GenerateCsv<T>(IEnumerable<T> data);
        byte[] GeneratePdf<T>(IEnumerable<T> data, string title);

        byte[] GenerateExcel(DataTable data, string title);
        byte[] GenerateCsv(DataTable data);
        byte[] GeneratePdf(DataTable data, string title);
    }
}


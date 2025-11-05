using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AuthApi.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace AuthApi.Services
{
    public class PdfService
{
    // Existing method for file path (optional)
    public async Task GenerateFdPdfAsync(List<FDModel> fdList, string outputPath)
    {
        using (var writer = new PdfWriter(outputPath))
        {
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.Add(new Paragraph("Fixed Deposit List").SetFontSize(18));

            foreach (var fd in fdList)
            {
                var p = new Paragraph()
                    .Add($"ID: {fd.Id}\n")
                    .Add($"Amount: ₹{fd.Amount:N2}\n")
                    .Add($"Interest Rate: {fd.InterestRate}%\n")
                    .Add($"Maturity Date: {fd.MaturityDate:yyyy-MM-dd}\n")
                    .Add("--------------------------------------------------");
                document.Add(p);
            }

            document.Close();
        }

        await Task.CompletedTask;
    }

    // New method to generate PDF to a stream
    public async Task GenerateFdPdfAsync(List<FDModel> fdList, MemoryStream outputStream)
    {
        using var writer = new PdfWriter(outputStream);
        using var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        document.Add(new Paragraph("Fixed Deposit List").SetFontSize(18));

        foreach (var fd in fdList)
        {
            var p = new Paragraph()
                .Add($"ID: {fd.Id}\n")
                .Add($"Amount: ₹{fd.Amount:N2}\n")
                .Add($"Interest Rate: {fd.InterestRate}%\n")
                .Add($"Maturity Date: {fd.MaturityDate:yyyy-MM-dd}\n")
                .Add("--------------------------------------------------");
            document.Add(p);
        }

        document.Close();

        await Task.CompletedTask;
    }
}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AuthApi.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace AuthApi.Services
{
    public class PdfService
    {
        public async Task GenerateFdPdfAsync(List<FDModel> fdList, MemoryStream outputStream)
        {
            var document = new PdfDocument();
            var headerFont = new XFont("Verdana", 16, XFontStyle.Bold);
            var titleFont = new XFont("Verdana", 12, XFontStyle.Bold);
            var contentFont = new XFont("Verdana", 12, XFontStyle.Regular);
            var noteFont = new XFont("Verdana", 10, XFontStyle.Regular);
            var footerFont = new XFont("Verdana", 10, XFontStyle.Italic);

            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "FD.jpg");
            XImage? logo = File.Exists(imagePath) ? XImage.FromFile(imagePath) : null;

            foreach (var fd in fdList)
            {
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                double margin = 40;
                double yPoint = 40;
                double bottomMargin = page.Height - 80;

                // Draw logo if available
                if (logo != null)
                    gfx.DrawImage(logo, page.Width / 2 - 50, yPoint, 100, 50);

                yPoint += 70;

                // Header
                gfx.DrawString("Fixed Deposit Certificate", headerFont, XBrushes.DarkBlue,
                    new XRect(0, yPoint, page.Width, 30), XStringFormats.TopCenter);

                yPoint += 50;

                // FD details
                gfx.DrawString($"FD ID: {fd.Id}", titleFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Bank Name: {fd.BankName}", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Account Number: {fd.AccountNumber}", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Invested Amount: ₹{fd.Amount:N2}", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Interest Rate: {fd.InterestRate}%", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Invested Date: {fd.InvestedDate:yyyy/MM/dd}", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Maturity Date: {fd.MaturityDate:yyyy/MM/dd}", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Tenure: {fd.TenureMonths} months", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 25;
                gfx.DrawString($"Status: {fd.Status}", contentFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 40;

                gfx.DrawString("Important Notes:", titleFont, XBrushes.Black, new XPoint(margin, yPoint));
                yPoint += 20;

                // Notes text
                string[] notes =
                {
                    "1. This Fixed Deposit (FD) is held with ABC Bank Ltd. and governed by its regulations.",
                    "2. Interest is credited based on the chosen plan: quarterly, monthly, or cumulative.",
                    "3. Premature withdrawal may attract a penalty as per bank norms.",
                    "4. Nominee details are as per records; please verify regularly.",
                    "5. This certificate is non-transferable and must be retained safely.",
                    "6. Deposit is insured under the Deposit Insurance and Credit Guarantee Corporation (DICGC) scheme up to applicable limits.",
                    "7. For disputes, jurisdiction will be the city of the issuing branch.",
                    "8. Bank reserves the right to modify rates as per RBI or internal policy.",
                    "9. This document is computer-generated and doesn’t require a signature.",
                    "10. For assistance, contact customer care or your nearest branch."
                };

                foreach (var note in notes)
                {
                    // Wrap long lines
                    var textLines = SplitTextToFitWidth(gfx, note, noteFont, page.Width - 2 * margin);

                    foreach (var line in textLines)
                    {
                        if (yPoint > bottomMargin)
                        {
                            // Create a new page when space runs out
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPoint = margin;

                            // Continue footer on new page
                            gfx.DrawString("Fixed Deposit Certificate (Continued)", titleFont, XBrushes.Gray,
                                new XRect(0, yPoint, page.Width, 20), XStringFormats.TopCenter);
                            yPoint += 30;
                        }

                        gfx.DrawString(line, noteFont, XBrushes.Black,
                            new XRect(margin, yPoint, page.Width - 2 * margin, 15), XStringFormats.TopLeft);
                        yPoint += 16;
                    }
                }

                // Footer (only on last page of each FD)
                gfx.DrawLine(XPens.Black, margin, page.Height - 60, page.Width - margin, page.Height - 60);
                gfx.DrawString("ABC Bank Ltd. | Customer Care: 1800-123-456 | Email: support@abcbank.com",
                    footerFont, XBrushes.Gray,
                    new XRect(margin, page.Height - 50, page.Width - 2 * margin, 20),
                    XStringFormats.TopCenter);
            }

            document.Save(outputStream, false);
            outputStream.Position = 0;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Splits text into multiple lines based on available width.
        /// </summary>
        private static List<string> SplitTextToFitWidth(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var size = gfx.MeasureString(testLine, font);

                if (size.Width > maxWidth)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines;
        }
    }
}

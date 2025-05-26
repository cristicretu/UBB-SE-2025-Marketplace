using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace MarketMinds.Shared.Services
{
    public static class PdfGenerator
    {
        public static byte[] GenerateContractPdf(string contractTitle, string contractContent)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.Content()
                        .Column(col =>
                        {
                            col.Item().PaddingBottom(20).Text(contractTitle).FontSize(20).Bold();
                            col.Item().Text(contractContent).FontSize(12);
                            col.Item().Text($"Generated: {DateTime.Now}").FontSize(10).Italic().AlignRight();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
} 
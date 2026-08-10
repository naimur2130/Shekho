using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Shekho.Services
{
    public class CertificateService : ICertificateService
    {
        public byte[] GenerateCertificate(string studentName, string courseTitle, DateTime completionDate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20).FontColor(Colors.Black));

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(25);

                            col.Item().AlignCenter().Text("🎓 Certificate of Completion").Bold().FontSize(30);
                            col.Item().AlignCenter().Text("This certificate is proudly presented to").FontSize(18);

                            col.Item().AlignCenter().Text(studentName).Bold().FontSize(28).FontColor(Colors.Blue.Medium);

                            col.Item().AlignCenter().Text("For successfully completing the course").FontSize(18);

                            col.Item().AlignCenter().Text(courseTitle).Bold().FontSize(24).FontColor(Colors.Green.Darken1);

                            col.Item().AlignCenter().Text($"Date: {completionDate:MMMM dd, yyyy}").FontSize(16);

                            col.Item().AlignCenter().Text("Shekho Learning Platform").Italic().FontSize(18).FontColor(Colors.Grey.Darken1);
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();

            return pdfBytes;
        }
    }
}


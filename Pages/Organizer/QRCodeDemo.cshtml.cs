using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace MONDAY_FK_X_SOEN341_PROJECT_F25.Pages.Organizer
{
    public class QRCodeDemoModel : PageModel
    {
        [BindProperty]
        public string? InputText { get; set; }

        public IActionResult OnGet() => Page();

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(InputText))
                return Page();

            // Generate QR code
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(InputText, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var qrImage = qrCode.GetGraphic(20);

            byte[] qrBytes;
            using (var ms = new MemoryStream())
            {
                qrImage.Save(ms, ImageFormat.Png);
                qrBytes = ms.ToArray();
            }

            // Create PDF
            using var pdf = new PdfDocument();
            var page = pdf.AddPage();
            using var gfx = XGraphics.FromPdfPage(page);
            using var qrStream = new MemoryStream(qrBytes);
            using var xImage = XImage.FromStream(() => qrStream);

            gfx.DrawString("QR Code for: " + InputText,
                new XFont("Arial", 14),
                XBrushes.Black,
                new XPoint(20, 30));
            gfx.DrawImage(xImage, 20, 50, 200, 200);

            using var pdfStream = new MemoryStream();
            pdf.Save(pdfStream, false);
            pdfStream.Position = 0;

            // Return PDF to browser
            return File(pdfStream.ToArray(), "application/pdf", "QRCode.pdf");
        }
    }
}

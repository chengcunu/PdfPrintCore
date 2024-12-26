using System.Text;

namespace PdfPrintCore.API.Demo;

public static class EndPoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapGet("/", () =>
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "index.html.txt");
            using var reader = new StreamReader(filename);
            return Results.Content(reader.ReadToEnd(), "text/html", Encoding.UTF8);
        });

        app.MapGet("list-printers", List<string> () => PrinterService.ListPrinters());

        // GET {{HostAddress}}/get-printer-info?printer=Microsoft%20Print%20to%20PDF
        app.MapGet("get-printer-info", PrinterInfo (string printer) => PrinterService.GetPrinterInfo(printer));

        // POST {{HostAddress}}/print?printer=Microsoft%20Print%20to%20PDF&color=&renderFormFill=
        // Content-Type: application/pdf
        // Content: Body stream
        app.MapPost("print", static async ([AsParameters]PrintTemplate template, Stream body, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("print");

            using var stream = new MemoryStream();
            await body.CopyToAsync(stream, cancellationToken);

            using var document = new PdfDocument(stream);
            PrintingService service = new(document);

            service.OnDrawingPage +=(args) => { };
            if (template.ColorModel is ColorModel color) service.PrinterSettings.ColorModel = color;
            if (template.RenderFormFill is bool formFill) service.PrinterSettings.RenderFormFill = formFill;

            await service.PrintAsync(template.Printer, cancellationToken);
            return Results.Ok();
        });

        // POST {{HostAddress}}/print/inbase64?printer=&color=&renderFormFill=
        // Content-Type: text
        // Content: Base64 text
        app.MapPost("print/inbase64", static async ([AsParameters] PrintTemplate template, HttpContext context) =>
        {
            byte[] buffer = await Base64.ConvertFromBase64StreamAsync(context.Request.BodyReader);

            using var document = new PdfDocument(buffer);
            PrintingService service = new(document);

            if (template.ColorModel is ColorModel color) service.PrinterSettings.ColorModel = color;
            if (template.RenderFormFill is bool formFill) service.PrinterSettings.RenderFormFill = formFill;

            await service.PrintAsync(template.Printer, context.RequestAborted);
        });
    }

    public class PrintTemplate
    {
        public required string Printer { get; init; }
        public ColorModel? ColorModel { get; init; }
        public bool? RenderFormFill { get; init; }
    }
}

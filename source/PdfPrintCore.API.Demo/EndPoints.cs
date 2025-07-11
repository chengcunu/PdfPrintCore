using System.Text;

namespace PdfPrintCore.API.Demo;

public static class EndPoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", () =>
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "index.html.txt");
            using var reader = new StreamReader(filename);
            return Results.Content(reader.ReadToEnd(), "text/html", Encoding.UTF8);
        });

        routes.MapGet("api/list-installed-printers", List<string> () => PrinterService.ListInstalledPrinters());

        routes.MapGet("api/list-printers", List<Printer> () => PrinterService.ListPrinters());

        // GET {{HostAddress}}/api/get-printer-info?printer=Microsoft%20Print%20to%20PDF
        routes.MapGet("api/get-printer", Printer (string printer) => PrinterService.GetPrinter(printer));

        // POST {{HostAddress}}/api/print?printer=Microsoft%20Print%20to%20PDF&color=&renderFormFill=
        // Content-Type: application/pdf
        // Content: Body stream
        routes.MapPost("api/print", static async ([AsParameters]PrintFromTemplate template, Stream body, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("api/print");
            
            using var stream = new MemoryStream();
            await body.CopyToAsync(stream, cancellationToken);

            using var document = new PdfDocument(stream);
            PrintService service = new(document);

            service.OnDrawingPage +=(args) => { };
            if (template.ColorModel is ColorModel color) service.PrintOptions.ColorModel = color;
            if (template.RenderFormFill is bool formFill) service.PrintOptions.RenderFormFill = formFill;

            await service.PrintAsync(template.Printer, cancellationToken);
            return Results.Ok();
        });

        // POST {{HostAddress}}/api/print/inbase64?printer=&color=&renderFormFill=
        // Content-Type: text
        // Content: Base64 text
        routes.MapPost("api/print/base64", static async ([AsParameters] PrintFromTemplate template, HttpContext context) =>
        {
            byte[] buffer = await Base64.ConvertFromBase64StreamAsync(context.Request.BodyReader);

            using var document = new PdfDocument(buffer);
            PrintService service = new(document);

            if (template.ColorModel is ColorModel color) service.PrintOptions.ColorModel = color;
            if (template.RenderFormFill is bool formFill) service.PrintOptions.RenderFormFill = formFill;

            await service.PrintAsync(template.Printer, context.RequestAborted);
        });
    }

    public class PrintFromTemplate
    {
        public required string Printer { get; init; }
        public ColorModel? ColorModel { get; init; }
        public bool? RenderFormFill { get; init; }
    }
}

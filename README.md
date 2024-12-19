# ![](https://raw.githubusercontent.com/chengcunu/PdfPrintCore/main/documentation/logo64.png) PdfPrintCore

A .NET core PDF printing library for printing PDF files to a printer in the background.

## Supported Platforms
* Windows
* Macos
* Linux

## Using

[PdfPrintCore](https://www.nuget.org/packages/PdfPrintCore)  is available as a convenient NuGet package, to use install the package like this:

```
nuget install PdfPrintCore
```
Linux support nuget package: [PdfPrintCore.NativeAssets.Linux](https://www.nuget.org/packages/PdfPrintCore.NativeAssets.Linux) 
## Example

``` c#
using var document = new PdfDocument("test.pdf");
PrintingService service = new(document);
await service.PrintAsync("Microsoft Print to PDF", cancellationToken);
```
See the [Demo](source/PdfPrintCore.API.Demo) for more examples.

## License

You can get a free license here  [license.xml](source/PdfPrintCore.API.Demo/PdfPrintCore.license.xml) 

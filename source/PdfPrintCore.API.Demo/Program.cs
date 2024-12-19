using PdfPrintCore;
using PdfPrintCore.API.Demo;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using static PdfPrintCore.API.Demo.EndPoints;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

bool runAsService = !(Debugger.IsAttached || args.Contains("--console"));
string? url = builder.Configuration["Url"];

builder.Services.ConfigureHttpJsonOptions(static options => {
    options.SerializerOptions.TypeInfoResolverChain.Add(DefaultJsonSerializerContext.Default);
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (runAsService)
{
    string workingDir = Path.GetDirectoryName(Environment.ProcessPath)!;
    Directory.SetCurrentDirectory(workingDir);

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        builder.Services.AddWindowsService();
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        builder.Services.AddSystemd();
}

var app = builder.Build();
app.MapEndpoints();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run(url);

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(PrinterInfo))]
[JsonSerializable(typeof(PrintSettings))]
[JsonSerializable(typeof(PrintTemplate))]
internal partial class DefaultJsonSerializerContext : JsonSerializerContext { }
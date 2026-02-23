using Microsoft.Extensions.Logging;
using PuppetFestivalProject.Shared.Services;
using PuppetFestivalProject.Services;
using PuppetFestivalProject.Shared.Data;

namespace PuppetFestivalProject;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the PuppetFestivalProject.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
       builder.Services.AddSingleton<TemplateServices>();
        return builder.Build();
    }
}

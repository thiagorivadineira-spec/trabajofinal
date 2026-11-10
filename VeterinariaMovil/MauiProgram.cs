using Microsoft.Extensions.Logging;
using VeterinariaMovil.Services;

namespace VeterinariaMovil;

public static class MauiProgram
{
    // URL de la API publicada (la MISMA que usa la web). Cambiala por la de tu sitio nuevo.
    public const string ApiBaseUrl = "https://veterinariavolki.runasp.net/";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Un solo HttpClient para toda la app, apuntando a la API
        builder.Services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        });

        // Sesión (token guardado en SecureStorage del celular) y cliente de la API
        builder.Services.AddSingleton<SesionMovil>();
        builder.Services.AddSingleton<ApiMovil>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

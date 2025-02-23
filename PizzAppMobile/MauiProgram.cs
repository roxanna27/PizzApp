using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using PizzAppMobile.Services;
using PizzAppMobile.Views;

namespace PizzAppMobile;

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
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        // Înregistrare servicii pentru Dependency Injection (DI)
        builder.Services.AddSingleton<ApiService>();  // Serviciul API
        builder.Services.AddSingleton<ProductsPage>();  // Pagină principală

        return builder.Build();
    }
}

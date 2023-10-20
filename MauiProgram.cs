namespace Calculator;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("UbuntuMono.ttf", "RegularFont");
                fonts.AddFont("UbuntuMono-Bold.ttf", "BoldFont");
            });

        return builder.Build();
    }
}

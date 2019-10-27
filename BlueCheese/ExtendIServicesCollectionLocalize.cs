using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace BlueCheese
{
    public static class ExtendIServicesCollectionLocalize
    {
        public static void LocalizeBlueCheese(this IServiceCollection services)
        {
            services.AddLocalization(o => o.ResourcesPath = "");
            services.Configure<RequestLocalizationOptions>(options =>
            {

                var supportedCultures = new[]
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("es-ES")
                };
                options.DefaultRequestCulture = new RequestCulture("en-GB", "en-GB");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting 
                // numbers, dates, etc.

                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, 
                // i.e. we have localized resources for.

                options.SupportedUICultures = supportedCultures;
            });
        }
    }
}

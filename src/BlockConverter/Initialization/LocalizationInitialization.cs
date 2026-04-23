using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gulla.Episerver.BlockConverter.Initialization;

[ModuleDependency(typeof(FrameworkInitialization))]
public class LocalizationInitialization : IInitializableModule
{
    public void Initialize(InitializationEngine context)
    {
        if (context.Locate.Advanced.GetRequiredService<LocalizationService>() is not ProviderBasedLocalizationService localizationService)
            return;

        var assembly = Assembly.GetAssembly(typeof(LocalizationInitialization))!;
        var xmlResources = assembly.GetManifestResourceNames()
            .Where(r => r.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase));

        foreach (var file in xmlResources)
        {
            using var stream = assembly.GetManifestResourceStream(file)!;
            var provider = new XmlLocalizationProvider();
            provider.Initialize(file, null);
            provider.Load(stream);
            localizationService.AddProvider(provider);
        }
    }

    public void Uninitialize(InitializationEngine context) { }
}

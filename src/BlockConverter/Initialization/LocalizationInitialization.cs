using System;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;

namespace Gulla.Episerver.BlockConverter.Initialization
{
    [ModuleDependency(typeof(FrameworkInitialization))]
    public class LocalizationInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (context.Locate.Advanced.GetInstance<LocalizationService>() is ProviderBasedLocalizationService localizationService)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(LocalizationInitialization));

                string[] xmlResources =
                    assembly.GetManifestResourceNames()
                        .Where(r => r.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();

                foreach (string file in xmlResources)
                {
                    using (var stream = assembly.GetManifestResourceStream(file))
                    {
                        var provider = new XmlLocalizationProvider();
                        provider.Initialize(file, null);
                        provider.Load(stream);
                        localizationService.AddProvider(provider);
                    }
                }
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters) { }
    }
}

using System;
using System.Linq;
using ICSharpCode.Core;
using ICSharpCode.Core.Services;
using ICSharpCode.SharpDevelop.Gui;
using MyLoadTest.VuGenAddInManager.Compatibility;
using MyLoadTest.VuGenAddInManager.Properties;

namespace MyLoadTest.VuGenAddInManager.HostCommands
{
    public sealed class AddInManagerInitializationCommand : AbstractMenuCommand
    {
        public override void Run()
        {
            Initialize();
            RegisterResources();

            // Remove all unreferenced NuGet packages
            AddInManagerServices.Setup.RemoveUnreferencedNuGetPackages();
        }

        private static void Initialize()
        {
            var container = new SharpDevelopServiceContainer();

            container.AddFallbackProvider(ServiceSingleton.FallbackServiceProvider);

            container.AddService(typeof(IPropertyService), new PropertyServiceImpl(new ICSharpCode.Core.Properties()));
            container.AddService(typeof(IMessageService), new SDMessageService());
            container.AddService(typeof(ILoggingService), new FallbackLoggingService());
            container.AddService(typeof(IAddInTree), new AddInTreeImpl());
                                                      
            ServiceSingleton.ServiceProvider = container;

            WorkbenchSingleton.WorkbenchCreated +=
                (sender, args) => container.AddService(typeof(IWorkbench), WorkbenchSingleton.Workbench);
        }

        private static void RegisterResources()
        {
            ResourceService.RegisterNeutralImages(Resources.ResourceManager);
            ResourceService.RegisterNeutralStrings(Resources.ResourceManager);
        }
    }
}
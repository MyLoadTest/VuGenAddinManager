using System;
using System.Linq;
using ICSharpCode.SharpDevelop;

namespace MyLoadTest.VuGenAddInManager.HostCommands
{
    public sealed class AddInManagerInitializationCommand : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            // Remove all unreferenced NuGet packages
            AddInManagerServices.Setup.RemoveUnreferencedNuGetPackages();
        }
    }
}
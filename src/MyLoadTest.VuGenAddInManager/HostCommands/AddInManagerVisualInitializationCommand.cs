using System;
using System.Linq;
using ICSharpCode.SharpDevelop;

namespace MyLoadTest.VuGenAddInManager.HostCommands
{
    public sealed class AddInManagerVisualInitializationCommand : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            if (AddInManagerServices.Settings.AutoSearchForUpdates)
            {
                // Initialize UpdateNotifier and let it check for available updates
                var updateNotifier = new UpdateNotifier();
                updateNotifier.StartUpdateLookup();
            }
        }
    }
}
using System;
using System.Linq;
using ICSharpCode.Core;

namespace MyLoadTest.VuGenAddInManager.HostCommands
{
    public sealed class AddInManagerVisualInitializationCommand : AbstractMenuCommand
    {
        public override void Run()
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
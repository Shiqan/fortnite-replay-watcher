using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace FortniteReplayWatcher
{
    [RunInstaller(true)]
    public class MyProjectInstaller : Installer
    {
        public MyProjectInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            var serviceInstaller = new ServiceInstaller
            {
                ServiceName = "FortniteReplayWatcher",
                DisplayName = "FortniteReplayWatcher",
                Description = "Automagically upload your Fortnite replay files",
                StartType = ServiceStartMode.Automatic

            };

            Installers.AddRange(new Installer[] {
                serviceProcessInstaller, serviceInstaller
            });
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            //The following code starts the services after it is installed.
            using (var serviceController = new ServiceController("FortniteReplayWatcher"))
            {
                serviceController.Start();
            }
        }
    }
}
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace FortniteReplayWatcher
{
    [RunInstaller(true)]
    public class MyProjectInstaller : Installer
    {
        private readonly ServiceProcessInstaller _serviceProcessInstaller;
        private readonly ServiceInstaller _serviceInstaller;

        public MyProjectInstaller()
        {
            _serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.User
            };

            _serviceInstaller = new ServiceInstaller
            {
                ServiceName = "FortniteReplayWatcher",
                DisplayName = "FortniteReplayWatcher",
                Description = "Automagically upload your Fortnite replay files",
                StartType = ServiceStartMode.Automatic,
                DelayedAutoStart = true

            };

            Installers.AddRange(new Installer[] {
                _serviceProcessInstaller, _serviceInstaller
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
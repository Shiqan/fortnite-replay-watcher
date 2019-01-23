using System.ServiceProcess;

namespace FortniteReplayWatcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new FortniteReplayWatcher()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

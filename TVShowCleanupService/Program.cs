using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TVShowCleanup;

namespace TVShowCleanupService
{
    static class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

#if DEBUG
            // Debug
            var myService = new TVShowCleanupService();
            myService.Debug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#else
            // Release
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new TVShowCleanupService() };
            ServiceBase.Run(ServicesToRun);
#endif
            
        }
        
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((Exception)e.ExceptionObject);            
        }

        static void HandleException(Exception e)
        {
            //Handle your Exception here
            CleanupLog.WriteLine($"ERROR! {e}");            
        }
    }
}

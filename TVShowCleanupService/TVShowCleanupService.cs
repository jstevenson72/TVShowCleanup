using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using TVShowCleanup;

namespace TVShowCleanupService
{
    public partial class TVShowCleanupService : ServiceBase
    {
        public TVShowCleanupService()
        {
            InitializeComponent();

            CleanupLog.WriteMethod();
        }

        protected override void OnStart(string[] args)
        {
            CleanupLog.WriteMethod();

            Task.Run(() => CleanupHelper.StartCleanup());
        }

        protected override void OnStop()
        {
            CleanupLog.WriteMethod();

            CleanupHelper.StopCleanup();
        }

        internal void Debug()
        {
            OnStart(null);
        }
    }
}

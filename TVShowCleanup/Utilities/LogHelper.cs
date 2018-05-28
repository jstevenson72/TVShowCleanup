using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShowCleanup.Utilities
{
    public sealed class LogHelper
    {
        static readonly Log _log = new Log();

        private LogHelper()
        {
        }

        public static Log Log
        {
            get
            {
                return _log;
            }
        }

    }
}

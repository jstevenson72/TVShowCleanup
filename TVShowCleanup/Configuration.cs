using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TVShowCleanup
{
    [DataContract]
    public class Configuration
    {
        [DataMember]
        public List<string> DirectoriesToCleanup { get; set; } = new List<string>();

        [DataMember]
        public double HowOftenInMinutesToCleanup { get; set; } = 10;

        [DataMember]
        public bool EnableGlobalTimerCleanup { get; set; } = false;

        [DataMember]
        public bool EnableFileWatcherCleanup { get; set; } = true;

        [DataMember]
        public int KeepFilesNewerThanThisNumberOfDays { get; set; } = 15; // 2 Weeks

        [DataMember]
        public int MinimumNumberOfFilesToKeep { get; set; } = 10;

        [DataMember]
        public string Username { get; internal set; }
        
        [DataMember] 
        public string Password { get; internal set; }
    }
}

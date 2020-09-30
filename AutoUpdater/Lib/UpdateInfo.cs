using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoUpdater
{

    [Serializable]
    public class UpdateInfo
    {
        public string AppName { get; set; }

        public Version AppVersion { get; set; }

        public DateTime AppDate { get; set; }

        public Guid MD5 { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor.Model
{
    public class EmailSetting
    {
        public string email { get; set; }
        public string password { get; set; }
        public int PortNumber { get; set; }
        public string HostName { get; set; }
    }
}

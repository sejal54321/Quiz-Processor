using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor.Model
{
    public class XmlDetails
    {
        public Int64 Id { get; set; }
        public Int64 SummaryDetailId { get; set; }
        public string XmlDetail { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

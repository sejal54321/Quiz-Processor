using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor.Model
{
    public class SummaryDetails
    {
        public Int64 Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public Nullable<int> TotalQuestion { get; set; }
        public Nullable<int> AnsweredQuestion { get; set; }
        public DateTime Date { get; set; }
        public Nullable<int> TimeTaken { get; set; }
        public Nullable<decimal> PassingPercent { get; set; }
        public Nullable<Decimal> Score { get; set; }
        public Nullable<decimal> PassingScore { get; set; }
        public Nullable<decimal> Percentage { get; set; }      
        public bool IsPassed { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor.Model
{
    public class QuestionDetails
    {
        public Int64 Id { get; set; }
        public Int64 SummaryDetailId { get; set; }
        public string QuizQuestion { get; set; }
        public string QuestionType { get; set; }
        public string CorrectAnswer { get; set; }
        public string UserSelection { get; set; }
        //public string QuestionAnswerd { get; set; }
        public Nullable<int> AwardedPoints { get; set; }
        public Nullable<int> MaxPoints { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

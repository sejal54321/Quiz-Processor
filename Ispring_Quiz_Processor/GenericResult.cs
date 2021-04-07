using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor
{
    public class GenericResult
    {
        public string Message { get; set; }
        // public string stringBuilderMessage { get; set; }
        public object Errors { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class GenericResult<TResult> : GenericResult
    {
        public TResult Data { get; set; }
    }
}

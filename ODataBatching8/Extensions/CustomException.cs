using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataBatching8.Extensions
{
    public class CustomException : Exception
    {
        public int HttpStatusCode { get; private set; }

        public CustomException()
        {

        }

        public CustomException(int httpStatusCode, string message) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}

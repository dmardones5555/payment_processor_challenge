using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class DomainException: Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public DomainException(string message) : base(message)
        {
            Code = "domain_error";
        }

        public DomainException(string code, string message, int statusCode = 400) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}

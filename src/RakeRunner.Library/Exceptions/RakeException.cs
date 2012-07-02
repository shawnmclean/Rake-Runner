using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakeRunner.Library.Exceptions
{
    public class RakeFailedException : Exception
    {
        public RakeFailedException()
        {
        }

        public RakeFailedException(string message)
            : base(message)
        {
        }
    }
}

using System;

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
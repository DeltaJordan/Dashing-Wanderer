using System;

namespace DashingWanderer.Exceptions
{
    /// <summary>
    /// Exception that is output for guild memebers to see.
    /// </summary>
    public class OutputException : Exception
    {
        public OutputException(string message) : base(message)
        {
        }
    }
}

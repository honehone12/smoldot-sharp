using System;

namespace SmoldotSharp.JsonRpc
{
    public class UnexpectedDataException : Exception
    {
        public UnexpectedDataException()
            : base("The data is not expected type or enum variants.")
        {
            // Empty
        }
    }
}

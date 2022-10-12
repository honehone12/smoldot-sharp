using System;
using SmoldotSharp.Msgs;

namespace SmoldotSharp
{
    public class InitializationFailedException : Exception
    {
        public InitializationFailedException(string name)
            : base($"Failed to initialize {name}.")
        {
            // Empty
        }
    }

    public class UnexpectedMessageException : Exception
    {
        public UnexpectedMessageException(Message m)
            : base($"Unexpected message. name: {m.GetType().Name}")
        {
            // Empty
        }
    }

    public class UnexpectedEnumValueException : Exception
    {
        public UnexpectedEnumValueException()
            : base("Enum value is not expcted.")
        {
            // Empty
        }
    }

    public class ExceedAllocationLimitException : Exception
    {
        public ExceedAllocationLimitException()
            : base("Data size exceed memory allocation limit.")
        {
            // Empty
        }
    }

    public class UnExpectedDataUsageException : Exception
    {
        public UnExpectedDataUsageException(string message)
            : base("Out of expected usage. " + message)
        {
            // Empty
        }
    }

    public class NoSuchChainException : Exception
    {
        public NoSuchChainException(string name)
            : base($"No such chain. name: {name}")
        {
            // Empty
        }
    }
}
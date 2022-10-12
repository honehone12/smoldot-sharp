using System;

namespace ScaleSharpLight
{
    /// <summary>
    /// Thrown when buffer is smaller than expected.
    /// </summary>
    public class BufferOutOfRangeException : Exception
    {
        public BufferOutOfRangeException()
            : base("Buffer is not enough for the operation.")
        {
            // Empty
        }
    }

    /// <summary>
    /// Thrown when provided data is not expected.
    /// </summary>
    public class NotExpectedDataException : Exception
    {
        public NotExpectedDataException()
            : base("The data is not expected for encoding.")
        {
            // Empty
        }
    }
}
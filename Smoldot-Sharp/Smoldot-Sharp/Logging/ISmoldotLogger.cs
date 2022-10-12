namespace SmoldotSharp
{
    public interface ISmoldotLogger
    {
        public void Log(SmoldotLogLevel logLevel, string what);
    }
}
using System;
using System.Diagnostics;

namespace SmoldotSharp
{
    public class SmoldotDevLogger : ISmoldotLogger
    {
        readonly SmoldotLogLevel filterLevel;

        public SmoldotDevLogger(SmoldotLogLevel filterLevel)
        {
            this.filterLevel = filterLevel;
        }

        public void Log(SmoldotLogLevel logLevel, string what)
        {
            if (logLevel > filterLevel)
            {
                return;
            }

            var callStack = new StackFrame(1, true);
            Console.WriteLine(
                $"{DateTime.Now} [{logLevel}] {what} -- {callStack.GetFileName()} ln:{callStack.GetFileLineNumber()} col:{callStack.GetFileColumnNumber()} {callStack.GetMethod()}");
        }
    }
}
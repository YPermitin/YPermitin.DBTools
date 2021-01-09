using System;

namespace YY.DBTools.SQLServer.XEvents.EventArguments
{
    public sealed class OnErrorEventArgs : EventArgs
    {
        public OnErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}

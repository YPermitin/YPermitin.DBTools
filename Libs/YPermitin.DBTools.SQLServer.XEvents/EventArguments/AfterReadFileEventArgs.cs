﻿using System;

namespace YPermitin.DBTools.SQLServer.XEvents.EventArguments
{
    public sealed class AfterReadFileEventArgs : EventArgs
    {
        public AfterReadFileEventArgs(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}

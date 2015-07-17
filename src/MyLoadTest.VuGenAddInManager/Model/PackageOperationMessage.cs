using System;
using System.Linq;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.Model
{
    public sealed class PackageOperationMessage
    {
        private readonly string _message;
        private readonly object[] _args;

        public PackageOperationMessage(MessageLevel level, string message, params object[] args)
        {
            this.Level = level;
            this._message = message;
            this._args = args;
        }

        public MessageLevel Level
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format(_message, _args);
        }
    }
}
using System;
using System.Linq;
using ICSharpCode.Core.Services;

namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    internal sealed class FallbackLoggingService : TextWriterLoggingService
    {
        public FallbackLoggingService()
            : base(new TraceTextWriter())
        {
        }
    }
}
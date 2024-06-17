using System;

namespace Owlcat.Runtime.Core.Logging;

public interface IDisposableLogSink : ILogSink, IDisposable
{
}

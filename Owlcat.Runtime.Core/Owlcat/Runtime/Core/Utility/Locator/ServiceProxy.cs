using System;

namespace Owlcat.Runtime.Core.Utility.Locator;

public class ServiceProxy
{
	internal IService Service { get; set; }

	internal void Dispose()
	{
		(Service as IDisposable)?.Dispose();
		Service = null;
	}

	protected ServiceProxy()
	{
	}
}
public class ServiceProxy<T> : ServiceProxy
{
	public T Instance => (T)base.Service;

	internal ServiceProxy()
	{
	}
}

using System;

namespace Owlcat.Runtime.Core.Utility.EditorAttributes;

public class InfoBoxDisableScope : IDisposable
{
	public InfoBoxDisableScope()
	{
		InfoBoxAttribute.Disabled = true;
	}

	public void Dispose()
	{
		InfoBoxAttribute.Disabled = false;
	}
}

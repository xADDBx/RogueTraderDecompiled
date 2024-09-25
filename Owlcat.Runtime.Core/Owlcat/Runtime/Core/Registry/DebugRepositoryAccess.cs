using System;
using System.Linq;

namespace Owlcat.Runtime.Core.Registry;

public static class DebugRepositoryAccess
{
	public static object[] GetAllRegstries()
	{
		if (Repository.Instance != null)
		{
			return Repository.Instance.AllRegistries.Cast<object>().ToArray();
		}
		return Array.Empty<object>();
	}
}

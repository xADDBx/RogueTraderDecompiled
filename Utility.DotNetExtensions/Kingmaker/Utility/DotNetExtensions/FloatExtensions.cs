using UnityEngine;

namespace Kingmaker.Utility.DotNetExtensions;

public static class FloatExtensions
{
	public static bool Approximately(this float a, float b)
	{
		return Mathf.Approximately(a, b);
	}
}

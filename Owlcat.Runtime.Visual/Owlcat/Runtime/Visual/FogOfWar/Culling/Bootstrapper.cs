using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

internal static class Bootstrapper
{
	[RuntimeInitializeOnLoadMethod]
	private static void RuntimeInitializeOnLoad()
	{
		FogOfWarCulling.Initialize();
	}
}

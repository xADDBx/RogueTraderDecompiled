using UnityEngine;

namespace Kingmaker.Controllers.FogOfWar.Culling;

internal static class Bootstrapper
{
	[RuntimeInitializeOnLoadMethod]
	private static void RuntimeInitializeOnLoad()
	{
		FogOfWarCulling.Initialize();
	}
}

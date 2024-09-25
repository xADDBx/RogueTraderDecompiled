using System;
using Kingmaker.Controllers.FogOfWar.Culling;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.Controllers.FogOfWar;

internal static class FogOfWarBootstrapper
{
	[RuntimeInitializeOnLoadMethod]
	private static void RuntimeInitializeOnLoad()
	{
		Initialize();
	}

	private static void Initialize()
	{
		FogOfWarArea.AreaActivated = (FogOfWarArea.ActivateEventHandler)Delegate.Combine(FogOfWarArea.AreaActivated, new FogOfWarArea.ActivateEventHandler(OnAreaActivated));
		FogOfWarBlocker.BlockerActivated += OnBlockerActivated;
		FogOfWarBlocker.BlockerDeactivated += OnBlockerDeactivated;
		FogOfWarBlocker.BlockerChanged += OnBlockerChanged;
	}

	private static void Terminate()
	{
		FogOfWarArea.AreaActivated = (FogOfWarArea.ActivateEventHandler)Delegate.Remove(FogOfWarArea.AreaActivated, new FogOfWarArea.ActivateEventHandler(OnAreaActivated));
		FogOfWarBlocker.BlockerActivated -= OnBlockerActivated;
		FogOfWarBlocker.BlockerDeactivated -= OnBlockerDeactivated;
		FogOfWarBlocker.BlockerChanged -= OnBlockerChanged;
	}

	private static void OnAreaActivated(FogOfWarArea area)
	{
		LineOfSightGeometry.Instance.Init(area.GetWorldBounds());
	}

	private static void OnBlockerActivated(FogOfWarBlocker blocker)
	{
		if (!blocker.TryGetComponent<FogOfWarCullingBlocker>(out var component))
		{
			component = blocker.gameObject.AddComponent<FogOfWarCullingBlocker>();
		}
		component.Setup(blocker);
	}

	private static void OnBlockerDeactivated(FogOfWarBlocker blocker)
	{
		LineOfSightGeometry.Instance.RemoveBlocker(blocker);
		if (blocker.TryGetComponent<FogOfWarCullingBlocker>(out var component))
		{
			component.Cleanup();
		}
	}

	private static void OnBlockerChanged(FogOfWarBlocker blocker)
	{
		LineOfSightGeometry.Instance.UpdateBlocker(blocker);
		if (blocker.TryGetComponent<FogOfWarCullingBlocker>(out var component))
		{
			component.Rebuild();
		}
	}
}

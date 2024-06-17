using Kingmaker.Controllers.Interfaces;
using Owlcat.Runtime.Visual.FogOfWar.Culling;
using UnityEngine;

namespace Kingmaker.Controllers;

public class FogOfWarCompleteController : IControllerTick, IController
{
	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if ((!(FogOfWarControllerData.GetFogOfWarFeature() == null) || !Application.isPlaying) && !FogOfWarControllerData.Suppressed)
		{
			FogOfWarCulling.CompleteUpdate(applyCullingResults: true);
		}
	}
}

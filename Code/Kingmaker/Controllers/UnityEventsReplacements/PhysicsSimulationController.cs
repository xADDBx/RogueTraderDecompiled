using System;
using Kingmaker.Controllers.Interfaces;
using UnityEngine;

namespace Kingmaker.Controllers.UnityEventsReplacements;

public class PhysicsSimulationController : IControllerTick, IController
{
	private const long PhysicsStepInMilliseconds = 20L;

	private const float PhysicsStepInSeconds = 0.020000001f;

	private const long PhysicsStepInTicks = 200000L;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		TickInternal();
	}

	private static void TickInternal()
	{
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		TimeSpan timeSpan = gameTime - Game.Instance.TimeController.DeltaTimeSpan;
		long num = timeSpan.Ticks / 200000;
		long num2 = gameTime.Ticks / 200000 + 1;
		for (long num3 = num; num3 < num2; num3++)
		{
			TimeSpan timeSpan2 = new TimeSpan(num3 * 200000);
			if (timeSpan < timeSpan2 && timeSpan2 <= gameTime)
			{
				Physics.Simulate(0.020000001f);
			}
		}
	}
}

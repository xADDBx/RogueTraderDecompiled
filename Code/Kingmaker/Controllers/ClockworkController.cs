using System;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers;

public class ClockworkController : IControllerTick, IController
{
	public static event Action OnTick;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		ClockworkController.OnTick?.Invoke();
	}
}

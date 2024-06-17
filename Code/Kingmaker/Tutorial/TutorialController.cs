using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Tutorial;

public class TutorialController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		Game.Instance.Player.Tutorial.Tick();
	}
}

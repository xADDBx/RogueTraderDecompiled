using Core.Cheats;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers.StarSystem;

public class StarSystemTimeController : BaseGameTimeController, IControllerDisable, IController, IControllerTick
{
	[Cheat(Name = "space_time_multiplier")]
	public static float TimeMultiplier { get; set; } = 2880f;


	public void OnDisable()
	{
		Game.Instance.TimeController.GameTimeScale = 1f;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		Game.Instance.TimeController.GameTimeScale = ((Game.Instance.Player.PlayerShip.Commands.CurrentMoveTo != null) ? (TimeMultiplier * AdditionalTimeMultiplier) : AdditionalTimeMultiplier);
	}

	protected override void OnTimeStateChanged()
	{
	}
}

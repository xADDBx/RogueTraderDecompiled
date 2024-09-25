using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers;

public class UnpauseController : IControllerEnable, IController, IControllerDisable, IControllerTick
{
	private bool m_WasPressed;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Game.Instance.IsControllerMouse && Game.Instance.InvertPauseButtonPressed != m_WasPressed)
		{
			m_WasPressed = Game.Instance.InvertPauseButtonPressed;
			Game.Instance.IsPaused = !Game.Instance.InvertPauseButtonPressed;
		}
		Game.Instance.TimeController.PlayerTimeScale = (Game.Instance.InvertPauseButtonPressed ? 0.6f : 1f);
	}

	public void OnEnable()
	{
		m_WasPressed = Game.Instance.InvertPauseButtonPressed;
	}

	public void OnDisable()
	{
		Game.Instance.TimeController.PlayerTimeScale = 1f;
	}
}

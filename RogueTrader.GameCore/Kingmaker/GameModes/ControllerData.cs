using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.GameModes;

public class ControllerData
{
	public IController Controller { get; private set; }

	public GameModeType[] GameModes { get; private set; }

	public ControllerData(IController controller, IEnumerable<GameModeType> gameModes)
	{
		SetController(controller);
		SetGameModes(gameModes);
	}

	public void SetGameModes(IEnumerable<GameModeType> gameModes)
	{
		GameModes = gameModes.ToArray();
	}

	public void SetController(IController controller)
	{
		Controller = controller;
	}

	public override string ToString()
	{
		string text = string.Join(", ", GameModes.Select((GameModeType i) => i.ToString()));
		return Controller.GetType().Name + " (" + text + ")";
	}
}

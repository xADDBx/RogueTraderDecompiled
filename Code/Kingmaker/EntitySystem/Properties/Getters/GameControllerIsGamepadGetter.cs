using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("bcb9b1e65398460c95eb93db9b095180")]
public class GameControllerIsGamepadGetter : PropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Game Controller is Gamepad";
	}

	protected override int GetBaseValue()
	{
		if (Game.Instance.ControllerMode != Game.ControllerModeType.Gamepad)
		{
			return 0;
		}
		return 1;
	}
}

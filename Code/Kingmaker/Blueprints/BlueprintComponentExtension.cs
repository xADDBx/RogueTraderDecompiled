using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.Tutorial.Actions;
using Kingmaker.Tutorial.Etudes;

namespace Kingmaker.Blueprints;

public static class BlueprintComponentExtension
{
	public static bool IsInGameState(this BlueprintComponent component)
	{
		return component.CheckIsGameState();
	}

	public static bool CheckIsGameState(this BlueprintComponent blueprintComponent)
	{
		if (blueprintComponent is EtudeBracketEnableTutorials)
		{
			return false;
		}
		if (blueprintComponent is EtudeBracketEnableTutorialSingle)
		{
			return false;
		}
		if (blueprintComponent is UIEventTrigger uIEventTrigger)
		{
			GameAction[] actions = uIEventTrigger.Actions.Actions;
			for (int i = 0; i < actions.Length; i++)
			{
				if (actions[i] is ShowNewTutorial)
				{
					return false;
				}
			}
		}
		if (blueprintComponent is EtudePlayTrigger etudePlayTrigger)
		{
			GameAction[] actions = etudePlayTrigger.Actions.Actions;
			for (int i = 0; i < actions.Length; i++)
			{
				if (actions[i] is ShowNewTutorial)
				{
					return false;
				}
			}
		}
		return true;
	}
}

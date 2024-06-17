using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
public class ActionList
{
	[ValidateNotNull]
	[SerializeReference]
	public GameAction[] Actions = new GameAction[0];

	public bool HasActions
	{
		get
		{
			if (Actions != null)
			{
				return Actions.Length != 0;
			}
			return false;
		}
	}

	public void Run()
	{
		GameAction[] actions = Actions;
		foreach (GameAction gameAction in actions)
		{
			if (gameAction == null)
			{
				continue;
			}
			try
			{
				using (ElementsDebugScope.Open(gameAction))
				{
					gameAction.RunAction();
				}
			}
			catch (Exception ex)
			{
				if (CutscenePlayerDataScope.Current != null)
				{
					throw;
				}
				ElementLogicException exception = (ex as ElementLogicException) ?? new ElementLogicException(gameAction, ex);
				PFLog.Actions.ExceptionWithReport(exception, null);
			}
			finally
			{
			}
		}
	}
}

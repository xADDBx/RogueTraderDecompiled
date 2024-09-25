using System;
using System.Collections.Generic;
using Code.GameCore.ElementsSystem;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Utility.CodeTimer;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
public class ActionList : ElementsList, IHashable
{
	[ValidateNotNull]
	[SerializeReference]
	public GameAction[] Actions = new GameAction[0];

	public override IEnumerable<Element> Elements => Actions;

	public bool HasActions
	{
		get
		{
			GameAction[] actions = Actions;
			if (actions != null)
			{
				return actions.Length > 0;
			}
			return false;
		}
	}

	public void Run(bool @unsafe = false)
	{
		using (ProfileScope.New("ActionList"))
		{
			using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(this);
			Exception ex = null;
			GameAction[] actions = Actions;
			foreach (GameAction gameAction in actions)
			{
				try
				{
					gameAction?.Run(this);
				}
				catch (Exception ex2)
				{
					if (ex == null)
					{
						ex = ex2;
						elementsDebugger?.SetException(ex);
					}
					if (@unsafe || CutscenePlayerDataScope.Current != null)
					{
						throw;
					}
				}
			}
			if (ex == null)
			{
				elementsDebugger?.SetResult(1);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

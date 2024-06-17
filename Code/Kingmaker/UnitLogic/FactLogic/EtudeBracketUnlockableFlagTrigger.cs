using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("d1dfc17d49354a9b980afe5f01bac608")]
public class EtudeBracketUnlockableFlagTrigger : EtudeBracketTrigger, IUnlockHandler, ISubscriber, IUnlockValueHandler, IHashable
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnlockableFlagReference m_Flag;

	public bool RunActionsOnEnter;

	public ActionList OnUnlocked;

	public ActionList OnLocked;

	public ActionList OnChanged;

	public BlueprintUnlockableFlag Flag => m_Flag;

	void IUnlockHandler.HandleUnlock(BlueprintUnlockableFlag flag)
	{
		if (flag == Flag)
		{
			OnUnlocked.Run();
		}
	}

	void IUnlockHandler.HandleLock(BlueprintUnlockableFlag flag)
	{
		if (flag == Flag)
		{
			OnLocked.Run();
		}
	}

	void IUnlockValueHandler.HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		if (flag == Flag)
		{
			OnChanged.Run();
		}
	}

	protected override void OnEnter()
	{
		if (RunActionsOnEnter)
		{
			if (Flag.IsUnlocked)
			{
				OnUnlocked.Run();
			}
			else
			{
				OnLocked.Run();
			}
			OnChanged.Run();
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

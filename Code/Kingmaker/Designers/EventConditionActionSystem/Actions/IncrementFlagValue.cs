using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/IncrementFlagValue")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("f4ef63ec9a4741d4bae871a91255f89a")]
public class IncrementFlagValue : GameAction, IUnlockableFlagReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	[ValidateNotNull]
	[SerializeReference]
	public IntEvaluator Value;

	public bool UnlockIfNot;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();

	public override string GetDescription()
	{
		return $"Увеличивает значение ВЫДАННОГО флага {Flag} на указанную величину {Value}\n" + $"Выдавать ли флаг, если не выдан: {UnlockIfNot}";
	}

	public override string GetCaption()
	{
		return $"Increment unlocked flag value ({Flag})({Value})";
	}

	protected override void RunAction()
	{
		if (UnlockIfNot && !Flag.IsUnlocked)
		{
			Flag.Unlock();
		}
		if (Flag.IsUnlocked)
		{
			Flag.Value += Value.GetValue();
		}
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag == Flag)
		{
			return UnlockableFlagReferenceType.Unlock | UnlockableFlagReferenceType.SetValue;
		}
		return UnlockableFlagReferenceType.None;
	}
}

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/FlagInRange")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("342eb5cf69e9d0e48917ff4215600e0b")]
public class FlagInRange : Condition, IUnlockableFlagReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	public int MinValue;

	public int MaxValue;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();

	protected override string GetConditionCaption()
	{
		return $"Flag {Flag} value is in range [{MinValue};{MaxValue}]";
	}

	protected override bool CheckCondition()
	{
		if (Flag == null)
		{
			Element.LogError(this, "Flag in {0} is NULL", name);
			return false;
		}
		if (!Flag.IsUnlocked)
		{
			return false;
		}
		if (Flag.Value >= MinValue && Flag.Value <= MaxValue)
		{
			return true;
		}
		return false;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (Flag == flag)
		{
			return UnlockableFlagReferenceType.Check | UnlockableFlagReferenceType.CheckValue;
		}
		return UnlockableFlagReferenceType.None;
	}
}

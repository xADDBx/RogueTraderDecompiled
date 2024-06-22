using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/FlagUnlocked")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("fcc336266b98019488d20a50d120e05c")]
public class FlagUnlocked : Condition, IUnlockableFlagReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("ConditionFlag")]
	private BlueprintUnlockableFlagReference m_ConditionFlag;

	[Tooltip("False - white list. True - black list")]
	public bool ExceptSpecifiedValues;

	public List<int> SpecifiedValues = new List<int>();

	public BlueprintUnlockableFlag ConditionFlag
	{
		get
		{
			return m_ConditionFlag?.Get();
		}
		set
		{
			m_ConditionFlag = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintUnlockableFlagReference>();
		}
	}

	protected override string GetConditionCaption()
	{
		if (SpecifiedValues.Count <= 0)
		{
			return $"Unlocked ({ConditionFlag})";
		}
		string arg = string.Join(", ", SpecifiedValues.Select((int v) => v.ToString()));
		if (ExceptSpecifiedValues)
		{
			return $"Flag Value ({ConditionFlag} not {arg})";
		}
		return $"Flag Value ({ConditionFlag} is {arg})";
	}

	protected override bool CheckCondition()
	{
		if (ConditionFlag == null)
		{
			Element.LogError(this, "Flag in {0} is NULL", name);
			return false;
		}
		if (!ConditionFlag.IsUnlocked)
		{
			return false;
		}
		if (SpecifiedValues.Count == 0)
		{
			return true;
		}
		foreach (int specifiedValue in SpecifiedValues)
		{
			if (specifiedValue == ConditionFlag.Value)
			{
				return !ExceptSpecifiedValues;
			}
		}
		return ExceptSpecifiedValues;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag == ConditionFlag)
		{
			if (SpecifiedValues.Count <= 0)
			{
				return UnlockableFlagReferenceType.Check;
			}
			return UnlockableFlagReferenceType.Check | UnlockableFlagReferenceType.CheckValue;
		}
		return UnlockableFlagReferenceType.None;
	}
}

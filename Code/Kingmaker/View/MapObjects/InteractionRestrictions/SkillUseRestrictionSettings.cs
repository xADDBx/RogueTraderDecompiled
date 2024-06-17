using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public abstract class SkillUseRestrictionSettings
{
	public int DC;

	public bool Exact;

	public bool StartUnlocked;

	public bool IsPartyCheck;

	public bool InteractOnlyWithToolIfFailed { get; set; }

	public abstract StatType GetSkill();

	public abstract BlueprintItem GetItem();

	public void CopyDCData(SkillUseRestrictionSettings other)
	{
		DC = other.DC;
		Exact = other.Exact;
	}
}

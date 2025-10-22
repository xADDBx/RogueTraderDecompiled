using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public abstract class SkillUseRestrictionSettings
{
	public SkillCheckDifficulty Difficulty;

	[SerializeField]
	[ShowIf("DifficultyIsCustom")]
	private int DC;

	public bool StartUnlocked;

	public bool IsPartyCheck;

	public bool InteractOnlyWithToolIfFailed { get; set; }

	private bool DifficultyIsCustom => Difficulty == SkillCheckDifficulty.Custom;

	public abstract StatType GetSkill();

	public abstract BlueprintItem GetItem();

	public void CopyDCData(SkillUseRestrictionSettings other)
	{
		Difficulty = other.Difficulty;
		DC = other.DC;
	}

	public int GetDC()
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC(GetSkill());
		}
		return DC;
	}
}

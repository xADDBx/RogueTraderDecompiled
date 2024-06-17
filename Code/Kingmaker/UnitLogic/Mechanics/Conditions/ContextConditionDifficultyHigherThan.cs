using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Settings.Difficulty;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("60413a6d9c4dca742bcb441dabdbe47f")]
public class ContextConditionDifficultyHigherThan : ContextCondition
{
	public bool Less;

	public bool Reverse;

	public bool CheckOnlyForMonster = true;

	[SerializeField]
	private DifficultyPresetAsset m_Difficulty;

	public DifficultyPresetAsset CheckedDifficulty => m_Difficulty;

	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}

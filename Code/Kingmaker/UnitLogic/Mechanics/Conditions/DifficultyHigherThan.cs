using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Settings.Difficulty;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("8971cb2653f412b46ba71580482c29f6")]
public class DifficultyHigherThan : Condition
{
	public bool Less;

	[SerializeField]
	private DifficultyPresetAsset m_Difficulty;

	public DifficultyPresetAsset CheckedDifficulty => m_Difficulty;

	protected override bool CheckCondition()
	{
		return false;
	}

	protected override string GetConditionCaption()
	{
		return "Check if current difficulty is higher or equal (no context)";
	}
}

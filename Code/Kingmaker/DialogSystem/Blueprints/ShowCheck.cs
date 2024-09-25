using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[Serializable]
public class ShowCheck
{
	public StatType Type;

	public SkillCheckDifficulty Difficulty;

	[SerializeField]
	[ShowIf("DifficultyIsCustom")]
	private int DC;

	private bool DifficultyIsCustom => Difficulty == SkillCheckDifficulty.Custom;

	public int GetDC()
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC();
		}
		return DC;
	}

	public string GetDCString()
	{
		if (Difficulty != 0)
		{
			return Difficulty.ToString();
		}
		return $"Custom[{DC}]";
	}
}

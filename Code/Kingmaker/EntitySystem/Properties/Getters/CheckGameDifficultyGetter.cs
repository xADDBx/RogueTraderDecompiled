using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1892ce922fb24464a4d1572235596426")]
public class CheckGameDifficultyGetter : PropertyGetter
{
	[SerializeField]
	private GameDifficultyOption m_minDifficulty;

	public GameDifficultyOption MinDifficulty => m_minDifficulty;

	protected override string GetInnerCaption()
	{
		return $"Check current difficulty is [{MinDifficulty}]";
	}

	protected override int GetBaseValue()
	{
		if (SettingsController.Instance.DifficultyPresetsController.CurrentDifficultyCompareTo(MinDifficulty) < 0)
		{
			return 0;
		}
		return 1;
	}
}

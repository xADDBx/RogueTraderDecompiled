using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("cfdff8f2e2114414a5637e536a747269")]
public class CheckIronmanGetter : PropertyGetter
{
	[SerializeField]
	private bool m_IsInIronMan;

	public bool IsInIronMan => m_IsInIronMan;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check ironman is [{IsInIronMan}]";
	}

	protected override int GetBaseValue()
	{
		if (SettingsRoot.Difficulty.OnlyOneSave.GetValue() != m_IsInIronMan)
		{
			return 0;
		}
		return 1;
	}
}

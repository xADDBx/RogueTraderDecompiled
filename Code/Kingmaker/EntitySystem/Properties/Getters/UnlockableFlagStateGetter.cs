using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("f1550561ed1c44848d257af430758146")]
public class UnlockableFlagStateGetter : PropertyGetter
{
	[SerializeField]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"State of {Flag}";
	}

	protected override int GetBaseValue()
	{
		if (!Flag.IsUnlocked)
		{
			return 0;
		}
		return 1;
	}
}

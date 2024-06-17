using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions;

[TypeId("89ea65515aaf4922ac3ff4ba172a2081")]
public class FactionReputationLevelReached : Condition
{
	public enum CheckMode
	{
		Level,
		Points
	}

	[SerializeField]
	private FactionType m_Faction;

	[SerializeField]
	[HideIf("IsPoints")]
	private int m_ReputationLvl;

	[SerializeField]
	[ShowIf("IsPoints")]
	private int m_ReputationPoints;

	[SerializeField]
	private CheckMode m_CheckMode;

	public bool IsPoints => m_CheckMode == CheckMode.Points;

	protected override string GetConditionCaption()
	{
		return m_CheckMode switch
		{
			CheckMode.Level => $"{m_Faction} reputation lvl {m_ReputationLvl}", 
			CheckMode.Points => $"{m_Faction} reputation points {m_ReputationPoints}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override bool CheckCondition()
	{
		return m_CheckMode switch
		{
			CheckMode.Level => ReputationHelper.FactionReputationLevelReached(m_Faction, m_ReputationLvl), 
			CheckMode.Points => ReputationHelper.ReputationPointsReached(m_Faction, m_ReputationPoints), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}

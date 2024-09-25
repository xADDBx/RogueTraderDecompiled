using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[TypeId("3da4d82fd1064a8c82680cf4008b167a")]
public class AnomalyResearch : AnomalyInteraction, ICheckForLoot
{
	[SerializeField]
	public List<StatDC> Stats;

	[SerializeField]
	public List<LootEntry> CheckPassedLoot;

	[SerializeField]
	public List<LootEntry> CheckFailedLoot;

	[SerializeField]
	public ActionList Fail;

	[SerializeField]
	public ActionList Success;

	[NonSerialized]
	private SkillCheckResult m_SkillCheckResult;

	[NonSerialized]
	private RulePerformSkillCheck m_Rule;

	public override void Interact()
	{
		if (Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject is AnomalyEntityData entity)
		{
			EventBus.RaiseEvent(entity, delegate(IAnomalyResearchHandler e)
			{
				e.HandleAnomalyStartResearch();
			});
			EventBus.RaiseEvent(delegate(IStatCheckLootAnomalyHandler h)
			{
				h.HandleStatCheckLootStartCheck(this);
			});
		}
	}

	private SkillCheckResult HandleCheck(BaseUnitEntity unit, StatType statType)
	{
		if (!(Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject is AnomalyEntityData anomalyEntityData) || anomalyEntityData.Blueprint != base.OwnerBlueprint)
		{
			return null;
		}
		StatDC statDC = Stats.EmptyIfNull().FirstOrDefault((StatDC stat) => stat.Stat == statType);
		if (statDC == null)
		{
			return null;
		}
		RulePerformSkillCheck evt = new RulePerformSkillCheck(unit, statType, statDC.DC);
		m_Rule = Rulebook.Trigger(evt);
		ActionList actions = (m_Rule.ResultIsSuccess ? Success : Fail);
		Game.Instance.Player.StarSystemsState.StarSystemContextData.Setup(anomalyEntityData, unit);
		anomalyEntityData.MainFact.RunActionInContext(actions);
		anomalyEntityData.OnInteractionEnded();
		EventBus.RaiseEvent(anomalyEntityData, delegate(IAnomalyResearchHandler e)
		{
			e.HandleAnomalyResearched(unit, m_Rule);
		});
		EventBus.RaiseEvent(delegate(IStatCheckLootAnomalyHandler h)
		{
			h.HandleStatCheckLootChecked();
		});
		return new SkillCheckResult(m_Rule, unit);
	}

	public List<StatDC> GetStats()
	{
		return Stats;
	}

	public void Check(StatType type, BaseUnitEntity unit)
	{
		m_SkillCheckResult = HandleCheck(unit, type);
	}

	public SkillCheckResult GetCheckResult()
	{
		return m_SkillCheckResult;
	}

	public ILootable GetLoot()
	{
		AnomalyEntityData entity = Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject as AnomalyEntityData;
		return new LootFromPointOfInterestHolder(m_Rule.ResultIsSuccess ? CheckPassedLoot : CheckFailedLoot, entity);
	}
}

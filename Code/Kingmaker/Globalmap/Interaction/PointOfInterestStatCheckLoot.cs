using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestStatCheckLoot : BasePointOfInterest, IItemsCollectionHandler, ISubscriber, ICheckForLoot, IHashable
{
	private LootFromPointOfInterestHolder m_LootHolder;

	private StarSystemObjectEntity m_SsoEntity;

	[JsonProperty]
	private bool m_IsStatChecked;

	private bool m_IsStatStartChecking;

	private SkillCheckResult m_SkillCheckResult;

	public new BlueprintPointOfInterestStatCheckLoot Blueprint => (BlueprintPointOfInterestStatCheckLoot)base.Blueprint;

	public PointOfInterestStatCheckLoot(BlueprintPointOfInterestStatCheckLoot blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestStatCheckLoot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		if (base.Status == ExplorationStatus.Explored)
		{
			return;
		}
		if (m_IsStatChecked)
		{
			EventBus.RaiseEvent(delegate(IStatCheckLootPointOfInterestHandler h)
			{
				h.HandleStatCheckLootChecked();
			});
			return;
		}
		m_SsoEntity = entity;
		EventBus.RaiseEvent(delegate(IStatCheckLootPointOfInterestHandler h)
		{
			h.HandleStatCheckLootStartCheck(this);
		});
		if (!m_IsStatStartChecking)
		{
			EventBus.Subscribe(this);
		}
		m_IsStatStartChecking = true;
	}

	private void CheckStat(BaseUnitEntity unit, StatType statType)
	{
		RulePerformSkillCheck rulePerformSkillCheck = Rulebook.Trigger(new RulePerformSkillCheck(unit, statType, (Blueprint.Stats?.FirstOrDefault((StatDC statDC) => statDC.Stat == statType)?.DC).GetValueOrDefault()));
		m_SkillCheckResult = new SkillCheckResult(rulePerformSkillCheck, unit);
		List<LootEntry> lootCollection = (rulePerformSkillCheck.ResultIsSuccess ? Blueprint.CheckPassedLoot : Blueprint.CheckFailedLoot);
		m_SsoEntity.AddLootHolder(this, lootCollection);
		m_IsStatChecked = true;
		EventBus.RaiseEvent(delegate(IStatCheckLootPointOfInterestHandler h)
		{
			h.HandleStatCheckLootChecked();
		});
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		CheckStatus();
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		CheckStatus();
	}

	private void CheckStatus()
	{
		if (m_LootHolder != null && !m_LootHolder.Items.Any())
		{
			Game.Instance.GameCommandQueue.PointOfInterestSetInteracted(Blueprint);
			EventBus.Unsubscribe(this);
		}
	}

	public List<StatDC> GetStats()
	{
		return Blueprint.Stats;
	}

	public void Check(StatType type, BaseUnitEntity unit)
	{
		CheckStat(unit, type);
	}

	public SkillCheckResult GetCheckResult()
	{
		return m_SkillCheckResult;
	}

	public ILootable GetLoot()
	{
		m_LootHolder = m_SsoEntity.LootHolder.FirstOrDefault((LootFromPointOfInterestHolder lootHolder) => lootHolder.Point == this);
		if (m_LootHolder == null)
		{
			PFLog.Default.Warning("Cannot find lootHolder for point of interest");
		}
		return m_LootHolder;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_IsStatChecked);
		return result;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[TypeId("77fdfbe7d03b42bfad18c05814027fdf")]
public class ShapeFlames : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ITargetRulebookSubscriber, IHashable
{
	public static readonly List<DOT> SaveIgnoreList = new List<DOT>
	{
		DOT.Burning,
		DOT.PsykerBurning,
		DOT.NavigatorBurning
	};

	[SerializeField]
	private BlueprintBuffReference[] m_Buffs;

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.ShapeFlames.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.ShapeFlames.Release();
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		MechanicEntity mechanicEntity = evt.Target as MechanicEntity;
		if (!evt.IsMelee || mechanicEntity == null || !base.Owner.IsAlly(base.Context.MaybeCaster))
		{
			return;
		}
		foreach (Buff buff in base.Owner.Buffs.Enumerable.Where((Buff p) => Buffs.Contains(p.Blueprint)))
		{
			List<ContextActionDealDamage> list = new List<ContextActionDealDamage>();
			int num = 0;
			EntityFact.ComponentsEnumerable<AddFactContextActions> componentsEnumerable = buff.SelectComponents<AddFactContextActions>();
			EntityFact.ComponentsEnumerable<TurnBasedModeEventsTrigger> componentsEnumerable2 = buff.SelectComponents<TurnBasedModeEventsTrigger>();
			EntityFact.ComponentsEnumerable<DOTLogic> componentsEnumerable3 = buff.SelectComponents<DOTLogic>();
			foreach (AddFactContextActions item5 in componentsEnumerable)
			{
				GameAction[] actions = item5.NewRound.Actions;
				for (int i = 0; i < actions.Length; i++)
				{
					if (actions[i] is ContextActionDealDamage item)
					{
						list.Add(item);
					}
				}
				actions = item5.RoundEnd.Actions;
				for (int i = 0; i < actions.Length; i++)
				{
					if (actions[i] is ContextActionDealDamage item2)
					{
						list.Add(item2);
					}
				}
			}
			foreach (TurnBasedModeEventsTrigger item6 in componentsEnumerable2)
			{
				GameAction[] actions = item6.RoundStartActions.Actions;
				for (int i = 0; i < actions.Length; i++)
				{
					if (actions[i] is ContextActionDealDamage item3)
					{
						list.Add(item3);
					}
				}
				actions = item6.RoundEndActions.Actions;
				for (int i = 0; i < actions.Length; i++)
				{
					if (actions[i] is ContextActionDealDamage item4)
					{
						list.Add(item4);
					}
				}
			}
			foreach (DOTLogic item7 in componentsEnumerable3)
			{
				DOTLogic.Tick(buff, item7, onlyDamage: true, mechanicEntity);
			}
			if (!list.Empty())
			{
				num += list.Sum((ContextActionDealDamage p) => p.Value.Calculate(buff.Context));
				int value = list.Max((ContextActionDealDamage p) => p.Penetration.Calculate(buff.Context));
				num -= base.Owner.Stats.GetAttribute(StatType.WarhammerStrength).WarhammerBonus;
				RuleCalculateDamage ruleCalculateDamage = new CalculateDamageParams(base.Owner, mechanicEntity, evt.Ability, null, DamageType.Fire.CreateDamage(num, num), value, 1).Trigger();
				Rulebook.Trigger(new RuleDealDamage(base.Owner, mechanicEntity, ruleCalculateDamage.ResultDamage));
			}
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (base.Context.MaybeCaster != null && Buffs.Contains(evt.Reason.Context?.AssociatedBlueprint) && base.Owner.IsAlly(base.Context.MaybeCaster))
		{
			evt.ValueModifiers.Add(ModifierType.PctMul, 50, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

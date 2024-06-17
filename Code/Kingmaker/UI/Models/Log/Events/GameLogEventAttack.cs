using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventAttack : GameLogRuleEvent<RulePerformAttack, GameLogEventAttack>
{
	public class WeaponSkillModifierValues
	{
		public readonly int ResultValue;

		private readonly List<string> m_KeyOrders;

		private readonly Dictionary<string, int> m_Values;

		public WeaponSkillModifierValues(int resultValue, int baseValue)
		{
			ResultValue = resultValue;
			m_KeyOrders = new List<string>();
			m_Values = new Dictionary<string, int>();
			m_KeyOrders.Add("Base");
			m_Values.Add("Base", baseValue);
		}

		public void AddModifier(ModifiableValue.Modifier modifier)
		{
			if (modifier.ModValue != 0)
			{
				string text = StatModifiersBreakdown.GetBonusSourceText(modifier);
				if (string.IsNullOrEmpty(text))
				{
					text = LocalizedTexts.Instance.Descriptors.GetText(modifier.ModDescriptor);
				}
				if (!m_Values.ContainsKey(text))
				{
					m_KeyOrders.Add(text);
					m_Values.Add(text, 0);
				}
				m_Values[text] += modifier.ModValue;
			}
		}

		public void AddModifier(Modifier modifier)
		{
			if (modifier.Value != 0)
			{
				string text = StatModifiersBreakdown.GetBonusSourceText(modifier);
				if (string.IsNullOrEmpty(text))
				{
					text = LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
				}
				if (!m_Values.ContainsKey(text))
				{
					m_KeyOrders.Add(text);
					m_Values.Add(text, 0);
				}
				m_Values[text] += modifier.Value;
			}
		}

		public IEnumerable<Tuple<string, int>> GetModifiers()
		{
			foreach (string keyOrder in m_KeyOrders)
			{
				yield return new Tuple<string, int>(keyOrder, m_Values[keyOrder]);
			}
		}
	}

	[CanBeNull]
	private List<GameLogRuleEvent<RuleDealDamage>> m_TargetDamageList;

	private Dictionary<MechanicsFeatureType, List<FeatureCountableFlag.BuffList.Element>> m_AssociatedBuffs = new Dictionary<MechanicsFeatureType, List<FeatureCountableFlag.BuffList.Element>>();

	private readonly List<FeatureCountableFlag.BuffList.Element> m_Empty = new List<FeatureCountableFlag.BuffList.Element>();

	public WeaponSkillModifierValues InitiatorWeaponSkillModifierValues { get; private set; }

	public WeaponSkillModifierValues TargetWeaponSkillModifierValues { get; private set; }

	public MechanicEntity Attacker => base.Rule.ConcreteInitiator;

	public MechanicEntity Target => base.Rule.ConcreteTarget;

	public RulePerformAttackRoll RollPerformAttackRule => base.Rule.RollPerformAttackRule;

	public RuleRollDamage RollRuleDamage => base.Rule.RuleRollDamage;

	public int TargetDamageValue => m_TargetDamageList?.Sum((GameLogRuleEvent<RuleDealDamage> i) => i.Rule.Result) ?? 0;

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled)
			{
				if (Target is DestructibleEntity)
				{
					return TargetDamageValue != 0;
				}
				return true;
			}
			return false;
		}
	}

	public List<GameLogRuleEvent<RuleDealDamage>> TargetDamageList => m_TargetDamageList;

	public bool IsOverpenetrationTrigger { get; private set; }

	public bool IsPushTrigger { get; private set; }

	public int? AssassinLethality { get; private set; }

	public GameLogEventAttack(RulePerformAttack rule)
		: base(rule)
	{
		if (rule.RollPerformAttackRule.HitChanceRule.IsMelee)
		{
			HandleCriticalHit(rule.RollPerformAttackRule.HitChanceRule);
		}
		AssassinLethality = null;
		MechanicEntity concreteInitiator = rule.ConcreteInitiator;
		if (concreteInitiator != null && concreteInitiator.MainFact != null && concreteInitiator.IsPlayerFaction)
		{
			BaseUnitEntity initiatorUnit = rule.InitiatorUnit;
			if (initiatorUnit != null && initiatorUnit.HasAssassinCareer)
			{
				AssassinLethality = BlueprintRoot.Instance.AssassinLethalityProperty?.GetValue(new PropertyContext(rule.ConcreteInitiator.MainFact));
			}
		}
		foreach (Tuple<BaseUnitEntity, List<MechanicsFeatureType>> item2 in new List<Tuple<BaseUnitEntity, List<MechanicsFeatureType>>>
		{
			new Tuple<BaseUnitEntity, List<MechanicsFeatureType>>(base.Rule.InitiatorUnit, new List<MechanicsFeatureType>
			{
				MechanicsFeatureType.AutoHit,
				MechanicsFeatureType.AutoMiss
			}),
			new Tuple<BaseUnitEntity, List<MechanicsFeatureType>>(base.Rule.TargetUnit, new List<MechanicsFeatureType>
			{
				MechanicsFeatureType.AutoDodge,
				MechanicsFeatureType.AutoParry
			})
		})
		{
			if (item2.Item1 == null)
			{
				continue;
			}
			List<MechanicsFeatureType> item = item2.Item2;
			if (item == null || item.Count <= 0)
			{
				continue;
			}
			foreach (MechanicsFeatureType item3 in item2.Item2)
			{
				List<FeatureCountableFlag.BuffList.Element> list = item2.Item1.GetMechanicFeature(item3).AssociatedBuffs.Buffs.ToList();
				if (!m_AssociatedBuffs.ContainsKey(item3))
				{
					m_AssociatedBuffs.Add(item3, new List<FeatureCountableFlag.BuffList.Element>());
				}
				foreach (FeatureCountableFlag.BuffList.Element item4 in list)
				{
					if (!m_AssociatedBuffs[item3].Contains(item4))
					{
						m_AssociatedBuffs[item3].Add(item4);
					}
				}
			}
		}
	}

	private IReadOnlyList<FeatureCountableFlag.BuffList.Element> GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType type)
	{
		if (!m_AssociatedBuffs.TryGetValue(type, out var value))
		{
			return m_Empty;
		}
		return value;
	}

	public IReadOnlyList<FeatureCountableFlag.BuffList.Element> GetAutoHitAssociatedBuffs()
	{
		return GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType.AutoHit);
	}

	public IReadOnlyList<FeatureCountableFlag.BuffList.Element> GetAutoMissAssociatedBuffs()
	{
		return GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType.AutoMiss);
	}

	public IReadOnlyList<FeatureCountableFlag.BuffList.Element> GetAutoDodgeAssociatedBuffs()
	{
		return GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType.AutoDodge);
	}

	public IReadOnlyList<FeatureCountableFlag.BuffList.Element> GetAutoParryAssociatedBuffs()
	{
		return GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType.AutoParry);
	}

	private void HandleCriticalHit(RuleCalculateHitChances rule)
	{
		ModifiableValueAttributeStat attributeOptional = rule.ConcreteInitiator.GetAttributeOptional(StatType.WarhammerWeaponSkill);
		int resultValue = attributeOptional?.ModifiedValue ?? rule.InitiatorWeaponSkillValueModifiers.Value;
		int baseValue = attributeOptional?.BaseValue ?? 0;
		InitiatorWeaponSkillModifierValues = new WeaponSkillModifierValues(resultValue, baseValue);
		if (attributeOptional != null)
		{
			foreach (ModifiableValue.Modifier displayModifier in attributeOptional.GetDisplayModifiers())
			{
				InitiatorWeaponSkillModifierValues.AddModifier(displayModifier);
			}
		}
		foreach (Modifier item in rule.InitiatorWeaponSkillValueModifiers.List)
		{
			InitiatorWeaponSkillModifierValues.AddModifier(item);
		}
		ModifiableValueAttributeStat attributeOptional2 = rule.ConcreteTarget.GetAttributeOptional(StatType.WarhammerWeaponSkill);
		int resultValue2 = attributeOptional2?.ModifiedValue ?? rule.TargetWeaponSkillValueModifiers.Value;
		int baseValue2 = attributeOptional2?.BaseValue ?? 0;
		TargetWeaponSkillModifierValues = new WeaponSkillModifierValues(resultValue2, baseValue2);
		if (attributeOptional2 != null)
		{
			foreach (ModifiableValue.Modifier displayModifier2 in attributeOptional2.GetDisplayModifiers())
			{
				TargetWeaponSkillModifierValues.AddModifier(displayModifier2);
			}
		}
		foreach (Modifier item2 in rule.TargetWeaponSkillValueModifiers.List)
		{
			TargetWeaponSkillModifierValues.AddModifier(item2);
		}
	}

	protected override bool TryHandleInnerEventInternal(GameLogEvent @event)
	{
		if (@event is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.Rule.Target == Target)
		{
			if (m_TargetDamageList == null)
			{
				m_TargetDamageList = new List<GameLogRuleEvent<RuleDealDamage>>();
			}
			if (!m_TargetDamageList.Contains(gameLogRuleEvent))
			{
				m_TargetDamageList.Add(gameLogRuleEvent);
			}
		}
		if (@event is GameLogRuleEvent<RulePerformSkillCheck>)
		{
			return false;
		}
		return base.TryHandleInnerEventInternal(@event);
	}

	public void SetOverpenetrationTrigger(bool value)
	{
		IsOverpenetrationTrigger = value;
	}

	public void SetPushTrigger(bool value)
	{
		IsPushTrigger = value;
	}
}

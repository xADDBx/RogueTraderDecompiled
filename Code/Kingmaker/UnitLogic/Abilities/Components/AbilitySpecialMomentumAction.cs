using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("b8df90ef597f4a6f9fbb86bc2e18c656")]
public class AbilitySpecialMomentumAction : BlueprintComponent, IAbilityOnCastLogic, IAbilityCasterRestriction
{
	public MomentumAbilityType MomentumType;

	[SerializeField]
	private BlueprintUnitFactReference[] m_IgnoreUltimateCooldownFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> IgnoreUltimateCooldownFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] ignoreUltimateCooldownFacts = m_IgnoreUltimateCooldownFacts;
			return ignoreUltimateCooldownFacts;
		}
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		BlueprintMomentumRoot momentumRoot = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		if (caster.Facts.HasComponent<WarhammerFreeUltimateBuff>())
		{
			return true;
		}
		bool flag = false;
		if (IgnoreUltimateCooldownFacts.Count() != 0)
		{
			foreach (BlueprintUnitFact ignoreUltimateCooldownFact in IgnoreUltimateCooldownFacts)
			{
				if (caster.Facts.Contains(ignoreUltimateCooldownFact))
				{
					flag = true;
					break;
				}
			}
		}
		switch (MomentumType)
		{
		case MomentumAbilityType.HeroicAct:
			if (CheckHeroicAct(caster))
			{
				return !caster.Facts.Contains(momentumRoot.HeroicActBuff) || flag;
			}
			return false;
		case MomentumAbilityType.DesperateMeasure:
			if (CheckDesperateMeasure(caster))
			{
				return !caster.Facts.Contains(momentumRoot.HeroicActBuff) || flag;
			}
			return false;
		case MomentumAbilityType.Both:
			if (!CheckHeroicAct(caster))
			{
				return CheckDesperateMeasure(caster);
			}
			return true;
		default:
			return false;
		}
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		switch (MomentumType)
		{
		case MomentumAbilityType.HeroicAct:
			return CheckHeroicActPlayer(caster) ? LocalizedTexts.Instance.Reasons.AlreadyHeroicActed : LocalizedTexts.Instance.Reasons.NotYetHeroicAct;
		case MomentumAbilityType.DesperateMeasure:
		{
			if (!CheckDesperateMeasureBuffPlayer())
			{
				return LocalizedTexts.Instance.Reasons.AlreadyDesperateMeasuredThisTurn;
			}
			if (CheckDesperateMeasurePlayer(caster))
			{
				return LocalizedTexts.Instance.Reasons.AlreadyHeroicActed;
			}
			int dmThreshold = caster.GetDesperateMeasureThreshold();
			return LocalizedTexts.Instance.Reasons.NotYetDesperateMeasures.ToString(delegate
			{
				GameLogContext.Text = dmThreshold.ToString();
			});
		}
		case MomentumAbilityType.Both:
			return LocalizedTexts.Instance.Reasons.AlreadyHeroicActed;
		default:
			return LocalizedTexts.Instance.Reasons.NotEnoughMorale;
		}
	}

	public bool CheckDesperateMeasure(MechanicEntity caster)
	{
		int threshold = Math.Min(caster.GetDesperateMeasureThreshold() + (caster.Features.WitheringShard ? 50 : 0), 100);
		bool num = Game.Instance.TurnController.MomentumController.Groups.Any((MomentumGroup p) => p.Units.Contains(caster) && p.Momentum <= threshold);
		bool flag = CheckDesperateMeasureBuff(caster);
		return num && flag;
	}

	public bool CheckDesperateMeasurePlayer(MechanicEntity caster)
	{
		BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		int threshold = Math.Min(caster.GetDesperateMeasureThreshold() + (caster.Features.WitheringShard ? 50 : 0), 100);
		return Game.Instance.TurnController.MomentumController.Groups.Any((MomentumGroup p) => p.Blueprint == root.PartyGroup && p.Momentum <= threshold);
	}

	public bool CheckDesperateMeasureBuff(MechanicEntity caster)
	{
		BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		return !Game.Instance.TurnController.MomentumController.Groups.Any((MomentumGroup p1) => p1.Units.Contains(caster) && p1.Units.Any((EntityRef<MechanicEntity> p2) => p2.Entity.Facts.Contains(root.DesperateMeasureBuff)));
	}

	public bool CheckDesperateMeasureBuffPlayer()
	{
		BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		return !Game.Instance.TurnController.MomentumController.Groups.Any((MomentumGroup p1) => p1.Blueprint == root.PartyGroup && p1.Units.Any((EntityRef<MechanicEntity> p2) => p2.Entity.Facts.Contains(root.DesperateMeasureBuff)));
	}

	public bool CheckHeroicAct(MechanicEntity caster)
	{
		BlueprintMomentumRoot momentumRoot = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		int threshold = momentumRoot.HeroicActThreshold + (caster.Features.WitheringShard ? (-50) : 0);
		return Game.Instance.TurnController.MomentumController.Groups.Any((MomentumGroup p) => p.Units.Contains(caster) && p.Momentum >= threshold);
	}

	public bool CheckHeroicActPlayer(MechanicEntity mechanicEntity = null)
	{
		BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		int threshold = root.HeroicActThreshold + (mechanicEntity?.Features.WitheringShard ? (-50) : 0);
		return Game.Instance.TurnController.MomentumController.Groups.Any((MomentumGroup p) => p.Blueprint == root.PartyGroup && p.Momentum >= threshold);
	}

	public void OnCast(AbilityExecutionContext context)
	{
		BlueprintMomentumRoot momentumRoot = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		context.MaybeCaster?.Buffs.Add(momentumRoot.HeroicActBuff, context, BuffEndCondition.CombatEnd);
		if (MomentumType == MomentumAbilityType.DesperateMeasure)
		{
			context.MaybeCaster?.Buffs.Add(momentumRoot.DesperateMeasureBuff, context, 1.Rounds());
		}
	}
}

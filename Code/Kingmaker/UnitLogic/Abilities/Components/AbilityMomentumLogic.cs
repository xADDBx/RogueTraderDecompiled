using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("6c38aa4927ed44e7acde5844e8c777bb")]
public class AbilityMomentumLogic : BlueprintComponent, IAbilityOnCastLogic, IAbilityCasterRestriction
{
	public bool HeroicAct;

	[HideIf("HeroicAct")]
	public int Cost;

	[HideIf("HeroicAct")]
	public bool HasCostReducingFact;

	[SerializeField]
	[ShowIf("HasCostReducingFact")]
	private BlueprintUnitFactReference m_CostReducingFact;

	[ShowIf("HasCostReducingFact")]
	public int CostReduction;

	[SerializeField]
	public ConditionsChecker ConditionsOnAdditionCost;

	[SerializeField]
	public ContextValue AdditionalCost;

	public BlueprintUnitFact CostReducingFact => m_CostReducingFact.Get();

	public void OnCast(AbilityExecutionContext context)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			return;
		}
		MomentumGroup group = Game.Instance.TurnController.MomentumController.GetGroup(maybeCaster);
		if (group == null)
		{
			return;
		}
		BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		int num = ((!HeroicAct) ? ((HasCostReducingFact && maybeCaster.Facts.Contains(CostReducingFact)) ? (Cost - CostReduction) : Cost) : ((!maybeCaster.Facts.GetComponents<WarhammerFreeUltimateBuff>().Any((WarhammerFreeUltimateBuff buff) => buff.NoMomentumCost)) ? (group.Units.Sum((EntityRef<MechanicEntity> p) => (p.Entity?.Facts.Get(root.HeroicActBuffCounter)?.GetRank()).GetValueOrDefault()) * 25 + 75) : 0));
		ConditionsChecker conditionsOnAdditionCost = ConditionsOnAdditionCost;
		if (conditionsOnAdditionCost != null && conditionsOnAdditionCost.HasConditions)
		{
			using (context.GetDataScope(maybeCaster.ToITargetWrapper()))
			{
				if (ConditionsOnAdditionCost.Check())
				{
					num += AdditionalCost.Calculate(context);
				}
			}
		}
		int cost = num;
		RuleReason reason = context;
		Rulebook.Trigger(RulePerformMomentumChange.CreateAbilityCost(maybeCaster, cost, in reason));
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		return Game.Instance.TurnController.MomentumController.Groups.Any(delegate(MomentumGroup p)
		{
			BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
			MomentumGroup group = Game.Instance.TurnController.MomentumController.GetGroup(caster);
			if (group == null)
			{
				return false;
			}
			int num = ((!HeroicAct) ? ((HasCostReducingFact && caster.Facts.Contains(CostReducingFact)) ? (Cost - CostReduction) : Cost) : ((!caster.Facts.GetComponents<WarhammerFreeUltimateBuff>().Any((WarhammerFreeUltimateBuff buff) => buff.NoMomentumCost)) ? (group.Units.Count((EntityRef<MechanicEntity> p) => p.Entity.Facts.Contains(root.HeroicActBuff)) * 25 + 75) : 0));
			return p.Units.Contains(caster) && p.Momentum >= num;
		});
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NotEnoughMorale;
	}
}

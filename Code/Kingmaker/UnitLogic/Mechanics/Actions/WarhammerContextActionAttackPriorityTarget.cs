using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("ce34afee78ed4a3590f7d3d9ebd9fa1d")]
public class WarhammerContextActionAttackPriorityTarget : ContextAction
{
	public enum PriorityTargetAttackSelectType
	{
		HighestCost,
		LowestCost
	}

	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public PriorityTargetAttackSelectType AttackSelectType;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	protected override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null || !(mechanicsContext.MainTarget.Entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity2 = mechanicsContext.MaybeCaster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		if (baseUnitEntity2 != null)
		{
			Ability ability = SelectAttackAbility(baseUnitEntity, baseUnitEntity2, AttackSelectType);
			if (ability != null)
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, baseUnitEntity2)
				{
					IgnoreCooldown = true,
					FreeAction = true
				};
				baseUnitEntity.Commands.AddToQueue(cmdParams);
			}
		}
	}

	public static Ability SelectAttackAbility(MechanicEntity target, BaseUnitEntity priorityTarget, PriorityTargetAttackSelectType attackSelectType)
	{
		IEnumerable<Ability> all = target.Facts.GetAll(delegate(Ability p)
		{
			if (p.Blueprint.GetComponent<AbilityDeliverAttackWithWeapon>() == null)
			{
				return false;
			}
			if (p.Data.GetPatternSettings() != null)
			{
				return false;
			}
			ItemEntity itemEntity = (ItemEntity)p.SourceItem;
			if (itemEntity != null && !(itemEntity.Blueprint is BlueprintItemWeapon))
			{
				return false;
			}
			return !p.Data.IsRestricted && p.Data.RangeCells >= target.DistanceToInCells(priorityTarget);
		});
		return attackSelectType switch
		{
			PriorityTargetAttackSelectType.HighestCost => all.MaxBy((Ability p) => p.Data.CalculateActionPointCost()), 
			PriorityTargetAttackSelectType.LowestCost => all.MinBy((Ability p) => p.Data.CalculateActionPointCost()), 
			_ => all.FirstOrDefault(), 
		};
	}

	public override string GetCaption()
	{
		return "Command to attack priority target";
	}
}

using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("3ddc7144d39a42faac68d5a6519189f7")]
public class WarhammerContextActionAttackNearestTarget : ContextAction
{
	public enum PriorityTargetAttackSelectType
	{
		HighestCost,
		LowestCost,
		LowestCostButBurst,
		LowestCostButAoE,
		LowestCostButBurstOrAoE
	}

	public PriorityTargetAttackSelectType AttackSelectType;

	public bool OnlyEnemies;

	[HideIf("OnlyEnemies")]
	public bool OnlyAllies;

	[HideIf("OnlyRanged")]
	public bool OnlyMelee;

	[HideIf("OnlyMelee")]
	public bool OnlyRanged;

	public List<BlueprintAbilityGroupReference> ViableGroups = new List<BlueprintAbilityGroupReference>();

	public override void RunAction()
	{
		if (ContextData<MechanicsContext.Data>.Current?.Context == null)
		{
			return;
		}
		MechanicEntity target = base.Target.Entity;
		if (target == null || (target is BaseUnitEntity baseUnitEntity && !baseUnitEntity.State.CanAct))
		{
			return;
		}
		List<MechanicEntity> list = Game.Instance.State.AllUnits.Where((AbstractUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && p.Health.HitPointsLeft > 0).Cast<MechanicEntity>().ToList();
		list.Remove(target);
		if (OnlyEnemies)
		{
			list.RemoveAll((MechanicEntity p) => !p.IsEnemy(target));
		}
		if (OnlyAllies)
		{
			list.RemoveAll((MechanicEntity p) => !p.IsAlly(target));
		}
		list.RemoveAll((MechanicEntity p) => !target.HasLOS(p));
		MechanicEntity mechanicEntity = list.MinBy((MechanicEntity p) => target.DistanceToInCells(p));
		Ability ability = SelectAttackAbility(target, mechanicEntity, AttackSelectType, ViableGroups, OnlyMelee, OnlyRanged);
		if (ability != null)
		{
			PartUnitCommands commandsOptional = target.GetCommandsOptional();
			if (commandsOptional != null)
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, mechanicEntity)
				{
					IgnoreCooldown = true,
					FreeAction = true
				};
				commandsOptional.AddToQueue(cmdParams);
			}
			else
			{
				Rulebook.Trigger(new RulePerformAbility(ability.Data, mechanicEntity)
				{
					IgnoreCooldown = true,
					ForceFreeAction = true
				});
			}
		}
	}

	public static Ability SelectAttackAbility(MechanicEntity target, MechanicEntity priorityTarget, PriorityTargetAttackSelectType attackSelectType, List<BlueprintAbilityGroupReference> viableGroups, bool onlyMelee = false, bool onlyRanged = false)
	{
		IEnumerable<Ability> all = target.Facts.GetAll(delegate(Ability p)
		{
			if (p.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>() == null)
			{
				return false;
			}
			ItemEntity itemEntity = (ItemEntity)p.SourceItem;
			if (itemEntity != null && !(itemEntity.Blueprint is BlueprintItemWeapon))
			{
				return false;
			}
			if (p.Data.IsRestricted)
			{
				return false;
			}
			ItemEntityWeapon weapon = p.Data.Weapon;
			if (!((weapon == null || !weapon.Blueprint.IsMelee) && onlyMelee))
			{
				ItemEntityWeapon weapon2 = p.Data.Weapon;
				if (!((weapon2 == null || !weapon2.Blueprint.IsRanged) && onlyRanged))
				{
					if (!viableGroups.Empty() && !viableGroups.Any((BlueprintAbilityGroupReference group) => p.Blueprint.AbilityGroups.Contains(group.Get())))
					{
						return false;
					}
					return p.Data.CanTarget(priorityTarget);
				}
			}
			return false;
		});
		switch (attackSelectType)
		{
		case PriorityTargetAttackSelectType.HighestCost:
			return all.MaxBy((Ability p) => p.Data.CalculateActionPointCost());
		case PriorityTargetAttackSelectType.LowestCost:
			return all.MinBy((Ability p) => p.Data.CalculateActionPointCost());
		case PriorityTargetAttackSelectType.LowestCostButBurst:
		{
			IEnumerable<Ability> enumerable2 = all.Where((Ability p) => p.Blueprint.IsBurst);
			if (!enumerable2.Any())
			{
				return all.MinBy((Ability p) => p.Data.CalculateActionPointCost());
			}
			return enumerable2.MaxBy((Ability p) => p.Data.CalculateActionPointCost());
		}
		case PriorityTargetAttackSelectType.LowestCostButAoE:
		{
			IEnumerable<Ability> enumerable3 = all.Where((Ability p) => p.Blueprint.IsAoE);
			if (!enumerable3.Any())
			{
				return all.MinBy((Ability p) => p.Data.CalculateActionPointCost());
			}
			return enumerable3.MaxBy((Ability p) => p.Data.CalculateActionPointCost());
		}
		case PriorityTargetAttackSelectType.LowestCostButBurstOrAoE:
		{
			IEnumerable<Ability> enumerable = all.Where((Ability p) => p.Blueprint.IsBurst || p.Blueprint.IsAoE);
			if (!enumerable.Any())
			{
				return all.MinBy((Ability p) => p.Data.CalculateActionPointCost());
			}
			return enumerable.MaxBy((Ability p) => p.Data.CalculateActionPointCost());
		}
		default:
			return all.FirstOrDefault();
		}
	}

	public override string GetCaption()
	{
		return "Command to attack nearest target";
	}
}

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("c635b9ab19e16be419c637e0474dc6a9")]
public class ContextActionAttackWithFirstWeaponAbility : ContextAction
{
	public bool TargetIsPriorityTarget;

	[ShowIf("TargetIsPriorityTarget")]
	[SerializeField]
	private BlueprintBuffReference m_PriorityTargetBuff;

	public bool useSecondWeapon;

	public bool OwnerIsAttacker;

	public bool SaveMPAfterUsingWeaponAbility;

	public bool ContextCasterIsTarget;

	public BlueprintBuff PriorityTargetBuff => m_PriorityTargetBuff?.Get();

	protected override void RunAction()
	{
		if (!(base.Caster is BaseUnitEntity baseUnitEntity))
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		BaseUnitEntity baseUnitEntity2 = (OwnerIsAttacker ? ((BaseUnitEntity)base.Context.MaybeOwner) : baseUnitEntity);
		if (baseUnitEntity2 == null)
		{
			Element.LogError(this, "Caster is missing");
		}
		else
		{
			if ((bool)baseUnitEntity2.Features.CantAct)
			{
				return;
			}
			MechanicEntity mechanicEntity = base.Target.Entity;
			if (ContextCasterIsTarget)
			{
				mechanicEntity = base.Context.MaybeCaster;
			}
			if (TargetIsPriorityTarget)
			{
				BaseUnitEntity baseUnitEntity3 = baseUnitEntity2.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(PriorityTargetBuff);
				if (baseUnitEntity3 == null)
				{
					return;
				}
				mechanicEntity = baseUnitEntity3;
			}
			if (mechanicEntity == null)
			{
				Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
				return;
			}
			ItemEntityWeapon itemEntityWeapon = ((!useSecondWeapon) ? baseUnitEntity2.GetFirstWeapon() : baseUnitEntity2.GetSecondaryHandWeapon());
			if (itemEntityWeapon == null)
			{
				Element.LogError(this, "No weapon in hand");
				return;
			}
			Ability ability = itemEntityWeapon.Abilities[0];
			if (ability == null)
			{
				Element.LogError(this, "No ability in weapon");
			}
			else
			{
				if (!ability.Data.CanTarget(mechanicEntity, out var _))
				{
					return;
				}
				if (SaveMPAfterUsingWeaponAbility)
				{
					PartUnitCombatState combatStateOptional = baseUnitEntity2.GetCombatStateOptional();
					if (combatStateOptional != null)
					{
						combatStateOptional.SaveMPAfterUsingNextAbility = true;
					}
				}
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, mechanicEntity)
				{
					IgnoreCooldown = true,
					FreeAction = true,
					OriginatedFrom = base.Context.SourceAbility
				};
				baseUnitEntity2.Commands.AddToQueue(cmdParams);
			}
		}
	}

	public override string GetCaption()
	{
		return "Attack target with first weapon ability.";
	}

	public DamagePredictionData GetDamagePrediction(AbilityExecutionContext context, MechanicEntity targetEntity, Vector3 casterPosition)
	{
		if (context.Caster == targetEntity)
		{
			return null;
		}
		if (!(context.Caster is BaseUnitEntity baseUnitEntity))
		{
			Element.LogError(this, "Caster is missing");
			return null;
		}
		BaseUnitEntity baseUnitEntity2 = (OwnerIsAttacker ? ((BaseUnitEntity)base.Context.MaybeOwner) : baseUnitEntity);
		if (baseUnitEntity2 == null)
		{
			Element.LogError(this, "Caster is missing");
			return null;
		}
		if ((bool)baseUnitEntity2.Features.CantAct)
		{
			return null;
		}
		ItemEntityWeapon itemEntityWeapon = ((!useSecondWeapon) ? baseUnitEntity2.GetFirstWeapon() : baseUnitEntity2.GetSecondaryHandWeapon());
		if (itemEntityWeapon == null)
		{
			Element.LogError(this, "No weapon in hand");
			return null;
		}
		Ability ability = itemEntityWeapon.Abilities[0];
		if (ability == null)
		{
			Element.LogError(this, "No ability in weapon");
			return null;
		}
		return ability.Data.GetDamagePrediction(targetEntity, casterPosition);
	}
}

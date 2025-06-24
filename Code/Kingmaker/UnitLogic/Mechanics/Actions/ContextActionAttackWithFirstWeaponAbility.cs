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
using Kingmaker.Utility.DotNetExtensions;
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
		BaseUnitEntity attacker = GetAttacker(base.Context, base.Caster);
		MechanicEntity mechanicEntity = base.Target.Entity;
		if (ContextCasterIsTarget)
		{
			mechanicEntity = base.Context.MaybeCaster;
		}
		if (TargetIsPriorityTarget)
		{
			BaseUnitEntity baseUnitEntity = attacker.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(PriorityTargetBuff);
			if (baseUnitEntity == null)
			{
				return;
			}
			mechanicEntity = baseUnitEntity;
		}
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
			return;
		}
		AbilityData ability = GetAbility(attacker);
		if (ability == null || !ability.CanTarget(mechanicEntity, out var _))
		{
			return;
		}
		if (SaveMPAfterUsingWeaponAbility)
		{
			PartUnitCombatState combatStateOptional = attacker.GetCombatStateOptional();
			if (combatStateOptional != null)
			{
				combatStateOptional.SaveMPAfterUsingNextAbility = true;
			}
		}
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability, mechanicEntity)
		{
			IgnoreCooldown = true,
			FreeAction = true,
			OriginatedFrom = base.Context.SourceAbility
		};
		attacker.Commands.AddToQueue(cmdParams);
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
		BaseUnitEntity attacker = GetAttacker(context, context.Caster);
		if (attacker == null)
		{
			return null;
		}
		return GetAbility(attacker)?.Data.GetDamagePrediction(targetEntity, casterPosition);
	}

	private BaseUnitEntity GetAttacker(MechanicsContext context, MechanicEntity casterEntity)
	{
		if (!(casterEntity is BaseUnitEntity baseUnitEntity))
		{
			Element.LogError(this, "Caster is missing");
			return null;
		}
		BaseUnitEntity baseUnitEntity2 = (OwnerIsAttacker ? ((BaseUnitEntity)context.MaybeOwner) : baseUnitEntity);
		if (baseUnitEntity2 == null)
		{
			Element.LogError(this, "Caster is missing");
			return null;
		}
		if ((bool)baseUnitEntity2.Features.CantAct)
		{
			return null;
		}
		return baseUnitEntity2;
	}

	private AbilityData GetAbility(BaseUnitEntity attacker)
	{
		ItemEntityWeapon itemEntityWeapon = ((!useSecondWeapon) ? attacker.GetFirstWeapon() : attacker.GetSecondaryHandWeapon());
		if (itemEntityWeapon == null)
		{
			Element.LogError(this, "No weapon in hand");
			return null;
		}
		Ability ability = itemEntityWeapon.Shield?.Abilities.Get(0) ?? itemEntityWeapon.Abilities.Get(0);
		if (ability == null)
		{
			Element.LogError(this, "No ability in weapon");
			return null;
		}
		return ability.Data;
	}
}

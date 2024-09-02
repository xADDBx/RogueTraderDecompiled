using System;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("962e4ff1f0264d03bba1bf1df5fc70ef")]
public class CommandUnitAttack : CommandBase
{
	private class Data
	{
		public AbstractUnitEntity Unit { get; set; }

		public MechanicEntity Target { get; set; }

		public ItemEntityWeapon Weapon { get; set; }

		public AbilityData Ability { get; set; }

		[CanBeNull]
		public UnitCommandHandle MoveCmdHandle { get; set; }

		[CanBeNull]
		public UnitCommandHandle AttackCmdHandle { get; set; }

		public bool CutsceneWeaponSet { get; set; }

		public bool TakingTooLong { get; set; }

		public bool Interrupted { get; set; }

		public bool IsTryingToSkip { get; set; }

		public IAbilityCustomAnimation CustomAnimation { get; set; }

		public Path Path { get; set; }
	}

	public enum AttackType
	{
		SingleShot,
		BurstFire,
		Melee,
		Custom
	}

	public AttackType Type;

	public bool Continuous;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ConditionalShow("OverrideSpeed")]
	public float Speed = 5f;

	[SerializeReference]
	public MechanicEntityEvaluator Target;

	[SerializeField]
	[ShowIf("IsCustomWeaponVisible")]
	private BlueprintItemWeaponReference m_CustomWeapon;

	public AttackHitPolicyType HitPolicy = AttackHitPolicyType.AutoHit;

	public DamagePolicyType DamagePolicy = DamagePolicyType.FxOnly;

	[Tooltip("Won't kill if Command is set to Repeat (Continuous)")]
	public bool KillTarget;

	public bool NeedLoS;

	public bool EnableLog;

	public bool MuteAttacker;

	private bool IsCustomWeaponVisible => Type == AttackType.Custom;

	public BlueprintItemWeapon CustomWeapon => m_CustomWeapon;

	public BlueprintAbilityFXSettings.Reference SpellWeaponFXSettings => BlueprintWarhammerRoot.Instance.CutsceneRoot.SpellWeaponFXSettings;

	public override bool IsContinuous => Continuous;

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (Unit == null || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		AbstractUnitEntity unit = Unit.GetValue();
		AttackType attackType = Type;
		BlueprintItemWeapon blueprintItemWeapon;
		if (Type == AttackType.Custom && CustomWeapon != null)
		{
			blueprintItemWeapon = CustomWeapon;
		}
		else if (Type == AttackType.SingleShot)
		{
			(blueprintItemWeapon, attackType) = FindSuitableSingleShot(unit);
		}
		else
		{
			blueprintItemWeapon = FindSuitableWeapon(unit, Type, activeSet: true, notActiveSets: true);
		}
		if (blueprintItemWeapon == null)
		{
			if (Type == AttackType.Melee)
			{
				blueprintItemWeapon = BlueprintWarhammerRoot.Instance.CutsceneRoot.DefaultWeaponMelee;
			}
			else if (Type == AttackType.SingleShot || Type == AttackType.BurstFire)
			{
				blueprintItemWeapon = BlueprintWarhammerRoot.Instance.CutsceneRoot.DefaultWeaponRanged;
			}
		}
		if (blueprintItemWeapon == null)
		{
			throw new Exception("Can't find suitable weapon");
		}
		BlueprintAbility abilityBlueprint = GetAbilityBlueprint(attackType, blueprintItemWeapon, Type);
		bool cutsceneWeaponSet = false;
		ItemEntityWeapon weapon = GetWeapon(unit, blueprintItemWeapon, ref cutsceneWeaponSet, ref abilityBlueprint);
		AbilityData ability = new AbilityData(abilityBlueprint, unit)
		{
			OverrideWeapon = weapon,
			FXSettingsOverride = (CanOverrideBecauseOfStaffWeapon(weapon.Blueprint, Type) ? SpellWeaponFXSettings.Get() : FindSuitableFXSettings(weapon, attackType))
		};
		MechanicEntity target = Target.GetValue();
		float distanceToTarget = unit.DistanceToInCells(target);
		bool num = CalculateNeedForApproach(distanceToTarget, weapon, abilityBlueprint);
		IAbilityCustomAnimation abilityCustomAnimation = blueprintItemWeapon.WeaponAbilities.FirstOrDefault()?.Ability?.GetComponent<IAbilityCustomAnimation>();
		Data data = player.GetCommandData<Data>(this);
		data.Unit = unit;
		data.Target = target;
		data.Ability = ability;
		data.Weapon = weapon;
		data.CutsceneWeaponSet = cutsceneWeaponSet;
		data.CustomAnimation = abilityCustomAnimation;
		if (Continuous && unit.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(inCombat: true);
		}
		if (skipping && KillTarget)
		{
			target.GetLifeStateOptional().MarkedForDeath = true;
			PFLog.Default.Log($"CommandUnitAttack.OnRun() skipping cutscene and killing target {target} by unit: {unit}");
		}
		if (num)
		{
			float distance = weapon.AttackRange;
			data.Path = PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, target.Position, distance, 1, delegate(ForcedPath path)
			{
				data.Path = null;
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else
				{
					UnitMoveToParams cmdParams2 = new UnitMoveToParams(path, target.Position, distance)
					{
						OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null),
						MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation)
					};
					data.MoveCmdHandle = unit.Commands.Run(cmdParams2);
				}
			});
		}
		else
		{
			UnitUseAbilityParams cmdParams = CreateAttackCommandParams(ability, target, abilityCustomAnimation);
			data.AttackCmdHandle = unit.Commands.Run(cmdParams);
		}
		if (MuteAttacker)
		{
			AkSoundEngine.SetRTPCValue("MuteEntity", 0f, Unit.GetValue().View.gameObject);
		}
	}

	private bool CalculateNeedForApproach(float distanceToTarget, ItemEntityWeapon weapon, BlueprintAbility abilityBlueprint)
	{
		if (CanOverrideBecauseOfStaffWeapon(weapon.Blueprint, Type) && abilityBlueprint.Range == AbilityRange.Custom)
		{
			return distanceToTarget > (float)abilityBlueprint.CustomRange;
		}
		return distanceToTarget > (float)weapon.AttackRange;
	}

	private static bool CanOverrideBecauseOfStaffWeapon(BlueprintItemWeapon weapon, AttackType type)
	{
		if (type != AttackType.Custom)
		{
			return weapon.VisualParameters.AnimStyle == WeaponAnimationStyle.Staff;
		}
		return false;
	}

	private static BlueprintAbility GetAbilityBlueprint(AttackType actualType, BlueprintItemWeapon weaponBlueprint, AttackType type)
	{
		if (CanOverrideBecauseOfStaffWeapon(weaponBlueprint, type))
		{
			return BlueprintWarhammerRoot.Instance.CutsceneRoot.AttackSpell;
		}
		return actualType switch
		{
			AttackType.SingleShot => BlueprintWarhammerRoot.Instance.CutsceneRoot.AttackSingle, 
			AttackType.BurstFire => BlueprintWarhammerRoot.Instance.CutsceneRoot.AttackBurst, 
			AttackType.Melee => BlueprintWarhammerRoot.Instance.CutsceneRoot.AttackSingle, 
			AttackType.Custom => BlueprintWarhammerRoot.Instance.CutsceneRoot.AttackSingle, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private ItemEntityWeapon GetWeapon(AbstractUnitEntity unit, BlueprintItemWeapon weaponBlueprint, ref bool cutsceneWeaponSet, ref BlueprintAbility abilityBlueprint)
	{
		ItemEntityWeapon itemEntityWeapon = null;
		PartUnitBody bodyOptional = unit.GetBodyOptional();
		if (bodyOptional == null)
		{
			return null;
		}
		foreach (HandSlot item in bodyOptional.HandsEquipmentSets.SelectMany((HandsEquipmentSet set) => set.Hands))
		{
			if (item.MaybeWeapon?.Blueprint == weaponBlueprint)
			{
				itemEntityWeapon = item.Weapon;
				break;
			}
		}
		if (itemEntityWeapon == null)
		{
			itemEntityWeapon = weaponBlueprint.CreateEntity<ItemEntityWeapon>();
			bodyOptional.SetCutsceneHandsEquipment(itemEntityWeapon);
			cutsceneWeaponSet = true;
		}
		return itemEntityWeapon;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.MoveCmdHandle?.Interrupt();
		if (commandData.Interrupted)
		{
			commandData.AttackCmdHandle?.Interrupt();
		}
		if (commandData.CutsceneWeaponSet)
		{
			commandData.Unit.GetBodyOptional()?.SetCutsceneHandsEquipment(null);
		}
		if (Continuous && commandData.Unit.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(inCombat: false);
		}
		player.ClearCommandData(this);
		if (MuteAttacker)
		{
			AkSoundEngine.SetRTPCValue("MuteEntity", 1f, Unit.GetValue().View.gameObject);
		}
		base.OnStop(player);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		bool flag = !player.Cutscene.NonSkippable;
		Data commandData = player.GetCommandData<Data>(this);
		if (Continuous && (!flag || !commandData.Interrupted))
		{
			return false;
		}
		if (commandData.TakingTooLong)
		{
			return true;
		}
		if (commandData.MoveCmdHandle != null)
		{
			return false;
		}
		if (commandData.Path != null)
		{
			return false;
		}
		UnitUseAbility unitUseAbility = (UnitUseAbility)(commandData.AttackCmdHandle?.Cmd);
		if (unitUseAbility == null)
		{
			return true;
		}
		if (unitUseAbility.IsFinished)
		{
			return true;
		}
		if (commandData.Unit.IsDeadOrUnconscious)
		{
			return true;
		}
		if (unitUseAbility.IsActed && unitUseAbility.ExecutionProcess.IsEnded)
		{
			return commandData.Interrupted;
		}
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (time > 20.0 && !Continuous)
		{
			player.GetCommandData<Data>(this).TakingTooLong = true;
			return;
		}
		Data commandData = player.GetCommandData<Data>(this);
		UnitUseAbility unitUseAbility = (UnitUseAbility)(commandData.AttackCmdHandle?.Cmd);
		UnitUseAbilityParams unitUseAbilityParams = null;
		UnitCommandHandle moveCmdHandle = commandData.MoveCmdHandle;
		if (moveCmdHandle != null && moveCmdHandle.IsFinished)
		{
			commandData.MoveCmdHandle = null;
			unitUseAbilityParams = CreateAttackCommandParams(commandData.Ability, commandData.Target, commandData.CustomAnimation);
		}
		else if (IsContinuous && commandData.MoveCmdHandle == null && (unitUseAbility == null || (unitUseAbility.IsFinished && unitUseAbility.ExecutionProcess.IsEnded)))
		{
			unitUseAbilityParams = CreateAttackCommandParams(commandData.Ability, commandData.Target, commandData.CustomAnimation);
		}
		if (unitUseAbilityParams != null)
		{
			commandData.AttackCmdHandle = commandData.Unit.Commands.Run(unitUseAbilityParams);
		}
	}

	public override bool TryPrepareForStop(CutscenePlayerData player)
	{
		bool flag = !player.Cutscene.NonSkippable;
		if (!(player.GetCommandData<Data>(this).Interrupted && Continuous && flag))
		{
			return base.TryPrepareForStop(player);
		}
		return true;
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		bool flag = !player.Cutscene.NonSkippable;
		Data commandData = player.GetCommandData<Data>(this);
		commandData.IsTryingToSkip = !Continuous || (Continuous && flag);
		return commandData.IsTryingToSkip;
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Interrupted = true;
		if (!commandData.IsTryingToSkip)
		{
			return;
		}
		commandData.TakingTooLong = true;
		UnitCommandHandle moveCmdHandle = commandData.MoveCmdHandle;
		if (moveCmdHandle != null && !moveCmdHandle.IsFinished && moveCmdHandle.Cmd != null)
		{
			AbstractUnitEntity unit = commandData.Unit;
			if (unit != null)
			{
				unit.View.MovementAgent.Stop();
				unit.View.MovementAgent.Blocker.BlockAtCurrentPosition();
				Vector3 vector = commandData.MoveCmdHandle.Cmd.ForcedPath.vectorPath.Last();
				Vector3 vector2 = commandData.Target.Position - vector;
				Vector3 normalized = new Vector3(vector2.x, 0f, vector2.z).normalized;
				float? orientation = ((normalized.magnitude > 0.5f) ? new float?(Quaternion.FromToRotation(Vector3.forward, normalized).eulerAngles.y) : null);
				unit.Translocate(vector, orientation);
			}
		}
	}

	private UnitUseAbilityParams CreateAttackCommandParams(AbilityData ability, MechanicEntity target, IAbilityCustomAnimation customAnimationOverride = null)
	{
		bool killTarget = !Continuous && KillTarget;
		return new UnitUseAbilityParams(ability, target)
		{
			DisableLog = !EnableLog,
			HitPolicy = HitPolicy,
			DamagePolicy = DamagePolicy,
			NeedLoS = NeedLoS,
			KillTarget = killTarget,
			CustomAnimationOverride = customAnimationOverride,
			DisableCameraFollow = true
		};
	}

	private static (BlueprintItemWeapon Weapon, AttackType Type) FindSuitableSingleShot(AbstractUnitEntity unit)
	{
		AttackType item;
		return (Weapon: FindSuitableWeapon(unit, item = AttackType.SingleShot, activeSet: true, notActiveSets: false) ?? FindSuitableWeapon(unit, item = AttackType.Melee, activeSet: true, notActiveSets: false) ?? FindSuitableWeapon(unit, item = AttackType.SingleShot, activeSet: false, notActiveSets: true) ?? FindSuitableWeapon(unit, item = AttackType.Melee, activeSet: false, notActiveSets: true) ?? FindSuitableWeapon(unit, item = AttackType.BurstFire, activeSet: true, notActiveSets: true), Type: item);
	}

	[CanBeNull]
	private static BlueprintItemWeapon FindSuitableWeapon(AbstractUnitEntity unit, AttackType type, bool activeSet, bool notActiveSets)
	{
		PartUnitBody bodyOptional = unit.GetBodyOptional();
		if (bodyOptional == null)
		{
			return null;
		}
		HandsEquipmentSet currentHandsEquipmentSet = bodyOptional.CurrentHandsEquipmentSet;
		if (activeSet)
		{
			BlueprintItemWeapon blueprintItemWeapon = GetSuitableWeaponInternal(currentHandsEquipmentSet.PrimaryHand, type) ?? GetSuitableWeaponInternal(currentHandsEquipmentSet.SecondaryHand, type);
			if (blueprintItemWeapon != null)
			{
				return blueprintItemWeapon;
			}
		}
		if (notActiveSets)
		{
			foreach (HandsEquipmentSet handsEquipmentSet in bodyOptional.HandsEquipmentSets)
			{
				if (handsEquipmentSet != currentHandsEquipmentSet)
				{
					BlueprintItemWeapon blueprintItemWeapon2 = GetSuitableWeaponInternal(handsEquipmentSet.PrimaryHand, type) ?? GetSuitableWeaponInternal(handsEquipmentSet.SecondaryHand, type);
					if (blueprintItemWeapon2 != null)
					{
						return blueprintItemWeapon2;
					}
				}
			}
		}
		return null;
		static BlueprintItemWeapon GetSuitableWeaponInternal(HandSlot slot, AttackType t)
		{
			ItemEntityWeapon maybeWeapon = slot.MaybeWeapon;
			if (maybeWeapon == null)
			{
				return null;
			}
			if (t switch
			{
				AttackType.SingleShot => maybeWeapon.Blueprint.IsRanged ? 1 : 0, 
				AttackType.BurstFire => (maybeWeapon.Blueprint.IsRanged && maybeWeapon.GetWeaponStats().ResultRateOfFire > 1) ? 1 : 0, 
				AttackType.Melee => maybeWeapon.Blueprint.IsMelee ? 1 : 0, 
				_ => 0, 
			} == 0)
			{
				return null;
			}
			return maybeWeapon.Blueprint;
		}
	}

	[CanBeNull]
	private static BlueprintAbilityFXSettings FindSuitableFXSettings(ItemEntityWeapon weapon, AttackType type)
	{
		BlueprintAbilityFXSettings blueprintAbilityFXSettings = null;
		foreach (Ability ability in weapon.Abilities)
		{
			AttackAbilityType? attackType = ability.Blueprint.AttackType;
			if (!attackType.HasValue || ability.Data.FXSettings == null)
			{
				continue;
			}
			blueprintAbilityFXSettings = ability.Data.FXSettings;
			bool flag = type == AttackType.SingleShot;
			if (flag)
			{
				bool flag2;
				switch (attackType)
				{
				case AttackAbilityType.SingleShot:
				case AttackAbilityType.Pattern:
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = flag2;
			}
			if (flag || (type == AttackType.BurstFire && attackType.HasValue && attackType.GetValueOrDefault() == AttackAbilityType.Scatter) || (type == AttackType.Melee && attackType.HasValue && attackType.GetValueOrDefault() == AttackAbilityType.Melee) || type == AttackType.Custom)
			{
				return blueprintAbilityFXSettings;
			}
		}
		if (blueprintAbilityFXSettings == null)
		{
			PFLog.Default.ErrorWithReport("Can't find FXSettings for cutscene attack ability");
		}
		return blueprintAbilityFXSettings;
	}

	protected override void OnRunException()
	{
		if (MuteAttacker)
		{
			AkSoundEngine.SetRTPCValue("MuteEntity", 1f, Unit.GetValue().View.gameObject);
		}
		base.OnRunException();
	}
}

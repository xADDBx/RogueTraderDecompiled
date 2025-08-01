using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities;

public class AbilityData : IUIDataProvider, IAbilityDataProviderForPattern, IHashable
{
	public enum VoiceIntensityType
	{
		None,
		Low,
		Medium,
		High
	}

	public enum UnavailabilityReasonType
	{
		None = -1,
		Unknown,
		AbilityDisabled,
		CasterRestrictionNotPassed,
		AbilityRestrictionNotPassed,
		MaterialComponentRequired,
		UnitFactRequired,
		NotEnoughAmmo,
		AlreadyFullAmmo,
		IsOnCooldown,
		IsOnCooldownUntilEndOfCombat,
		CannotUseInThreatenedArea,
		CannotUseInConcussionArea,
		CannotUseInCantAttackArea,
		CannotUseInInertWarpArea,
		HasNoLosToTarget,
		TargetTooFar,
		TargetTooClose,
		AreaEffectsCannotOverlap,
		IsUltimateAbilityUsedThisRound,
		CannotTargetSelf,
		CannotTargetAlly,
		CannotTargetEnemy,
		CannotTargetDead,
		CannotTargetAlive,
		CannotTargetNotEmptyCell,
		AbilityForbidden,
		UntargetableForAbilityGroup,
		TargetRestrictionNotPassed,
		CannotMove,
		FriendlyFire,
		NullTarget,
		RestrictedByInterruption,
		TargetEntityDisposed,
		OnlyUseInCombat,
		StrategistZonesCantOverlap
	}

	public class IgnoreCooldown : ContextFlag<IgnoreCooldown>
	{
	}

	public class ForceFreeAction : ContextFlag<ForceFreeAction>
	{
	}

	[JsonProperty]
	private EntityRef<MechanicEntity> m_CasterRef;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	private AbilityData m_ConvertedFrom;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private bool m_IsCharge;

	[JsonProperty]
	public readonly BlueprintAbility Blueprint;

	private string m_CachedName;

	private bool m_SourceTypeSet;

	private SpellSource m_SpellSource;

	private int? m_DefaultSpellLevel;

	[CanBeNull]
	private IAbilityVisibilityProvider[] m_CachedVisibilityProviders;

	private readonly bool m_IsPreview;

	private AbilityData m_CachedInitialTargetAbility;

	private List<RuleCalculateScatterShotHitDirectionProbability> m_ScatterShotHitDirectionProbabilities;

	private Dictionary<(string, byte), float> m_DodgeChanceCache;

	private Dictionary<(CustomGridNodeBase, CustomGridNodeBase), bool> m_LosCache;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	public Ability Fact { get; private set; }

	[JsonProperty]
	public string UniqueId { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	public BlueprintScriptableObject OverrideRequiredResource { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool ForceTargetOwner { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int DecorationColorNumber { get; set; } = -1;


	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int DecorationBorderNumber { get; set; } = -1;


	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public ItemEntityStarshipWeapon FakeStarshipWeapon { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public ItemEntityWeapon OverrideWeapon { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int OverrideRateOfFire { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int ItemSlotIndex { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public EntityFact ItemSlotSource { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettingsOverride { get; set; }

	public List<BlueprintAbilityGroup> AbilityGroups { get; private set; }

	public int? OverrideDC { get; set; }

	public int? OverrideSpellLevel { get; set; }

	public int? OverrideCasterLevel { get; set; }

	public int? OverrideCasterModifier { get; set; }

	public bool PotionForOther { get; set; }

	public bool IsAttackOfOpportunity { get; set; }

	public bool IsRedirected { get; set; }

	public bool IgnoreUsingInThreateningArea { get; set; }

	[CanBeNull]
	public WeaponAbility SettingsFromItem => ItemSlotList?[ItemSlotIndex];

	[CanBeNull]
	private IReadOnlyList<WeaponAbility> ItemSlotList
	{
		get
		{
			if (ItemSlotSource == null)
			{
				return (Weapon == null) ? StarshipWeapon?.Blueprint?.WeaponAbilities : Weapon?.Blueprint?.WeaponAbilities;
			}
			return ItemSlotSource.GetComponent<AddAbilitiesToCurrentWeapon>()?.WeaponAbilities;
		}
	}

	public ItemEntityWeapon SourceWeapon => SourceItem as ItemEntityWeapon;

	[CanBeNull]
	public ItemEntityWeapon Weapon => OverrideWeapon ?? SourceWeapon;

	public bool SourceItemIsWeapon
	{
		get
		{
			ItemEntity sourceItem = SourceItem;
			return sourceItem is ItemEntityWeapon || sourceItem is ItemEntityShield;
		}
	}

	public int BurstAttacksCount
	{
		get
		{
			if (!IsBurstAttack)
			{
				return 1;
			}
			if (OverrideRateOfFire <= 0)
			{
				return Rulebook.Trigger(new RuleCalculateBurstCount(Caster, this)).Result;
			}
			return OverrideRateOfFire;
		}
	}

	public int ActionsCount
	{
		get
		{
			if (!IsStarshipAttack)
			{
				return BurstAttacksCount;
			}
			return StarshipWeapon.Blueprint.DamageInstances;
		}
	}

	public bool IsBurstAttack => Blueprint.IsBurst;

	public bool IsStarshipAttack => StarshipWeapon != null;

	public int RangeCells => Rulebook.Trigger(new RuleCalculateAbilityRange(Caster, this)).Result;

	public int MinRangeCells
	{
		get
		{
			if (Blueprint.Range == AbilityRange.Personal)
			{
				return 0;
			}
			return Blueprint.MinRange;
		}
	}

	public bool IsWeaponAttackThatRequiresAmmo
	{
		get
		{
			if (SourceWeapon != null)
			{
				ItemEntityWeapon sourceWeapon = SourceWeapon;
				if (sourceWeapon == null || sourceWeapon.Blueprint.WarhammerMaxAmmo != -1)
				{
					ItemEntityWeapon sourceWeapon2 = SourceWeapon;
					if (sourceWeapon2 != null && sourceWeapon2.Blueprint.IsRanged)
					{
						AbilityAmmoLogic component = Blueprint.GetComponent<AbilityAmmoLogic>();
						if (component == null)
						{
							return true;
						}
						return !component.NoAmmoRequired;
					}
				}
			}
			return false;
		}
	}

	public int RateOfFire => this.GetWeaponStats().ResultRateOfFire;

	public int AmmoRequired
	{
		get
		{
			if (!IsWeaponAttackThatRequiresAmmo)
			{
				return 0;
			}
			int val = ((Weapon != null && Weapon.Blueprint.WarhammerMaxAmmo > 1) ? Math.Max(Weapon.CurrentAmmo, 2) : int.MaxValue);
			return ((!IsBurstAttack) ? 1 : Math.Min(val, RateOfFire)) + (Blueprint.GetComponent<AbilityAmmoLogic>()?.AdditionalAmmoCost ?? 0);
		}
	}

	public bool ClearMPAfterUse => Blueprint.GetComponent<WarhammerEndTurn>()?.clearMPInsteadOfEndingTurn ?? false;

	[CanBeNull]
	public AbilityData ConvertedFrom
	{
		get
		{
			return m_ConvertedFrom;
		}
		set
		{
			m_ConvertedFrom = value;
			Fact = ConvertedFrom?.Fact;
			if (ConvertedFrom != null && !ConvertedFrom.CanBeConvertedTo(Blueprint))
			{
				PFLog.Default.Error("Invalid spell conversion: {0} -> {1}", ConvertedFrom, this);
			}
		}
	}

	public SpellSource SpellSource
	{
		get
		{
			if (!m_SourceTypeSet)
			{
				m_SpellSource = CalcSpellSource();
				m_SourceTypeSet = true;
			}
			return m_SpellSource;
		}
		set
		{
			m_SpellSource = value;
			m_SourceTypeSet = true;
		}
	}

	public bool IsAOE
	{
		get
		{
			if (Blueprint.GetComponent<AbilityTargetsInPattern>() != null)
			{
				return true;
			}
			AbilityEffectRunAction component = Blueprint.GetComponent<AbilityEffectRunAction>();
			if (component != null && component.Actions.Actions.TryFind((GameAction x) => x is ContextActionSpawnAreaEffect, out var _))
			{
				return true;
			}
			AbilityMeleeBurst component2 = Blueprint.GetComponent<AbilityMeleeBurst>();
			if (component2 != null && component2.IsAoe)
			{
				return true;
			}
			WarhammerAbilityAttackDelivery component3 = Blueprint.GetComponent<WarhammerAbilityAttackDelivery>();
			if (component3 == null || !component3.IsPattern)
			{
				return Blueprint.GetComponents<FakeAttackType>().Any((FakeAttackType p) => p.CountAsAoE);
			}
			return true;
		}
	}

	public bool IsScatter
	{
		get
		{
			if (Blueprint.AttackType != AttackAbilityType.Scatter && !Blueprint.GetComponents<FakeAttackType>().Any((FakeAttackType p) => p.CountAsScatter))
			{
				return Blueprint.GetComponents<AbilityMeleeBurst>().Any();
			}
			return true;
		}
	}

	public bool IsSingleShot
	{
		get
		{
			if (Blueprint.AttackType != AttackAbilityType.SingleShot)
			{
				return Blueprint.GetComponents<FakeAttackType>().Any((FakeAttackType p) => p.CountAsSingleShot);
			}
			return true;
		}
	}

	public bool IsMelee
	{
		get
		{
			if (Blueprint.AttackType != AttackAbilityType.Melee && !Blueprint.GetComponents<FakeAttackType>().Any((FakeAttackType p) => p.CountAsMelee))
			{
				return Blueprint.GetComponents<AbilityMeleeBurst>().Any();
			}
			return true;
		}
	}

	public bool IsCharge
	{
		get
		{
			if (!Blueprint.IsCharge)
			{
				return m_IsCharge;
			}
			return true;
		}
		set
		{
			m_IsCharge = value;
		}
	}

	public bool IsHeroicAct => Blueprint.IsHeroicAct;

	public bool IsDesperateMeasure => Blueprint.IsDesperateMeasure;

	public bool IsUltimate
	{
		get
		{
			if (!IsHeroicAct)
			{
				return IsDesperateMeasure;
			}
			return true;
		}
	}

	public int CounterAttackRange
	{
		get
		{
			AbilityEffectRunAction component = Blueprint.GetComponent<AbilityEffectRunAction>();
			if (component != null && component.Actions.Actions.TryFind((GameAction x) => x is ContextActionApplyBuff, out var result))
			{
				CounterAttack counterAttack = (result as ContextActionApplyBuff)?.Buff?.GetComponent<CounterAttack>();
				if (counterAttack != null)
				{
					return counterAttack.GuardAlliesRange.Value;
				}
			}
			return -1;
		}
	}

	public MechanicEntity Caster
	{
		get
		{
			object obj = ConvertedFrom?.Caster;
			if (obj == null)
			{
				obj = m_CasterRef.Entity;
				if (obj == null)
				{
					Ability fact = Fact;
					if (fact == null)
					{
						return null;
					}
					obj = fact.ConcreteOwner;
				}
			}
			return (MechanicEntity)obj;
		}
	}

	[CanBeNull]
	public ItemEntity SourceItem => (ItemEntity)(Fact?.SourceItem);

	[CanBeNull]
	public Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot StarshipWeaponSlot
	{
		get
		{
			ItemSlot itemSlot = SourceItem?.HoldingSlot;
			if (itemSlot is Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot result)
			{
				return result;
			}
			if (itemSlot is AmmoSlot ammoSlot)
			{
				return ammoSlot.WeaponSlot;
			}
			return null;
		}
	}

	[CanBeNull]
	public ItemEntityStarshipWeapon StarshipWeapon
	{
		get
		{
			if (FakeStarshipWeapon != null)
			{
				return FakeStarshipWeapon;
			}
			return StarshipWeaponSlot?.Weapon;
		}
	}

	public RestrictedFiringArc RestrictedFiringArc => StarshipWeaponSlot?.FiringArc ?? RestrictedFiringArc.None;

	[CanBeNull]
	public BlueprintItemEquipment SourceItemEquipmentBlueprint => SourceItem?.Blueprint as BlueprintItemEquipment;

	[CanBeNull]
	public BlueprintItemEquipmentUsable SourceItemUsableBlueprint => SourceItem?.Blueprint as BlueprintItemEquipmentUsable;

	public bool IsVariable => Blueprint.HasVariants;

	public int SpellLevel => 0;

	[CanBeNull]
	public IAbilityResourceLogic ResourceLogic => Blueprint.GetComponent<IAbilityResourceLogic>();

	public AbilityParameter ParameterRequirements => Blueprint.GetComponent<IAbilityRequiredParameters>()?.RequiredParameters ?? AbilityParameter.None;

	public bool RequireParamUnitFact => ParameterRequirements.HasFlag(AbilityParameter.UnitFact);

	public bool RequireMaterialComponent
	{
		get
		{
			if ((bool)Blueprint.MaterialComponent.Item)
			{
				return SourceItem == null;
			}
			return false;
		}
	}

	public bool HasEnoughMaterialComponent
	{
		get
		{
			if (Blueprint.MaterialComponent.Item != null)
			{
				return Caster.GetInventoryOptional()?.Contains(Blueprint.MaterialComponent.Item, Blueprint.MaterialComponent.Count) ?? true;
			}
			return false;
		}
	}

	public bool NeedLoS
	{
		get
		{
			if (Blueprint.GetComponent<AbilityTargetIsDeadCompanion>() == null)
			{
				return Blueprint.GetComponent<AbilityIgnoreLineOfSight>() == null;
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			if (ConvertedFrom == null)
			{
				return Blueprint.Name;
			}
			m_CachedName = (string.IsNullOrEmpty(m_CachedName) ? (ConvertedFrom.Name + "-" + Blueprint.Name) : m_CachedName);
			return m_CachedName;
		}
	}

	public string Description => Blueprint.Description;

	public Sprite Icon => Blueprint.Icon;

	public string NameForAcronym => Blueprint.NameForAcronym;

	public string ShortenedDescription => Blueprint.GetShortenedDescription();

	public AbilityTargetAnchor TargetAnchor
	{
		get
		{
			if (ForceTargetOwner)
			{
				return AbilityTargetAnchor.Owner;
			}
			if (SourceItemUsableBlueprint != null && SourceItemUsableBlueprint.Type == UsableItemType.Potion && !PotionForOther)
			{
				return AbilityTargetAnchor.Owner;
			}
			if (Blueprint.Range == AbilityRange.Personal)
			{
				return AbilityTargetAnchor.Owner;
			}
			if (!Blueprint.CanTargetFriends && !Blueprint.CanTargetEnemies && !Blueprint.CanTargetPoint)
			{
				return AbilityTargetAnchor.Owner;
			}
			if (!Blueprint.CanTargetPointAfterRestrictions(this))
			{
				return AbilityTargetAnchor.Unit;
			}
			return AbilityTargetAnchor.Point;
		}
	}

	public bool HasRequiredParams => !RequireParamUnitFact;

	public bool IsAvailable
	{
		get
		{
			if (GetAvailableForCastCount() != 0 && HasEnoughActionPoint && HasEnoughAmmo && !IsRestricted)
			{
				if (IsOnCooldown)
				{
					return IsBonusUsage;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasEnoughActionPoint
	{
		get
		{
			PartUnitCombatState combatStateOptional = Caster.GetCombatStateOptional();
			if (combatStateOptional != null && combatStateOptional.IsInCombat)
			{
				return combatStateOptional.ActionPointsYellow >= CalculateActionPointCost();
			}
			return true;
		}
	}

	public bool IsOnCooldown
	{
		get
		{
			if (!ContextData<IgnoreCooldown>.Current)
			{
				return Caster.GetAbilityCooldownsOptional()?.IsOnCooldown(this) ?? false;
			}
			return false;
		}
	}

	public bool IsOnCooldownUntilEndOfCombat
	{
		get
		{
			if (!ContextData<IgnoreCooldown>.Current)
			{
				return Caster.GetAbilityCooldownsOptional()?.IsOnCooldownUntilEndOfCombat(this) ?? false;
			}
			return false;
		}
	}

	public int Cooldown => (Caster.GetAbilityCooldownsOptional()?.GetAutonomousCooldown(Blueprint)).GetValueOrDefault();

	public bool HasEnoughAmmo
	{
		get
		{
			if (IsWeaponAttackThatRequiresAmmo)
			{
				return !(SourceWeapon?.CurrentAmmo < AmmoRequired);
			}
			return true;
		}
	}

	public bool IsBonusUsage => Caster.GetBonusAbilityUseOptional()?.HasBonusAbilityUsage(this) ?? false;

	public bool SummonedLimitOnlyRestrictionValue
	{
		get
		{
			if (Blueprint.CasterRestrictions.Length != 1)
			{
				return false;
			}
			return Blueprint.CasterRestrictions.FirstOrDefault((IAbilityCasterRestriction r) => r is StarshipSummonedUnitsLimit) != null;
		}
	}

	public bool IsRestricted
	{
		get
		{
			if ((bool)Game.Instance.LoadedAreaState?.Settings.Peaceful)
			{
				return true;
			}
			if ((Blueprint.CombatStateRestriction == BlueprintAbility.CombatStateRestrictionType.InCombatOnly && !Caster.IsInCombat) || (Blueprint.CombatStateRestriction == BlueprintAbility.CombatStateRestrictionType.NotInCombatOnly && Caster.IsInCombat))
			{
				return true;
			}
			if (Fact != null && ((!(Caster.GetAbilitySettingsOptional()?.InterruptionAbilityRestrictions?.IsPassed(new PropertyContext(Fact, null, null, this)))) ?? false))
			{
				return true;
			}
			IAbilityCasterRestriction[] casterRestrictions = Blueprint.CasterRestrictions;
			for (int i = 0; i < casterRestrictions.Length; i++)
			{
				if (!casterRestrictions[i].IsCasterRestrictionPassed(Caster))
				{
					return true;
				}
			}
			if (Weapon != null)
			{
				WeaponReloadLogic reloadLogic = Blueprint.GetComponent<WeaponReloadLogic>();
				if (reloadLogic != null && Weapon.Abilities.Where((Ability a) => a.Data != this).All((Ability a) => !reloadLogic.IsAvailable(a.Data)))
				{
					return true;
				}
			}
			PartUnitCombatState combatStateOptional = Caster.GetCombatStateOptional();
			if (combatStateOptional != null && combatStateOptional.IsEngagedInRealOrVirtualPosition && !IgnoreUsingInThreateningArea && UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.CannotUse && !Caster.GetMechanicFeature(MechanicsFeatureType.CanShootInMelee).Value)
			{
				EventBus.RaiseEvent(delegate(IAbilityCannotUseInThreateningArea h)
				{
					h.HandleCannotUseAbilityInThreateningArea(this);
				});
				return true;
			}
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)(GraphNode)Caster.CurrentNode;
			if (!Blueprint.IsWeaponAbility && customGridNodeBase != null && AreaEffectsController.CheckConcussionEffect(customGridNodeBase))
			{
				return true;
			}
			if (Blueprint.IsWeaponAbility && customGridNodeBase != null && AreaEffectsController.CheckCantAttackEffect(customGridNodeBase))
			{
				return true;
			}
			if (Blueprint.IsPsykerAbility && customGridNodeBase != null && AreaEffectsController.CheckInertWarpEffect(customGridNodeBase))
			{
				return true;
			}
			if (Caster.IsPlayerFaction && RequireMaterialComponent && !HasEnoughMaterialComponent)
			{
				return true;
			}
			if (HasRequiredParams)
			{
				Ability fact = Fact;
				if (fact == null || fact.Active)
				{
					IAbilityRestriction[] restrictions = Blueprint.Restrictions;
					for (int i = 0; i < restrictions.Length; i++)
					{
						if (!restrictions[i].IsAbilityRestrictionPassed(this))
						{
							return true;
						}
					}
					if (StarshipWeapon != null && (bool)StarshipWeapon.IsBlocked)
					{
						return true;
					}
					UnitPartForbiddenAbilities optional = Caster.GetOptional<UnitPartForbiddenAbilities>();
					if (optional != null && !optional.AbilityAllowed(this))
					{
						return true;
					}
					WarhammerUnitPartDisableAttack optional2 = Caster.GetOptional<WarhammerUnitPartDisableAttack>();
					WarhammerUnitPartChooseWeapon optional3 = Caster.GetOptional<WarhammerUnitPartChooseWeapon>();
					if (optional2 != null && optional2.AttackDisabled && IsWeaponAttackThatRequiresAmmo && Weapon == optional3?.ChosenWeapon)
					{
						return true;
					}
					if (IsDesperateMeasure && Game.Instance.TurnController.IsUltimateAbilityUsedThisRound)
					{
						return true;
					}
					PartAbilitySettings abilitySettingsOptional = Caster.GetAbilitySettingsOptional();
					if (Fact != null && abilitySettingsOptional != null && abilitySettingsOptional.InterruptionAbilityRestrictions != null && !abilitySettingsOptional.InterruptionAbilityRestrictions.IsPassed(new PropertyContext(Fact, null, null, this)))
					{
						return false;
					}
					if (Caster.Facts.GetComponents((WarhammerAbilityRestriction restriction) => restriction.AbilityIsRestricted(this)).Any())
					{
						return true;
					}
					return false;
				}
			}
			return true;
		}
	}

	public bool IsFreeAction
	{
		get
		{
			if (!ContextData<ForceFreeAction>.Current)
			{
				return Blueprint.IsFreeAction;
			}
			return true;
		}
	}

	public VoiceIntensityType VoiceIntensity => VoiceIntensityType.None;

	[CanBeNull]
	public BlueprintScriptableObject RequiredResource => SimpleBlueprintExtendAsObject.Or(OverrideRequiredResource, null);

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => FXSettingsOverride ?? SettingsFromItem?.FXSettings ?? Blueprint.FXSettings;

	public IEnumerable<BlueprintProjectile> ProjectileVariants
	{
		get
		{
			if (FXSettings == null || FXSettings.VisualFXSettings == null || FXSettings.VisualFXSettings.Projectiles.Empty())
			{
				yield break;
			}
			foreach (BlueprintProjectile projectile in FXSettings.VisualFXSettings.Projectiles)
			{
				yield return projectile;
			}
		}
	}

	public bool UseBestShootingPosition
	{
		get
		{
			if (Blueprint.UseBestShootingPosition)
			{
				return Caster.Size.Is1x1();
			}
			return false;
		}
	}

	public bool IsInstantDeliver => Blueprint.GetComponent<AbilityEffectOverwatch>() != null;

	[NotNull]
	[ItemNotNull]
	public IEnumerable<BlueprintAbilityAdditionalEffect> AdditionalEffects
	{
		get
		{
			WeaponAbility s = SettingsFromItem;
			if ((s == null || s.OnHitOverrideType != OnHitOverrideType.Override) && Weapon?.Blueprint.OnHitActions != null)
			{
				yield return Weapon.Blueprint.OnHitActions;
			}
			if ((s == null || s.OnHitOverrideType != 0) && s?.OnHitActions != null)
			{
				yield return s.OnHitActions;
			}
		}
	}

	public BlueprintAbility.UsingInThreateningAreaType UsingInThreateningArea => PartAbilitySettings.GetThreatenedAreaSetting(Caster, Blueprint);

	public AbilityData InitialTargetAbility
	{
		get
		{
			AbilityData abilityData = m_CachedInitialTargetAbility;
			if ((object)abilityData == null)
			{
				AbilityData obj = GetAbilityForTargetIndex(0) ?? this;
				AbilityData abilityData2 = obj;
				m_CachedInitialTargetAbility = obj;
				abilityData = abilityData2;
			}
			return abilityData;
		}
	}

	public AbilityData Data => this;

	public ReadonlyList<RuleCalculateScatterShotHitDirectionProbability> ScatterShotHitDirectionProbabilities
	{
		get
		{
			if (m_IsPreview && m_ScatterShotHitDirectionProbabilities != null)
			{
				return m_ScatterShotHitDirectionProbabilities;
			}
			List<RuleCalculateScatterShotHitDirectionProbability> list = new List<RuleCalculateScatterShotHitDirectionProbability>();
			for (int i = 0; i < BurstAttacksCount; i++)
			{
				list.Add(Rulebook.Trigger(new RuleCalculateScatterShotHitDirectionProbability(Caster, this, i)));
			}
			if (m_IsPreview)
			{
				m_ScatterShotHitDirectionProbabilities = list;
			}
			return list;
		}
	}

	[CanBeNull]
	public AbilityRedirect RedirectSettings => Blueprint.GetComponent<AbilityRedirect>();

	public AoEPattern RedirectPattern => RedirectSettings?.CustomPattern ?? Root.WH.AbilityRedirect.Pattern;

	public bool CanRedirect => RedirectSettings?.CasterRestrictions.IsPassed(new PropertyContext(this, Caster)) ?? false;

	public bool TryGetCasterDesiredPositionAndDirection(out Vector3 position, out Vector3 direction)
	{
		if (Blueprint.TryGetComponent<IAbilityOverrideCasterDesiredPosition>(out var component))
		{
			return component.TryGetDesiredPositionAndDirection(this, out position, out direction);
		}
		position = (direction = default(Vector3));
		return false;
	}

	private AbilityData GetAbilityForTargetIndex(int targetIndex)
	{
		if (!Blueprint.TryGetComponent<IAbilityMultiTarget>(out var component) || !component.TryGetNextTargetAbilityAndCaster(this, targetIndex, out var ability, out var caster))
		{
			return null;
		}
		return new AbilityData(ability, caster);
	}

	private AbilityData([NotNull] BlueprintAbility blueprint, [NotNull] MechanicEntity caster, [CanBeNull] Ability fact, [CanBeNull] string guid)
	{
		Blueprint = blueprint ?? throw new ArgumentNullException("blueprint");
		m_CasterRef = caster ?? throw new ArgumentNullException("caster");
		Fact = fact;
		UniqueId = guid ?? Uuid.Instance.CreateString();
		m_IsPreview = ContextData<UnitHelper.PreviewUnit>.Current;
		BlueprintItemWeapon blueprintItemWeapon = Blueprint.GetComponent<WarhammerOverrideAbilityWeapon>()?.Weapon;
		if (blueprintItemWeapon != null)
		{
			OverrideWeapon = (ItemEntityWeapon)blueprintItemWeapon.CreateEntity();
		}
		AbilityCustomBladeDance component = Blueprint.GetComponent<AbilityCustomBladeDance>();
		if (component != null)
		{
			OverrideRateOfFire = component.RateOfAttack.Calculate(CreateExecutionContext(caster));
			if (Blueprint.FXSettings != null)
			{
				FXSettingsOverride = Blueprint.FXSettings;
			}
		}
		AbilityMeleeBurst component2 = Blueprint.GetComponent<AbilityMeleeBurst>();
		if (component2 != null)
		{
			OverrideRateOfFire = component2.GetRateOfFire(CreateExecutionContext(caster));
		}
		WarhammerOverrideRateOfFire component3 = Blueprint.GetComponent<WarhammerOverrideRateOfFire>();
		if (component3 != null)
		{
			OverrideRateOfFire = component3.RateOfFire;
		}
		if (caster is StarshipEntity starshipEntity)
		{
			WarhammerOverrideAbilityStarshipWeapon component4 = Blueprint.GetComponent<WarhammerOverrideAbilityStarshipWeapon>();
			if (component4 != null)
			{
				FakeStarshipWeapon = (ItemEntityStarshipWeapon)(component4.StarshipWeapon(starshipEntity)?.CreateEntity());
				FakeStarshipWeapon.FakeAmmo = (ItemEntityStarshipAmmo)(component4.StarshipWeaponAmmo(starshipEntity)?.CreateEntity());
			}
		}
		InitAbilityGroups();
	}

	public AbilityData([NotNull] BlueprintAbility blueprint, [NotNull] MechanicEntity caster)
		: this(blueprint, caster, null, null)
	{
	}

	public AbilityData([NotNull] Ability fact, MechanicEntity caster = null)
		: this(fact.Blueprint, caster ?? fact.ConcreteOwner ?? throw new ArgumentException("Caster is missing!"), fact, fact.UniqueId)
	{
	}

	public AbilityData([NotNull] AbilityData other, [NotNull] BlueprintAbility replaceBlueprint)
		: this(replaceBlueprint, other.Caster, other.Fact, other.UniqueId + "_" + replaceBlueprint.name)
	{
		m_ConvertedFrom = other;
	}

	[JsonConstructor]
	private AbilityData()
	{
	}

	public AbilityData Clone(bool isPreview)
	{
		using (ContextData<DisableStatefulRandomContext>.RequestIf(isPreview))
		{
			using (ContextData<UnitHelper.PreviewUnit>.RequestIf(isPreview))
			{
				return new AbilityData(Blueprint, Caster)
				{
					OverrideWeapon = Weapon,
					FakeStarshipWeapon = (FakeStarshipWeapon ?? StarshipWeapon)
				};
			}
		}
	}

	public void PrePostLoad(Ability ability)
	{
		m_CasterRef = ability.ConcreteOwner;
		Fact = ability;
		InitAbilityGroups();
	}

	public void PrePostLoad(MechanicEntity caster)
	{
		m_CasterRef = caster;
		InitAbilityGroups();
	}

	private void InitAbilityGroups()
	{
		AbilityGroups = new List<BlueprintAbilityGroup>();
		foreach (BlueprintAbilityGroup abilityGroup in Blueprint.AbilityGroups)
		{
			if (abilityGroup != null)
			{
				AbilityGroups.AddRange(abilityGroup.GetAllAbilityGroups());
			}
		}
	}

	public int GetVeilThicknessPointsToAdd(bool isPrediction = false)
	{
		if (!Blueprint.IsPsykerAbility)
		{
			return 0;
		}
		int veilThicknessPointsToAdd = Blueprint.GetVeilThicknessPointsToAdd();
		int veilChangeFromComponents = 0;
		foreach (EntityFact item in Caster.Facts.List)
		{
			item.CallComponents(delegate(ChangeVeilDamage i)
			{
				veilChangeFromComponents += i.GetChange(this, isPrediction);
			});
		}
		veilThicknessPointsToAdd += veilChangeFromComponents;
		return Math.Max(veilThicknessPointsToAdd, -1);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((AbilityData)obj);
	}

	public override int GetHashCode()
	{
		return (((((((((((((((((Blueprint != null) ? Blueprint.GetHashCode() : 0) * 397) ^ m_CasterRef.GetHashCode()) * 397) ^ ((m_ConvertedFrom != null) ? m_ConvertedFrom.GetHashCode() : 0)) * 397) ^ ((Fact != null) ? Fact.GetHashCode() : 0)) * 397) ^ OverrideDC.GetHashCode()) * 397) ^ OverrideSpellLevel.GetHashCode()) * 397) ^ OverrideCasterLevel.GetHashCode()) * 397) ^ OverrideCasterModifier.GetHashCode()) * 397) ^ PotionForOther.GetHashCode();
	}

	protected bool Equals(AbilityData other)
	{
		if ((object)other != null && object.Equals(Blueprint, other.Blueprint) && m_CasterRef == other.m_CasterRef && object.Equals(m_ConvertedFrom, other.m_ConvertedFrom) && object.Equals(Fact, other.Fact) && OverrideDC == other.OverrideDC && OverrideSpellLevel == other.OverrideSpellLevel && OverrideCasterLevel == other.OverrideCasterLevel && OverrideCasterModifier == other.OverrideCasterModifier)
		{
			return PotionForOther == other.PotionForOther;
		}
		return false;
	}

	public static bool operator ==(AbilityData a1, AbilityData a2)
	{
		return a1?.Equals(a2) ?? ((object)a2 == null);
	}

	public static bool operator !=(AbilityData a1, AbilityData a2)
	{
		return !(a1 == a2);
	}

	private SpellSource CalcSpellSource()
	{
		if (SourceItemUsableBlueprint != null && SourceItemUsableBlueprint.Type != UsableItemType.Scroll)
		{
			return SpellSource.None;
		}
		if (Blueprint.Type != 0)
		{
			return SpellSource.None;
		}
		Feature obj = Caster.Facts.List.FirstItem((EntityFact f) => f.Blueprint.GetComponents<AddFacts>().Any((AddFacts af) => af.Facts.Contains(Blueprint))) as Feature;
		BlueprintCharacterClass blueprintCharacterClass = ((obj == null) ? null : SimpleBlueprintExtendAsObject.Or(obj.SourceProgression, null)?.FirstClass);
		if (blueprintCharacterClass != null)
		{
			if (!blueprintCharacterClass.IsDivineCaster)
			{
				return SpellSource.Arcane;
			}
			return SpellSource.Divine;
		}
		return SpellSource.Unknown;
	}

	public AbilityExecutionContext CreateExecutionContext([NotNull] TargetWrapper target)
	{
		return CreateExecutionContext(target, Caster.Position);
	}

	public AbilityExecutionContext CreateExecutionContext([NotNull] TargetWrapper target, Vector3 casterPosition)
	{
		return new AbilityExecutionContext(this, target ?? throw new ArgumentNullException("target"), casterPosition);
	}

	public AbilityExecutionProcess Cast(AbilityExecutionContext context)
	{
		if (!context.IsForced && !IsAvailable)
		{
			PFLog.Default.ErrorWithReport("Can't cast spell: !context.IsForced && !IsAvailable");
			return null;
		}
		if (Blueprint.HasVariants)
		{
			PFLog.Default.ErrorWithReport("Can't cast spell with variants");
			return null;
		}
		return Game.Instance.AbilityExecutor.Execute(context);
	}

	public void SpendMaterialComponent()
	{
		if (RequireMaterialComponent)
		{
			Caster.GetInventoryOptional()?.Remove(Blueprint.MaterialComponent.Item, Blueprint.MaterialComponent.Count);
		}
	}

	public void Spend()
	{
		SpendMaterialComponent();
		SourceItem?.SpendCharges(Caster);
		if (ResourceLogic != null)
		{
			ResourceLogic.Spend(this);
		}
		else if (RequiredResource != null)
		{
			Caster.GetAbilityResourcesOptional()?.Spend(RequiredResource, 1);
		}
	}

	public bool IsValid(TargetWrapper target)
	{
		return IsValid(target, Caster.Position);
	}

	public bool IsValid(TargetWrapper target, out UnavailabilityReasonType unavailabilityReason)
	{
		return IsValid(target, Caster.Position, out unavailabilityReason);
	}

	public bool IsValid(TargetWrapper target, Vector3 casterPosition)
	{
		UnavailabilityReasonType unavailabilityReason;
		return IsValid(target, casterPosition, out unavailabilityReason);
	}

	public bool IsValid(TargetWrapper target, Vector3 casterPosition, out UnavailabilityReasonType unavailabilityReason)
	{
		AbilityTargetingCache instance = AbilityTargetingCache.Instance;
		if (!instance.IsActive)
		{
			return CalculateIsValid(target, casterPosition, out unavailabilityReason);
		}
		if (instance.TryGetReason(Blueprint, target, casterPosition, out unavailabilityReason))
		{
			return unavailabilityReason == UnavailabilityReasonType.None;
		}
		bool flag = CalculateIsValid(target, casterPosition, out unavailabilityReason);
		if (!flag && unavailabilityReason == UnavailabilityReasonType.None)
		{
			AILogger.Instance.Error(new AILogMessage($"invalid ability targeting didn't get any reasoning\n ability {Blueprint} by {Caster} from {casterPosition} to {target}"));
		}
		instance.AddEntry(Blueprint, target, casterPosition, unavailabilityReason);
		return flag;
	}

	public bool CalculateIsValid(TargetWrapper target, Vector3 casterPosition, out UnavailabilityReasonType unavailabilityReason)
	{
		if (Fact != null && ((!(Caster.GetAbilitySettingsOptional()?.InterruptionAbilityRestrictions?.IsPassed(new PropertyContext(Fact, null, null, this)))) ?? false))
		{
			unavailabilityReason = UnavailabilityReasonType.RestrictedByInterruption;
			return false;
		}
		IAbilityTargetRestriction[] targetRestrictions = Blueprint.TargetRestrictions;
		for (int i = 0; i < targetRestrictions.Length; i++)
		{
			if (!targetRestrictions[i].IsTargetRestrictionPassed(this, target, casterPosition))
			{
				unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
				return false;
			}
		}
		BlueprintComponent[] componentsArray = Blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (blueprintComponent is AbilityEffectRunAction abilityEffectRunAction && !abilityEffectRunAction.IsValidToCast(target, Caster, casterPosition))
			{
				unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
				return false;
			}
			if (blueprintComponent is CustomAbilityQueue customAbilityQueue && !customAbilityQueue.IsValidToCast(target, Caster, casterPosition, out var reason))
			{
				unavailabilityReason = reason;
				return false;
			}
		}
		if (!IsCasterValid(casterPosition, out var unavailabilityReason2))
		{
			unavailabilityReason = unavailabilityReason2;
			return false;
		}
		if (target.HasEntity && target.Entity == null)
		{
			unavailabilityReason = UnavailabilityReasonType.TargetEntityDisposed;
			return false;
		}
		if (target.Entity != null && TargetAnchor != AbilityTargetAnchor.Point)
		{
			if ((bool)target.Entity.Features.IsUntargetable && target.Entity != Caster)
			{
				unavailabilityReason = UnavailabilityReasonType.NullTarget;
				return false;
			}
			if (!Blueprint.CanTargetSelf && target.Entity == Caster)
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetSelf;
				return false;
			}
			if (!Blueprint.CanTargetFriends && target.Entity != Caster && Caster.IsAlly(target.Entity) && !CanRedirectFromTarget(target))
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetAlly;
				return false;
			}
			if (!Blueprint.CanTargetEnemies && Caster.IsEnemy(target.Entity))
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetEnemy;
				return false;
			}
			if (target.Entity.IsDeadOrUnconscious && !Blueprint.CanCastToDeadTarget)
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetDead;
				return false;
			}
			if (!target.Entity.IsDeadOrUnconscious && !Blueprint.CanCastToAliveTarget())
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetAlive;
				return false;
			}
			AbilityCanTargetOnlyPetUnits canTargetOnlyPetUnitsComponent = Blueprint.CanTargetOnlyPetUnitsComponent;
			if (canTargetOnlyPetUnitsComponent != null)
			{
				if (canTargetOnlyPetUnitsComponent.Inverted)
				{
					if (target.Entity is BaseUnitEntity { IsPet: not false })
					{
						unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
						return false;
					}
				}
				else
				{
					if (!(target.Entity is BaseUnitEntity { IsPet: not false } baseUnitEntity2))
					{
						unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
						return false;
					}
					if (canTargetOnlyPetUnitsComponent.CanTargetOnlyOwnersPet && baseUnitEntity2.Master != Caster)
					{
						unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
						return false;
					}
				}
			}
		}
		if (!Blueprint.CanTargetSelf && TargetAnchor == AbilityTargetAnchor.Point && Caster.GetOccupiedNodes(casterPosition).Contains(target.NearestNode))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotTargetSelf;
			return false;
		}
		UnitPartForbiddenAbilities optional = Caster.GetOptional<UnitPartForbiddenAbilities>();
		if (optional != null && !optional.AbilityAllowed(this, target))
		{
			unavailabilityReason = UnavailabilityReasonType.AbilityForbidden;
			return false;
		}
		if (Blueprint.IsSummoningUnit && WarhammerBlockManager.Instance.NodeContainsAny(target.NearestNode))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotTargetNotEmptyCell;
			return false;
		}
		unavailabilityReason = UnavailabilityReasonType.None;
		switch (TargetAnchor)
		{
		case AbilityTargetAnchor.Owner:
			if (target.HasEntity && target.Entity == Caster)
			{
				return true;
			}
			unavailabilityReason = UnavailabilityReasonType.NullTarget;
			return false;
		case AbilityTargetAnchor.Unit:
			if (target.Entity == null)
			{
				unavailabilityReason = UnavailabilityReasonType.NullTarget;
				return false;
			}
			if (Blueprint.CanTargetFriends || target.Entity.IsPlayerFaction || Caster.CanAttack(target.Entity))
			{
				return true;
			}
			unavailabilityReason = UnavailabilityReasonType.CannotTargetAlly;
			return false;
		case AbilityTargetAnchor.Point:
			return true;
		default:
			unavailabilityReason = UnavailabilityReasonType.Unknown;
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool IsCasterValid(Vector3 casterPosition)
	{
		UnavailabilityReasonType unavailabilityReason;
		return IsCasterValid(casterPosition, out unavailabilityReason);
	}

	public bool IsCasterValid(Vector3 casterPosition, out UnavailabilityReasonType unavailabilityReason)
	{
		if (!Caster.CanMove)
		{
			BlueprintComponent[] componentsArray = Blueprint.ComponentsArray;
			for (int i = 0; i < componentsArray.Length; i++)
			{
				if (componentsArray[i] is AbilityCustomLogic { IsMoveUnit: not false })
				{
					unavailabilityReason = UnavailabilityReasonType.CannotMove;
					return false;
				}
			}
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		if (!IgnoreUsingInThreateningArea && UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.CannotUse && !Caster.GetMechanicFeature(MechanicsFeatureType.CanShootInMelee).Value)
		{
			PartCombatGroup combatGroupOptional = Caster.GetCombatGroupOptional();
			if (combatGroupOptional != null)
			{
				foreach (UnitGroupMemory.UnitInfo enemy in combatGroupOptional.Memory.Enemies)
				{
					if (enemy.Unit.IsThreat(casterPosition.GetNearestNodeXZUnwalkable(), enemy.Unit.Position, Caster.SizeRect) && !enemy.Unit.IsOffEngageForTarget(Caster as BaseUnitEntity))
					{
						unavailabilityReason = UnavailabilityReasonType.CannotUseInThreatenedArea;
						return false;
					}
				}
			}
		}
		if (!Blueprint.IsWeaponAbility && AreaEffectsController.CheckConcussionEffect(nearestNodeXZUnwalkable))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotUseInConcussionArea;
			return false;
		}
		if (Blueprint.IsWeaponAbility && AreaEffectsController.CheckCantAttackEffect(nearestNodeXZUnwalkable))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotUseInCantAttackArea;
			return false;
		}
		if (Blueprint.IsPsykerAbility && AreaEffectsController.CheckInertWarpEffect(nearestNodeXZUnwalkable))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotUseInInertWarpArea;
			return false;
		}
		unavailabilityReason = UnavailabilityReasonType.None;
		return true;
	}

	public bool CanTarget(TargetWrapper target)
	{
		UnavailabilityReasonType? unavailableReason;
		return CanTarget(target, out unavailableReason);
	}

	public bool CanTarget(TargetWrapper target, out UnavailabilityReasonType? unavailableReason)
	{
		return CanTarget(target, Caster.Position, out unavailableReason, null);
	}

	public bool CanTargetFromDesiredPosition(TargetWrapper target)
	{
		UnavailabilityReasonType? unavailabilityReason;
		return CanTargetFromDesiredPosition(target, out unavailabilityReason);
	}

	public bool CanTargetFromDesiredPosition(TargetWrapper target, out UnavailabilityReasonType? unavailabilityReason)
	{
		return CanTarget(target, Game.Instance.VirtualPositionController.GetDesiredPosition(Caster), out unavailabilityReason, Game.Instance.VirtualPositionController.GetDesiredRotation(Caster));
	}

	private bool CanTarget(TargetWrapper target, Vector3 casterPosition, out UnavailabilityReasonType? unavailabilityReason, Vector3? casterDirection)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		int? casterDirection2 = ((casterDirection.HasValue && casterDirection.GetValueOrDefault().sqrMagnitude > 1E-06f) ? new int?(CustomGraphHelper.GuessDirection(casterDirection.Value)) : null);
		int distance;
		LosCalculations.CoverType los;
		return CanTargetFromNode(nearestNodeXZUnwalkable, null, target, out distance, out los, out unavailabilityReason, casterDirection2);
	}

	public bool CanTargetFromNode(CustomGridNodeBase casterNode, CustomGridNodeBase targetNodeHint, TargetWrapper target, out int distance, out LosCalculations.CoverType los, int? casterDirection = null)
	{
		UnavailabilityReasonType? unavailabilityReason;
		return CanTargetFromNode(casterNode, targetNodeHint, target, out distance, out los, out unavailabilityReason, casterDirection);
	}

	public bool CanTargetFromNode(CustomGridNodeBase casterNode, CustomGridNodeBase targetNodeHint, TargetWrapper target, out int distance, out LosCalculations.CoverType los, out UnavailabilityReasonType? unavailabilityReason, int? casterDirection = null)
	{
		MechanicEntity caster;
		bool num = TryGetCasterForDistanceCalculation(out caster);
		if (caster == null)
		{
			caster = Caster;
		}
		Vector3 vector = (num ? caster.CurrentUnwalkableNode.Vector3Position : casterNode.Vector3Position);
		distance = WarhammerGeometryUtils.DistanceToInCells(vector, caster.SizeRect, casterDirection.HasValue ? CustomGraphHelper.GetVector3Direction(casterDirection.Value) : caster.Forward, target.Point, target.SizeRect, target.Forward);
		los = LosCalculations.CoverType.None;
		if (!IsValid(target, vector, out var unavailabilityReason2))
		{
			unavailabilityReason = unavailabilityReason2;
			return false;
		}
		if (!this.IsPatternRestrictionPassed(target, out var unavailabilityReason3))
		{
			unavailabilityReason = ((unavailabilityReason3 != UnavailabilityReasonType.None) ? unavailabilityReason3 : UnavailabilityReasonType.AreaEffectsCannotOverlap);
			return false;
		}
		CustomGridNodeBase customGridNodeBase = targetNodeHint ?? target.NearestNode;
		CustomGridNodeBase bestShootingPosition = GetBestShootingPosition(casterNode, target);
		if (IsMelee && !LosCalculations.HasMeleeLos(bestShootingPosition, caster.SizeRect, customGridNodeBase, target.SizeRect))
		{
			unavailabilityReason = UnavailabilityReasonType.HasNoLosToTarget;
			return false;
		}
		if (target.HasEntity && target.Entity.Buffs.SelectComponents<UnitBuffUntargetableByAbilityGroups>().Any((UnitBuffUntargetableByAbilityGroups buff) => buff.IsBlocked(AbilityGroups)))
		{
			unavailabilityReason = UnavailabilityReasonType.UntargetableForAbilityGroup;
			return false;
		}
		if (IsRangeUnrestrictedForTarget(target))
		{
			unavailabilityReason = UnavailabilityReasonType.None;
			return true;
		}
		if (NeedLoS)
		{
			if (Blueprint.IsLosDefinedByPattern && Caster.IsInPlayerParty)
			{
				if (!GetPatternSettings().GetOrientedPattern(this, bestShootingPosition, customGridNodeBase).Nodes.Contains(customGridNodeBase))
				{
					unavailabilityReason = UnavailabilityReasonType.HasNoLosToTarget;
					return false;
				}
			}
			else if (RestrictedFiringArc != 0)
			{
				if (!IsTargetInsideRestrictedFiringArc(target, casterNode, casterDirection))
				{
					unavailabilityReason = UnavailabilityReasonType.HasNoLosToTarget;
					return false;
				}
			}
			else if (!LosCalculations.HasLos(UseBestShootingPosition ? bestShootingPosition : casterNode, Caster.SizeRect, customGridNodeBase, target.SizeRect))
			{
				unavailabilityReason = UnavailabilityReasonType.HasNoLosToTarget;
				return false;
			}
		}
		if (distance < MinRangeCells)
		{
			unavailabilityReason = UnavailabilityReasonType.TargetTooClose;
			return false;
		}
		if (distance > RangeCells)
		{
			unavailabilityReason = UnavailabilityReasonType.TargetTooFar;
			return false;
		}
		unavailabilityReason = UnavailabilityReasonType.None;
		return true;
	}

	public int GetAvailableForCastCount()
	{
		int num = -1;
		if (m_ConvertedFrom != null)
		{
			num = m_ConvertedFrom.GetAvailableForCastCount();
		}
		else
		{
			ItemEntity sourceItem = SourceItem;
			if (sourceItem != null && sourceItem.IsSpendCharges)
			{
				num = SourceItem.Charges;
			}
			else if (IsWeaponAttackThatRequiresAmmo)
			{
				num = SourceWeapon.CurrentAmmo / AmmoRequired;
			}
		}
		int num2 = 0;
		int num3 = 0;
		if (Fact?.UsagesPerDayResource != null)
		{
			num3 = Fact.ConcreteOwner.GetAbilityResourcesOptional()?.GetResourceAmount(Fact.UsagesPerDayResource) ?? 0;
			num2 = 1;
		}
		if ((bool)OverrideRequiredResource || ResourceLogic != null)
		{
			num3 = GetResourceAmount();
			num2 = ResourceLogic?.CalculateCost(this) ?? 1;
		}
		if (num2 > 0)
		{
			int num4 = num3 / num2;
			int num5 = ((num > -1) ? Math.Min(num4, num) : num4);
			num = ((num > -1) ? Math.Min(num, num5) : num5);
		}
		return num;
	}

	public int GetResourceCost()
	{
		int result = -1;
		IAbilityResourceLogic resourceLogic = ResourceLogic;
		if (resourceLogic != null && resourceLogic.IsSpendResource())
		{
			result = ResourceLogic?.CalculateCost(this) ?? 1;
		}
		return result;
	}

	public int GetResourceAmount()
	{
		if ((bool)OverrideRequiredResource && RequiredResource != null)
		{
			PartAbilityResourceCollection abilityResourcesOptional = Caster.GetAbilityResourcesOptional();
			if (abilityResourcesOptional != null)
			{
				return abilityResourcesOptional.GetResourceAmount(RequiredResource);
			}
		}
		return ResourceLogic?.CalculateResourceAmount(this) ?? (-1);
	}

	public List<UnavailabilityReasonType> GetUnavailabilityReasons()
	{
		return GetUnavailabilityReasons(Caster.Position);
	}

	private List<UnavailabilityReasonType> GetUnavailabilityReasons(Vector3 castPosition)
	{
		if (m_ConvertedFrom != null)
		{
			return m_ConvertedFrom.GetUnavailabilityReasons(castPosition);
		}
		List<UnavailabilityReasonType> list = new List<UnavailabilityReasonType>();
		IAbilityCasterRestriction[] casterRestrictions = Blueprint.CasterRestrictions;
		for (int i = 0; i < casterRestrictions.Length; i++)
		{
			if (!casterRestrictions[i].IsCasterRestrictionPassed(Caster))
			{
				list.Add(UnavailabilityReasonType.CasterRestrictionNotPassed);
			}
		}
		if (Caster.IsPlayerFaction && RequireMaterialComponent && !HasEnoughMaterialComponent)
		{
			list.Add(UnavailabilityReasonType.MaterialComponentRequired);
		}
		if (IsWeaponAttackThatRequiresAmmo && SourceWeapon?.CurrentAmmo < AmmoRequired)
		{
			list.Add(UnavailabilityReasonType.NotEnoughAmmo);
		}
		if (Weapon != null && Blueprint.GetComponent<WeaponReloadLogic>() != null && Weapon.CurrentAmmo >= Weapon.Blueprint.WarhammerMaxAmmo)
		{
			list.Add(UnavailabilityReasonType.AlreadyFullAmmo);
		}
		CustomGridNodeBase node = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(castPosition).node;
		if (!Blueprint.IsWeaponAbility && AreaEffectsController.CheckConcussionEffect(node))
		{
			list.Add(UnavailabilityReasonType.CannotUseInConcussionArea);
		}
		if (Blueprint.IsWeaponAbility && AreaEffectsController.CheckCantAttackEffect(node))
		{
			list.Add(UnavailabilityReasonType.CannotUseInCantAttackArea);
		}
		if (Blueprint.IsPsykerAbility && AreaEffectsController.CheckInertWarpEffect(node))
		{
			list.Add(UnavailabilityReasonType.CannotUseInInertWarpArea);
		}
		if (IsOnCooldownUntilEndOfCombat)
		{
			list.Add(UnavailabilityReasonType.IsOnCooldownUntilEndOfCombat);
		}
		if (IsOnCooldown)
		{
			list.Add(UnavailabilityReasonType.IsOnCooldown);
		}
		if (!HasRequiredParams)
		{
			list.Add(UnavailabilityReasonType.UnitFactRequired);
		}
		Ability fact = Fact;
		if (fact != null && !fact.Active)
		{
			list.Add(UnavailabilityReasonType.AbilityDisabled);
		}
		IAbilityRestriction[] restrictions = Blueprint.Restrictions;
		for (int i = 0; i < restrictions.Length; i++)
		{
			if (!restrictions[i].IsAbilityRestrictionPassed(this))
			{
				list.Add(UnavailabilityReasonType.AbilityRestrictionNotPassed);
			}
		}
		if (IsUltimate && Game.Instance.TurnController.IsUltimateAbilityUsedThisRound)
		{
			list.Add(UnavailabilityReasonType.IsUltimateAbilityUsedThisRound);
		}
		if (Blueprint.CombatStateRestriction == BlueprintAbility.CombatStateRestrictionType.InCombatOnly && !Caster.IsInCombat)
		{
			list.Add(UnavailabilityReasonType.OnlyUseInCombat);
		}
		return list;
	}

	public string GetUnavailableReason(Vector3 casterPosition)
	{
		List<UnavailabilityReasonType> unavailabilityReasons = GetUnavailabilityReasons(casterPosition);
		if (unavailabilityReasons.Count != 0)
		{
			return GetUnavailabilityReasonString(unavailabilityReasons[0]);
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public string GetUnavailabilityReasonString(UnavailabilityReasonType type)
	{
		return GetUnavailabilityReasonString(type, null, null);
	}

	public string GetUnavailabilityReasonString(UnavailabilityReasonType type, Vector3 casterPosition, [NotNull] TargetWrapper target)
	{
		return GetUnavailabilityReasonString(type, (Vector3?)casterPosition, target);
	}

	private string GetUnavailabilityReasonString(UnavailabilityReasonType type, Vector3? casterPosition, [CanBeNull] TargetWrapper target)
	{
		string unavailabilityReasonStringInternal = GetUnavailabilityReasonStringInternal(type, casterPosition, target);
		if (!unavailabilityReasonStringInternal.IsNullOrEmpty())
		{
			return unavailabilityReasonStringInternal;
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	private string GetUnavailabilityReasonStringInternal(UnavailabilityReasonType type, Vector3? casterPosition, [CanBeNull] TargetWrapper target)
	{
		switch (type)
		{
		case UnavailabilityReasonType.OnlyUseInCombat:
			return LocalizedTexts.Instance.Reasons.CombatRequired;
		case UnavailabilityReasonType.AbilityDisabled:
			return LocalizedTexts.Instance.Reasons.AbilityDisabled;
		case UnavailabilityReasonType.CasterRestrictionNotPassed:
		{
			IAbilityCasterRestriction[] casterRestrictions = Blueprint.CasterRestrictions;
			foreach (IAbilityCasterRestriction abilityCasterRestriction in casterRestrictions)
			{
				if (!abilityCasterRestriction.IsCasterRestrictionPassed(Caster))
				{
					return abilityCasterRestriction.GetAbilityCasterRestrictionUIText(Caster);
				}
			}
			break;
		}
		case UnavailabilityReasonType.AbilityRestrictionNotPassed:
		{
			IAbilityRestriction[] restrictions = Blueprint.Restrictions;
			foreach (IAbilityRestriction abilityRestriction in restrictions)
			{
				if (!abilityRestriction.IsAbilityRestrictionPassed(this))
				{
					return abilityRestriction.GetAbilityRestrictionUIText();
				}
			}
			break;
		}
		case UnavailabilityReasonType.TargetRestrictionNotPassed:
		{
			if (!casterPosition.HasValue || !(target != null))
			{
				break;
			}
			IAbilityTargetRestriction[] targetRestrictions = Blueprint.TargetRestrictions;
			foreach (IAbilityTargetRestriction abilityTargetRestriction in targetRestrictions)
			{
				if (!abilityTargetRestriction.IsTargetRestrictionPassed(this, target, casterPosition.Value))
				{
					return abilityTargetRestriction.GetAbilityTargetRestrictionUIText(this, target, casterPosition.Value);
				}
			}
			break;
		}
		case UnavailabilityReasonType.MaterialComponentRequired:
			return string.Concat(LocalizedTexts.Instance.Reasons.MaterialComponentRequired, ": ", Blueprint.MaterialComponent.Item.Name);
		case UnavailabilityReasonType.NotEnoughAmmo:
			return LocalizedTexts.Instance.Reasons.NotEnoughAmmo;
		case UnavailabilityReasonType.AlreadyFullAmmo:
			return LocalizedTexts.Instance.Reasons.AlreadyFullAmmo;
		case UnavailabilityReasonType.IsOnCooldown:
			return LocalizedTexts.Instance.Reasons.IsOnCooldown;
		case UnavailabilityReasonType.IsOnCooldownUntilEndOfCombat:
			return LocalizedTexts.Instance.Reasons.IsOnCooldownUntilEndOfCombat;
		case UnavailabilityReasonType.CannotUseInThreatenedArea:
			return LocalizedTexts.Instance.Reasons.CannotUseInThreatenedArea;
		case UnavailabilityReasonType.TargetTooFar:
			return LocalizedTexts.Instance.Reasons.TargetTooFar;
		case UnavailabilityReasonType.TargetTooClose:
			return LocalizedTexts.Instance.Reasons.TargetTooClose;
		case UnavailabilityReasonType.HasNoLosToTarget:
			return LocalizedTexts.Instance.Reasons.HasNoLosToTarget;
		case UnavailabilityReasonType.AreaEffectsCannotOverlap:
			return LocalizedTexts.Instance.Reasons.AreaEffectsCannotOverlap;
		case UnavailabilityReasonType.StrategistZonesCantOverlap:
			return LocalizedTexts.Instance.Reasons.StrategistZonesCantOverlap;
		case UnavailabilityReasonType.IsUltimateAbilityUsedThisRound:
			return LocalizedTexts.Instance.Reasons.AlreadyDesperateMeasuredThisTurn;
		case UnavailabilityReasonType.CannotTargetAlly:
			return LocalizedTexts.Instance.Reasons.TargetIsEnemy;
		case UnavailabilityReasonType.CannotTargetEnemy:
			return LocalizedTexts.Instance.Reasons.TargetIsAlly;
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public bool IsVisible()
	{
		if (m_CachedVisibilityProviders == null)
		{
			m_CachedVisibilityProviders = Blueprint.GetComponents<IAbilityVisibilityProvider>().ToArray();
		}
		IAbilityVisibilityProvider[] cachedVisibilityProviders = m_CachedVisibilityProviders;
		for (int i = 0; i < cachedVisibilityProviders.Length; i++)
		{
			if (!cachedVisibilityProviders[i].IsAbilityVisible(this))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanBeConvertedTo(BlueprintAbility otherSpell)
	{
		if (Blueprint == otherSpell)
		{
			return true;
		}
		if (Blueprint.HasVariant(otherSpell))
		{
			return true;
		}
		return false;
	}

	public IEnumerable<AbilityData> GetConversions()
	{
		List<AbilityData> result = null;
		ReferenceArrayProxy<BlueprintAbility>? referenceArrayProxy = Blueprint.GetComponent<AbilityVariants>()?.Variants;
		if (referenceArrayProxy.HasValue)
		{
			foreach (BlueprintAbility item in referenceArrayProxy.Value)
			{
				AddAbilityUnique(ref result, new AbilityData(this, item));
			}
		}
		IEnumerable<AbilityData> enumerable = result;
		return enumerable ?? Enumerable.Empty<AbilityData>();
	}

	private static void AddAbilityUnique([CanBeNull] ref List<AbilityData> result, AbilityData ability)
	{
		result = result ?? TempList.Get<AbilityData>();
		foreach (AbilityData item in result)
		{
			if (ability.Equals(item))
			{
				return;
			}
		}
		result.Add(ability);
	}

	public bool TryGetCasterNodeForDistanceCalculation(out CustomGridNodeBase node, bool forUi = false)
	{
		node = (TryGetCasterForDistanceCalculation(out var caster, forUi) ? caster.CurrentUnwalkableNode : null);
		return node != null;
	}

	public bool TryGetCasterForDistanceCalculation(out MechanicEntity caster, bool forUi = false)
	{
		caster = (Blueprint.TryGetComponent<IAbilityOverrideCasterForRange>(out var component) ? component.GetCaster(Caster, forUi) : null);
		return caster != null;
	}

	public CustomGridNodeBase GetBestShootingPosition(TargetWrapper target)
	{
		return GetBestShootingPosition(Caster.CurrentUnwalkableNode, target);
	}

	public CustomGridNodeBase GetBestShootingPositionForDesiredPosition(TargetWrapper target)
	{
		return GetBestShootingPosition((Game.Instance.VirtualPositionController?.GetDesiredPosition(Caster) ?? Caster.Position).GetNearestNodeXZUnwalkable(), target);
	}

	public CustomGridNodeBase GetBestShootingPosition(CustomGridNodeBase castNode, TargetWrapper target)
	{
		if (!UseBestShootingPosition)
		{
			return castNode;
		}
		return LosCalculations.GetBestShootingNode(castNode, Caster.SizeRect, target.NearestNode, target.SizeRect, Caster);
	}

	public OrientedPatternData GetPattern(TargetWrapper target, Vector3 casterPosition)
	{
		IAbilityAoEPatternProvider patternSettings = GetPatternSettings();
		if (patternSettings == null)
		{
			return OrientedPatternData.Empty;
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = target.Point.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = casterPosition.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase bestShootingPosition = GetBestShootingPosition(nearestNodeXZUnwalkable2, target);
		return patternSettings.GetOrientedPattern(this, bestShootingPosition, nearestNodeXZUnwalkable);
	}

	public IAbilityAoEPatternProvider GetPatternSettings()
	{
		return PartAbilityPatternSettings.GetAbilityPatternSettings(this);
	}

	public int CalculateActionPointCost()
	{
		if (!IsFreeAction)
		{
			return Rulebook.Trigger(new RuleCalculateAbilityActionPointCost(Caster, this)).Result;
		}
		return 0;
	}

	[NotNull]
	public HashSet<CustomGridNodeBase> GetRestrictedFiringArcNodes(CustomGridNodeBase overridePosition = null, int? overrideDirection = null)
	{
		if (RestrictedFiringArc != 0)
		{
			return StarshipWeaponSlot.GetRestrictedFiringArcNodes(RangeCells, Blueprint.GetComponent<RestrictedFiringAreaComponent>(), overridePosition, overrideDirection);
		}
		return TempHashSet.Get<CustomGridNodeBase>();
	}

	public bool IsTargetInsideRestrictedFiringArc(Vector3 targetPosition, CustomGridNodeBase overridePosition = null, int? overrideDirection = null)
	{
		if (RestrictedFiringArc == RestrictedFiringArc.None)
		{
			return true;
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = targetPosition.GetNearestNodeXZUnwalkable();
		HashSet<CustomGridNodeBase> restrictedFiringArcNodes = GetRestrictedFiringArcNodes(overridePosition, overrideDirection);
		if (nearestNodeXZUnwalkable == null)
		{
			return false;
		}
		BaseUnitEntity unit = nearestNodeXZUnwalkable.GetUnit();
		if (unit == null)
		{
			return restrictedFiringArcNodes.Contains(nearestNodeXZUnwalkable);
		}
		foreach (CustomGridNodeBase occupiedNode in unit.GetOccupiedNodes())
		{
			if (restrictedFiringArcNodes.Contains(occupiedNode))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsTargetInsideRestrictedFiringArc(TargetWrapper target, CustomGridNodeBase overridePosition = null, int? overrideDirection = null)
	{
		if (RestrictedFiringArc == RestrictedFiringArc.None)
		{
			return true;
		}
		return StarshipWeaponSlot.IsTargetInsideRestrictedFiringArc(target, RangeCells, Blueprint.GetComponent<RestrictedFiringAreaComponent>(), overridePosition, overrideDirection);
	}

	public bool IsRangeUnrestrictedForTarget(TargetWrapper target)
	{
		return Blueprint.GetComponent<AbilityUnrestrictedRangeForTarget>()?.IsRangeUnrestrictedForTarget(this, target) ?? false;
	}

	public float CalculateDodgeChanceCached(UnitEntity unit, LosCalculations.CoverType coverType)
	{
		if (m_DodgeChanceCache == null)
		{
			m_DodgeChanceCache = new Dictionary<(string, byte), float>();
		}
		(string, byte) key = (unit.UniqueId, (byte)coverType);
		if (m_DodgeChanceCache.TryGetValue(key, out var value))
		{
			return value;
		}
		float num = CalculateDodgeChance(unit, coverType);
		m_DodgeChanceCache.Add(key, num);
		return num;
	}

	public float CalculateDodgeChance(UnitEntity unit, LosCalculations.CoverType coverType)
	{
		return (float)Rulebook.Trigger(new RuleCalculateDodgeChance(unit, Caster, this, coverType)).Result / 100f;
	}

	public bool HasLosCached(CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		if (m_LosCache == null)
		{
			m_LosCache = new Dictionary<(CustomGridNodeBase, CustomGridNodeBase), bool>();
		}
		(CustomGridNodeBase, CustomGridNodeBase) key = (fromNode, toNode);
		if (m_LosCache.TryGetValue(key, out var value))
		{
			return value;
		}
		bool flag = HasLos(fromNode, toNode);
		m_LosCache.Add(key, flag);
		return flag;
	}

	public bool HasLos(CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		return LosCalculations.HasLos(fromNode, default(IntRect), toNode, default(IntRect));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		stringBuilder.Append(Blueprint.ToString());
		stringBuilder.Append("[caster=");
		string value = Caster.GetDescriptionOptional()?.Name ?? Caster.Blueprint.name;
		stringBuilder.Append(value);
		if (SourceItem != null)
		{
			stringBuilder.Append(", item=");
			stringBuilder.Append(SourceItem.Blueprint.ToString());
			if (SourceItem.IsSpendCharges)
			{
				stringBuilder.Append("(charges=");
				stringBuilder.Append($"{((SourceItem.Count > 1) ? SourceItem.Count : SourceItem.Charges)}");
				stringBuilder.Append(")");
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public bool CanRedirectFromTarget(TargetWrapper target)
	{
		if (CanRedirect)
		{
			MechanicEntity entity = target.Entity;
			if (entity != null && entity.Facts.Contains(Root.WH.AbilityRedirect.AllowRedirectFact))
			{
				return RedirectSettings.ClickedTargetRestrictions.IsPassed(new PropertyContext(this, entity));
			}
		}
		return false;
	}

	public IEnumerable<MechanicEntity> CalculateRedirectTargets(TargetWrapper mainTarget)
	{
		AbilityRedirect redirect = RedirectSettings;
		Vector3 direction = Quaternion.Euler(0f, mainTarget.Orientation, 0f) * Vector3.forward;
		NodeList nodes = RedirectPattern.GetOriented(mainTarget.NearestNode, direction).Nodes;
		List<BaseUnitEntity> tmpList = ListPool<BaseUnitEntity>.Claim();
		tmpList.Clear();
		foreach (CustomGridNodeBase item in nodes)
		{
			BaseUnitEntity unit = item.GetUnit();
			if (unit != null && IsValidTargetForRedirect(redirect, mainTarget, unit) && !tmpList.Contains(unit))
			{
				tmpList.Add(unit);
				yield return unit;
			}
		}
		tmpList.Clear();
		ListPool<BaseUnitEntity>.Release(tmpList);
	}

	private bool IsValidTargetForRedirect([NotNull] AbilityRedirect redirect, [NotNull] TargetWrapper mainTarget, [NotNull] BaseUnitEntity unit)
	{
		if (!redirect.IncludeClickedTarget && mainTarget.Entity == unit)
		{
			return false;
		}
		switch (redirect.RedirectTargetType)
		{
		case TargetType.Enemy:
			if (!Caster.IsEnemy(unit))
			{
				return false;
			}
			break;
		case TargetType.Ally:
			if (!Caster.IsAlly(unit))
			{
				return false;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case TargetType.Any:
			break;
		}
		return redirect.RedirectTargetRestrictions.IsPassed(new PropertyContext(this, unit));
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = m_CasterRef;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(m_ConvertedFrom);
		result.Append(ref val2);
		result.Append(ref m_IsCharge);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<Ability>.GetHash128(Fact);
		result.Append(ref val4);
		result.Append(UniqueId);
		Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(OverrideRequiredResource);
		result.Append(ref val5);
		bool val6 = ForceTargetOwner;
		result.Append(ref val6);
		int val7 = DecorationColorNumber;
		result.Append(ref val7);
		int val8 = DecorationBorderNumber;
		result.Append(ref val8);
		Hash128 val9 = ClassHasher<ItemEntityStarshipWeapon>.GetHash128(FakeStarshipWeapon);
		result.Append(ref val9);
		Hash128 val10 = ClassHasher<ItemEntityWeapon>.GetHash128(OverrideWeapon);
		result.Append(ref val10);
		int val11 = OverrideRateOfFire;
		result.Append(ref val11);
		int val12 = ItemSlotIndex;
		result.Append(ref val12);
		Hash128 val13 = ClassHasher<EntityFact>.GetHash128(ItemSlotSource);
		result.Append(ref val13);
		Hash128 val14 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(FXSettingsOverride);
		result.Append(ref val14);
		return result;
	}
}

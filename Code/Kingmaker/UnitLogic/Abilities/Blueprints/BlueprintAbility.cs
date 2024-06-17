using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.Enums;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CutsceneAttack;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using MemoryPack;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[Serializable]
[TypeId("da11db195c86e0d4dae17a2c03a4ba9a")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintAbility : BlueprintUnitFact, IBlueprintScanner, IResourceIdsHolder
{
	[Serializable]
	public class MaterialComponentData
	{
		[SerializeField]
		private BlueprintItemReference m_Item;

		public int Count;

		public BlueprintItem Item => m_Item?.Get();
	}

	public enum UsingInThreateningAreaType
	{
		WillCauseAOO,
		CanUseWithoutAOO,
		CannotUse
	}

	public enum UsingInOverwatchAreaType
	{
		WillCauseAttack,
		WillNotCauseAttack
	}

	public enum CombatStateRestrictionType
	{
		NoRestriction,
		InCombatOnly,
		NotInCombatOnly
	}

	public AbilityType Type;

	public AbilityRange Range;

	[ShowIf("IsRangeCustom")]
	public int CustomRange;

	[HideIf("IsRangePersonal")]
	public int MinRange;

	public int ActionPointCost = 1;

	public WarhammerAbilityParamsSource AbilityParamsSource = WarhammerAbilityParamsSource.None;

	[ShowIf("IsPsykerAbility")]
	public PsychicPower PsychicPower;

	[ShowIf("IsPsykerAbility")]
	[Tooltip("Используется для оверрайда значения выдаваемого от Psychic Power")]
	public int VeilThicknessPointsToAdd = 1;

	[ShowIf("IsSkillCheckAbilityParams")]
	public StatType ParamsSkill = StatType.SkillAthletics;

	public int CooldownRounds;

	[Tooltip("Включать имя данного спела в имя его варианта")]
	public bool ShowNameForVariant;

	[ShowIf("ShowNameForVariant")]
	[Tooltip("Применять опцию выше только, если кастер - IsPlayerFaction")]
	public bool OnlyForAllyCaster;

	public bool CanTargetPoint;

	public bool CanTargetEnemies;

	[InfoBox("Allows to cast on allies. But does not prevent from casting on enemies if only selected")]
	public bool CanTargetFriends;

	public bool CanTargetSelf = true;

	public bool SpellResistance;

	public bool ActionBarAutoFillIgnored;

	public bool Hidden;

	public bool DisableBestShootingPosition;

	public bool NeedEquipWeapons;

	public bool NotOffensive;

	public bool ShowInDialogue;

	public AbilityEffectOnUnit EffectOnAlly;

	public AbilityEffectOnUnit EffectOnEnemy;

	[SerializeField]
	private BlueprintAbilityReference m_Parent;

	public UnitAnimationActionCastSpell.CastAnimationStyle Animation;

	public bool CastInOffHand;

	public bool UseOnMechadendrite;

	[ShowIf("UseOnMechadendrite")]
	public MechadendritesType UsedMechadendrite = MechadendritesType.Utility;

	[SerializeField]
	private bool m_TargetMapObjects;

	public bool IsFreeAction;

	public bool ShouldTurnToTarget = true;

	[SerializeField]
	private bool m_TolerantForInvisible;

	[SerializeField]
	private bool m_IsStratagem;

	public UsingInThreateningAreaType UsingInThreateningArea;

	public UsingInOverwatchAreaType UsingInOverwatchArea;

	public CombatStateRestrictionType CombatStateRestriction = CombatStateRestrictionType.InCombatOnly;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintAbilityGroupReference[] m_AbilityGroups;

	public LocalizedString LocalizedDuration;

	public LocalizedString LocalizedSavingThrow;

	public MaterialComponentData MaterialComponent = new MaterialComponentData
	{
		Count = 1
	};

	public bool DisableLog;

	public string[] ResourceAssetIds;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	[CanBeNull]
	private IAbilityRestriction[] m_CachedRestrictions;

	[CanBeNull]
	private IAbilityTargetRestriction[] m_CachedTargetRestrictions;

	[CanBeNull]
	private IAbilityCasterRestriction[] m_CachedCasterRestrictions;

	[CanBeNull]
	private IAbilityCanTargetPointRestriction[] m_CachedCanTargetPointRestrictions;

	[SerializeField]
	private AbilityTag m_AbilityTag;

	[SerializeField]
	private CombatHudCommandSetAsset m_CombatHudCommandsOverride;

	public AbilityTag AbilityTag => m_AbilityTag;

	public bool IsMomentum => CasterRestrictions.Any((IAbilityCasterRestriction x) => x is AbilitySpecialMomentumAction || x is AbilityMomentumLogic);

	public bool IsHeroicAct => CasterRestrictions.Any(delegate(IAbilityCasterRestriction x)
	{
		if (x is AbilitySpecialMomentumAction abilitySpecialMomentumAction)
		{
			if (abilitySpecialMomentumAction.MomentumType == MomentumAbilityType.HeroicAct)
			{
				goto IL_0026;
			}
		}
		else if (x is AbilityMomentumLogic { HeroicAct: not false })
		{
			goto IL_0026;
		}
		return false;
		IL_0026:
		return true;
	});

	public bool IsDesperateMeasure => CasterRestrictions.Any((IAbilityCasterRestriction x) => x is AbilitySpecialMomentumAction abilitySpecialMomentumAction && abilitySpecialMomentumAction.MomentumType == MomentumAbilityType.DesperateMeasure);

	public BlueprintAbility Parent
	{
		get
		{
			return m_Parent?.Get();
		}
		set
		{
			m_Parent = value.ToReference<BlueprintAbilityReference>();
		}
	}

	public bool IsWeaponAbility => AbilityParamsSource == WarhammerAbilityParamsSource.Weapon;

	public bool IsPsykerAbility => AbilityParamsSource == WarhammerAbilityParamsSource.PsychicPower;

	public bool IsSkillCheckAbilityParams => AbilityParamsSource == WarhammerAbilityParamsSource.SkillCheck;

	public bool IsGrenade => AbilityTag == AbilityTag.ThrowingGrenade;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] abilityGroups = m_AbilityGroups;
			return abilityGroups;
		}
	}

	[UsedImplicitly]
	private bool IsRangeCustom => Range == AbilityRange.Custom;

	private bool IsRangeWeapon => Range == AbilityRange.Weapon;

	[UsedImplicitly]
	private bool IsRangePersonal => Range == AbilityRange.Personal;

	public bool CanCastToDeadTarget => this.GetComponent<ICanTargetDeadUnits>() != null;

	public IAbilityAoEPatternProvider PatternSettings => GetPatternSettings();

	public IAbilityRestriction[] Restrictions
	{
		get
		{
			if (m_CachedRestrictions == null)
			{
				m_CachedRestrictions = this.GetComponents<IAbilityRestriction>().ToArray();
			}
			return m_CachedRestrictions;
		}
	}

	public IAbilityTargetRestriction[] TargetRestrictions
	{
		get
		{
			if (m_CachedTargetRestrictions == null)
			{
				m_CachedTargetRestrictions = this.GetComponents<IAbilityTargetRestriction>().ToArray();
			}
			return m_CachedTargetRestrictions;
		}
	}

	public IAbilityCasterRestriction[] CasterRestrictions
	{
		get
		{
			if (m_CachedCasterRestrictions == null)
			{
				m_CachedCasterRestrictions = this.GetComponents<IAbilityCasterRestriction>().ToArray();
			}
			return m_CachedCasterRestrictions;
		}
	}

	public IAbilityCanTargetPointRestriction[] CanTargetPointRestrictions
	{
		get
		{
			if (m_CachedCanTargetPointRestrictions == null)
			{
				m_CachedCanTargetPointRestrictions = this.GetComponents<IAbilityCanTargetPointRestriction>().ToArray();
			}
			return m_CachedCanTargetPointRestrictions;
		}
	}

	public SpellSchool School => SpellSchool.None;

	public int AoERadius => this.GetAoERadiusProvider()?.AoERadius ?? 0;

	public TargetType AoETargets => PatternSettings?.Targets ?? this.GetAoERadiusProvider()?.Targets ?? TargetType.Any;

	public bool HasVariants => this.GetComponent<AbilityVariants>();

	public bool TargetMapObjects => m_TargetMapObjects;

	public bool IsSpell => Type == AbilityType.Spell;

	public bool IsCantrip => false;

	public string RawDescription => base.Description;

	public SpellDescriptor SpellDescriptor => this.GetComponent<SpellDescriptorComponent>()?.Descriptor ?? ((SpellDescriptorWrapper)SpellDescriptor.None);

	public bool IsAoEDamage => base.ElementsArray.HasItem((Element e) => e is ContextActionDealDamage contextActionDealDamage && contextActionDealDamage.IsAoE);

	public bool IsAoE => this.GetComponent<AbilityTargetsInPattern>() != null;

	public bool TolerantForInvisible => m_TolerantForInvisible;

	public AttackAbilityType? AttackType => GetAttackType();

	public bool IsBurst => base.ComponentsArray.HasItem(delegate(BlueprintComponent i)
	{
		if (i is WarhammerAbilityAttackDelivery warhammerAbilityAttackDelivery)
		{
			if (warhammerAbilityAttackDelivery.IsBurst)
			{
				goto IL_0026;
			}
		}
		else if (i is AbilityCutsceneAttack { IsBurst: not false })
		{
			goto IL_0026;
		}
		return false;
		IL_0026:
		return true;
	});

	public bool IsLosDefinedByPattern => base.ComponentsArray.HasItem((BlueprintComponent i) => i is WarhammerAbilityAttackDelivery { IsScatterOrRangedPattern: not false } warhammerAbilityAttackDelivery && warhammerAbilityAttackDelivery.IsLosDefinedByPattern);

	public bool HasUnrestrictedRangeForTargetLogic => this.GetComponents<AbilityUnrestrictedRangeForTarget>() != null;

	public bool UseBestShootingPosition
	{
		get
		{
			if (!DisableBestShootingPosition && this.GetComponent<AbilityCustomDirectMovement>() == null)
			{
				WarhammerAbilityAttackDelivery component = this.GetComponent<WarhammerAbilityAttackDelivery>();
				return component == null || component.UseBestShootingPosition;
			}
			return false;
		}
	}

	public bool IsMoveUnit => this.GetComponent<AbilityCustomLogic>()?.IsMoveUnit ?? false;

	public bool IsCharge => this.GetComponent<AbilityCustomDirectMovement>()?.IsCharge ?? false;

	public bool IsDirectMovement => this.GetComponent<AbilityCustomDirectMovement>();

	public bool IsStratagem => m_IsStratagem;

	public CombatHudCommandSetAsset CombatHudCommandsOverride => m_CombatHudCommandsOverride;

	public bool IsSummoningUnit
	{
		get
		{
			if ((bool)this.GetComponent<AbilityCustomStarshipNPCTorpedoLaunch>())
			{
				return true;
			}
			return this.GetComponent<AbilityEffectRunAction>()?.Actions.Actions.Any((GameAction a) => a is WarhammerContextActionSpawnChildStarship) ?? false;
		}
	}

	public bool IsCustomProjectileDistribution => this.GetComponent<CustomProjectileDistribution>();

	public string ShortenedDescription => UIUtilityTexts.GetLongOrShortText(base.Description, state: false);

	public override string Description => UIUtilityTexts.GetLongOrShortText(base.Description, state: true);

	private WarhammerAbilityTooltipHelper TooltipHelper => this.GetComponent<WarhammerAbilityTooltipHelper>();

	public bool CanTargetPointAfterRestrictions(AbilityData abilityData)
	{
		if (CanTargetPoint)
		{
			return CanTargetPointRestrictions.ToList().All((IAbilityCanTargetPointRestriction checker) => checker.IsAbilityCanTargetPointRestrictionPassed(abilityData));
		}
		return false;
	}

	private AttackAbilityType? GetAttackType()
	{
		BlueprintComponent[] componentsArray = base.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is WarhammerAbilityAttackDelivery warhammerAbilityAttackDelivery)
			{
				if (warhammerAbilityAttackDelivery.IsMelee)
				{
					return AttackAbilityType.Melee;
				}
				if (warhammerAbilityAttackDelivery.IsScatter)
				{
					return AttackAbilityType.Scatter;
				}
				if (warhammerAbilityAttackDelivery.IsPattern)
				{
					return AttackAbilityType.Pattern;
				}
				return AttackAbilityType.SingleShot;
			}
		}
		return null;
	}

	public int GetRange()
	{
		return Range switch
		{
			AbilityRange.Personal => 0, 
			AbilityRange.Touch => 1, 
			AbilityRange.Unlimited => 100000, 
			AbilityRange.Weapon => -1, 
			AbilityRange.Custom => CustomRange, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetVeilThicknessPointsToAdd()
	{
		if (!IsPsykerAbility)
		{
			return 0;
		}
		int num = ((PsychicPower == PsychicPower.Major) ? BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.VeilThicknessPointsToAddForMajor : BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.VeilThicknessPointsToAddForMinor);
		if (num != VeilThicknessPointsToAdd)
		{
			return VeilThicknessPointsToAdd;
		}
		return num;
	}

	protected override Type GetFactType()
	{
		return typeof(Ability);
	}

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		return new Ability(this, owner);
	}

	public bool HasVariant(BlueprintAbility other)
	{
		return this.GetComponent<AbilityVariants>()?.Variants.HasReference(other) ?? false;
	}

	public bool IsInSpellList(BlueprintSpellList spellList)
	{
		try
		{
			SpellLevelList[] spellsByLevel = spellList.SpellsByLevel;
			for (int i = 0; i < spellsByLevel.Length; i++)
			{
				if (spellsByLevel[i].Spells.HasItem(this))
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
		}
	}

	public bool IsInSpellListOfUnit(BaseUnitEntity unit)
	{
		try
		{
			foreach (ClassData @class in unit.Progression.Classes)
			{
				BlueprintSpellbook spellbook = @class.Spellbook;
				if (spellbook != null && IsInSpellList(spellbook.SpellList))
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
		}
	}

	public string[] GetResourceIds()
	{
		return ResourceAssetIds;
	}

	[BlueprintButton]
	[UsedImplicitly]
	public void FixParentForVariants()
	{
	}

	public string GetShortenedDescription()
	{
		if (!SettingsRoot.Game.Tooltips.Shortened)
		{
			return Description;
		}
		return ShortenedDescription;
	}

	public string GetTarget(int weaponRange = -1, MechanicEntity caster = null)
	{
		AbilityTargetStrings abilityTargets = LocalizedTexts.Instance.AbilityTargets;
		AbilityRangeStrings abilityTargetRanges = LocalizedTexts.Instance.AbilityTargetRanges;
		StringBuilder stringBuilder = new StringBuilder();
		if (IsStratagem)
		{
			switch (TooltipHelper?.TargetType ?? TargetType.Any)
			{
			case TargetType.Ally:
				stringBuilder.Append(abilityTargets.AllAllies);
				break;
			case TargetType.Enemy:
				stringBuilder.Append(abilityTargets.AllEnemies);
				break;
			default:
				stringBuilder.Append(abilityTargets.AllCreatures);
				break;
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(abilityTargets.InsideSelectedCombatArea);
		}
		else if (IsCharge)
		{
			stringBuilder.Append(abilityTargets.FirstCreature);
			stringBuilder.Append(' ');
			stringBuilder.Append(string.Format(abilityTargets.WithinLine, GetRange()));
			stringBuilder.Append(' ');
		}
		else if (IsMoveUnit)
		{
			stringBuilder.Append(abilityTargets.Movement);
			if (IsRangeCustom && CustomRange > 0)
			{
				stringBuilder.Append(' ');
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), CustomRange));
			}
		}
		else if (Range == AbilityRange.Personal && PatternSettings == null)
		{
			switch (TooltipHelper?.TargetType)
			{
			case TargetType.Ally:
				stringBuilder.Append(abilityTargets.AllAllies);
				break;
			case TargetType.Enemy:
				stringBuilder.Append(abilityTargets.AllEnemies);
				break;
			default:
				stringBuilder.Append(abilityTargets.AllCreatures);
				break;
			case null:
				stringBuilder.Append(abilityTargets.Personal);
				break;
			}
		}
		else if (Range != 0 && PatternSettings == null)
		{
			if (CanTargetPoint)
			{
				stringBuilder.Append(abilityTargets.TargetPoint);
			}
			else
			{
				switch (TooltipHelper?.TargetType)
				{
				case TargetType.Ally:
					stringBuilder.Append(abilityTargets.AllAllies);
					break;
				case TargetType.Enemy:
					stringBuilder.Append(abilityTargets.AllEnemies);
					break;
				default:
					stringBuilder.Append(abilityTargets.AllCreatures);
					break;
				case null:
					if (CanTargetEnemies && CanTargetFriends)
					{
						stringBuilder.Append(abilityTargets.OneCreature);
					}
					else if (CanTargetEnemies && !CanTargetFriends)
					{
						stringBuilder.Append(abilityTargets.OneEnemyCreature);
					}
					else if (!CanTargetEnemies && CanTargetFriends)
					{
						stringBuilder.Append(abilityTargets.OneFriendlyCreature);
					}
					else
					{
						if (!CanTargetSelf)
						{
							break;
						}
						ContextActionOnAllUnitsInCombat contextActionOnAllUnitsInCombat = GetContextActionOnAllUnitsInCombat();
						if (contextActionOnAllUnitsInCombat == null)
						{
							stringBuilder.Append(abilityTargets.Personal);
							stringBuilder.Append(".\n");
							return stringBuilder.ToString();
						}
						if (contextActionOnAllUnitsInCombat.OnlyAllies)
						{
							stringBuilder.Append(abilityTargets.AllAllies);
							break;
						}
						if (!contextActionOnAllUnitsInCombat.OnlyEnemies)
						{
							stringBuilder.Append(abilityTargets.Personal);
							stringBuilder.Append(".\n");
							return stringBuilder.ToString();
						}
						stringBuilder.Append(abilityTargets.AllEnemies);
					}
					break;
				}
			}
			stringBuilder.Append(' ');
			if (IsRangeCustom || !abilityTargetRanges.Contains(Range))
			{
				int num;
				if (caster != null)
				{
					RuleCalculateAbilityRange ruleCalculateAbilityRange = Rulebook.Trigger(new RuleCalculateAbilityRange(caster, new AbilityData(this, caster)));
					num = ruleCalculateAbilityRange.OverrideRange ?? ruleCalculateAbilityRange.DefaultRange;
				}
				else
				{
					num = GetRange();
				}
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), num));
			}
			else if (IsRangeWeapon && weaponRange > 0)
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), weaponRange));
			}
			else
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(Range)));
			}
		}
		else if (IsBurst)
		{
			if (CanTargetEnemies && CanTargetFriends)
			{
				stringBuilder.Append(abilityTargets.FirstCreature);
			}
			else if (CanTargetEnemies && !CanTargetFriends)
			{
				stringBuilder.Append(abilityTargets.FirstEnemyCreature);
			}
			else if (!CanTargetEnemies && CanTargetFriends)
			{
				stringBuilder.Append(abilityTargets.FirstFriendlyCreature);
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(abilityTargets.EveryShot);
			stringBuilder.Append(' ');
			stringBuilder.Append(string.Format(abilityTargets.WithinCone, weaponRange));
		}
		else if (PatternSettings != null)
		{
			if (AoETargets == TargetType.Any)
			{
				stringBuilder.Append(abilityTargets.AllCreatures);
			}
			else if (AoETargets == TargetType.Enemy)
			{
				stringBuilder.Append(abilityTargets.AllEnemies);
			}
			else if (AoETargets == TargetType.Ally)
			{
				stringBuilder.Append(abilityTargets.AllAllies);
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(abilityTargets.InsideAreaOfEffect);
			stringBuilder.Append(' ');
			if (Range == AbilityRange.Personal)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			else if (IsRangeCustom)
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), CustomRange));
			}
			else if (IsRangeWeapon)
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), weaponRange));
			}
			else
			{
				stringBuilder.Append(abilityTargetRanges.GetText(Range));
			}
		}
		stringBuilder.Append(".\n");
		return stringBuilder.ToString();
	}

	private ContextActionOnAllUnitsInCombat GetContextActionOnAllUnitsInCombat()
	{
		return (from c in base.ComponentsArray
			select c as AbilityEffectRunAction into c
			where c != null
			select c).SelectMany((AbilityEffectRunAction a) => a.Actions.Actions).FirstOrDefault((GameAction a) => a is ContextActionOnAllUnitsInCombat) as ContextActionOnAllUnitsInCombat;
	}

	public Sprite GetTargetImage()
	{
		UIIcons uIIcons = BlueprintRoot.Instance.UIConfig.UIIcons;
		switch (TooltipHelper?.TargetType)
		{
		case TargetType.Enemy:
			return uIIcons.TargetEnemyAll;
		case TargetType.Ally:
			return uIIcons.TargetAllyAll;
		default:
			return uIIcons.TargetAnyAll;
		case null:
			if (PatternSettings != null)
			{
				if (IsMoveUnit)
				{
					return uIIcons.TargetCharge;
				}
				if (AoETargets == TargetType.Any)
				{
					return uIIcons.TargetAnyAll;
				}
				if (AoETargets == TargetType.Enemy)
				{
					return uIIcons.TargetEnemyAll;
				}
				if (AoETargets == TargetType.Ally)
				{
					return uIIcons.TargetAllyAll;
				}
				return null;
			}
			if (Range == AbilityRange.Personal)
			{
				return uIIcons.TargetPersonal;
			}
			if (CanTargetPoint)
			{
				return uIIcons.SpellTargetPoint;
			}
			if (CanTargetEnemies && CanTargetFriends)
			{
				return uIIcons.TargetAnyOne;
			}
			if (CanTargetEnemies && !CanTargetFriends)
			{
				return uIIcons.TargetEnemyOne;
			}
			if (!CanTargetEnemies && CanTargetFriends)
			{
				return uIIcons.TargetAllyOne;
			}
			if (CanTargetSelf)
			{
				ContextActionOnAllUnitsInCombat contextActionOnAllUnitsInCombat = GetContextActionOnAllUnitsInCombat();
				if (contextActionOnAllUnitsInCombat != null)
				{
					if (contextActionOnAllUnitsInCombat.OnlyAllies)
					{
						return uIIcons.TargetAllyAll;
					}
					if (contextActionOnAllUnitsInCombat.OnlyEnemies)
					{
						return uIIcons.TargetEnemyAll;
					}
				}
				return uIIcons.TargetPersonal;
			}
			return null;
		}
	}

	private IAbilityAoEPatternProvider GetPatternSettings()
	{
		IAbilityAoEPatternProvider abilityAoEPatternProvider = this.GetComponent<IAbilityAoEPatternProviderHolder>()?.PatternProvider ?? this.GetComponent<IAbilityAoEPatternProvider>();
		if (abilityAoEPatternProvider != null)
		{
			return abilityAoEPatternProvider;
		}
		foreach (Element item in base.ElementsArray)
		{
			if (item is ContextActionSpawnAreaEffect contextActionSpawnAreaEffect)
			{
				return contextActionSpawnAreaEffect.AreaEffect;
			}
		}
		return null;
	}

	public void Scan()
	{
	}

	private void ScanAbility(ICollection<ContextDiceValue> diceValues, ICollection<ContextDurationValue> durationValues)
	{
	}
}

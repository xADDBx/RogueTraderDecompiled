using System;
using System.Collections.Generic;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.NodeLink;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SystemMechanicsRoot
{
	[Serializable]
	public class AiImpatienceSettings
	{
		public ImpatienceLevel Type;

		public int InefficientAttacksCountTreshold;

		[Range(0f, 100f)]
		public int SwitchTargetChance;
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DeathDoorBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_SummonedUnitBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_SummonedTorpedoesBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DismemberedUnitBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_REAvoidanceCriticalFailureBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_ChargeBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_ResurrectionBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DisintegrateBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DimensionDoorBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFeatureReference m_TwoWeaponFightingBasicMechanics;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_BuffDefaultSource;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_ChargeAbility;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_DefaultUnit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_EmptyMechanicEntity;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_LedgermainUnit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintSummonPoolReference m_LedgermainPool;

	public GameObject FadeOutFx;

	public GameObject FadeInFx;

	public GameObject DeathTintFx;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFeatureReference m_UndeadType;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DisarmMainHandBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DisarmOffHandBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DisableArmorInRestEncounterBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_SunderArmorBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DirtyTrickBlindnessBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DirtyTrickEntangledBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DirtyTrickSickenedBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_SummonedUnitAppearBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemWeaponReference m_RayWeapon;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_FactionNeutrals;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_FactionMobs;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_FactionCutsceneNeutral;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_GoldCoin;

	private bool m_SpellProjectilesSorted;

	[SerializeField]
	[ValidateNotNull]
	private ConsumablesRoot m_Consumables;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintVendorFactionsRoot.Reference m_VendorFactionsRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintNodeLinkRoot.Reference m_NodeLinkRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintCargoRoot.Reference m_CargoRoot;

	public List<CommonSpellProjectile> SpellProjectiles;

	public float MinMetersToRun = 2.25f;

	[Header("Reach hit FX")]
	public float ReachFXBaseRange = 1.5f;

	public GameObject ReachFXTargetPrefab;

	public GameObject ReachFXMovingPrefab;

	public string ReachFXLocatorName = "Locator_HitFX_00";

	public float ReachFXFlightTime = 0.3f;

	[Header("Tutorial")]
	public int TutorialCooldownSeconds = 60;

	public float TutorialDelaySeconds = 3f;

	public float TutorialDelaySecondsAfterLoading = 10f;

	[Header("AI")]
	public AiImpatienceSettings[] Impatience = new AiImpatienceSettings[0];

	[Header("Locust")]
	[InfoBox("Progression formula: An = start + step * (n * (n + 1) / 2 - 1), A1 = start")]
	public int SwarmStrengthToScaleStart = 100;

	public int SwarmStrengthToScaleStep = 100;

	public BlueprintBuff DeathDoorBuff => m_DeathDoorBuff?.Get();

	public BlueprintBuff SummonedUnitBuff => m_SummonedUnitBuff?.Get();

	public BlueprintBuff SummonedTorpedoesBuff => m_SummonedTorpedoesBuff?.Get();

	public BlueprintBuff DismemberedUnitBuff => m_DismemberedUnitBuff?.Get();

	public BlueprintBuff REAvoidanceCriticalFailureBuff => m_REAvoidanceCriticalFailureBuff?.Get();

	public BlueprintBuff ChargeBuff => m_ChargeBuff?.Get();

	public BlueprintBuff ResurrectionBuff => m_ResurrectionBuff?.Get();

	public BlueprintBuff DisintegrateBuff => m_DisintegrateBuff?.Get();

	public BlueprintBuff DimensionDoorBuff => m_DimensionDoorBuff;

	public BlueprintAbility BuffDefaultSource => m_BuffDefaultSource?.Get();

	public BlueprintAbility ChargeAbility => m_ChargeAbility?.Get();

	public BlueprintUnit DefaultUnit => m_DefaultUnit?.Get();

	public BlueprintMechanicEntityFact EmptyMechanicEntity => m_EmptyMechanicEntity;

	public BlueprintUnit LedgermainUnit => m_LedgermainUnit?.Get();

	public BlueprintSummonPool LedgermainPool => m_LedgermainPool?.Get();

	public BlueprintFeature UndeadType => m_UndeadType?.Get();

	public BlueprintBuff DisarmMainHandBuff => m_DisarmMainHandBuff?.Get();

	public BlueprintBuff DisarmOffHandBuff => m_DisarmOffHandBuff?.Get();

	public BlueprintBuff DisableArmorInRestEncounterBuff => m_DisableArmorInRestEncounterBuff?.Get();

	public BlueprintBuff SunderArmorBuff => m_SunderArmorBuff?.Get();

	public BlueprintBuff DirtyTrickBlindnessBuff => m_DirtyTrickBlindnessBuff?.Get();

	public BlueprintBuff DirtyTrickEntangledBuff => m_DirtyTrickEntangledBuff?.Get();

	public BlueprintBuff DirtyTrickSickenedBuff => m_DirtyTrickSickenedBuff?.Get();

	public BlueprintBuff SummonedUnitAppearBuff => m_SummonedUnitAppearBuff?.Get();

	public BlueprintItemWeapon RayWeapon => m_RayWeapon?.Get();

	public BlueprintFaction FactionNeutrals => m_FactionNeutrals?.Get();

	public BlueprintFaction FactionMobs => m_FactionMobs?.Get();

	public BlueprintFaction FactionCutsceneNeutral => m_FactionCutsceneNeutral?.Get();

	public BlueprintItem GoldCoin => m_GoldCoin?.Get();

	public BlueprintCargoRoot CargoRoot => m_CargoRoot?.Get();

	public BlueprintNodeLinkRoot BlueprintNodeLinkRoot => m_NodeLinkRoot?.Get();

	public ConsumablesRoot Consumables => m_Consumables;

	public BlueprintVendorFactionsRoot VendorFactionsRoot => m_VendorFactionsRoot?.Get();

	[CanBeNull]
	public BlueprintProjectile GetSpellProjectile(SpellSchool school, SpellDescriptor descriptor)
	{
		if (!m_SpellProjectilesSorted)
		{
			SpellProjectiles.Sort();
		}
		foreach (CommonSpellProjectile spellProjectile in SpellProjectiles)
		{
			if (spellProjectile.School == school || spellProjectile.School == SpellSchool.None)
			{
				if ((descriptor & spellProjectile.Descriptors) == spellProjectile.Descriptors)
				{
					return spellProjectile.Projectile;
				}
				if (spellProjectile.Descriptors.Value == SpellDescriptor.None)
				{
					return spellProjectile.Projectile;
				}
			}
		}
		return null;
	}
}

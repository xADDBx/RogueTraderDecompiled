using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CheatRoot
{
	[Serializable]
	public class Party
	{
		[SerializeField]
		[FormerlySerializedAs("Player")]
		private BlueprintUnitReference m_Player;

		[SerializeField]
		[FormerlySerializedAs("Companions")]
		private BlueprintUnitReference[] m_Companions;

		[SerializeField]
		[FormerlySerializedAs("CompanionFakeBlueprints")]
		private BlueprintUnitReference[] m_CompanionFakeBlueprints;

		public BlueprintUnit Player => m_Player?.Get();

		public ReferenceArrayProxy<BlueprintUnit> Companions
		{
			get
			{
				BlueprintReference<BlueprintUnit>[] companions = m_Companions;
				return companions;
			}
		}

		public ReferenceArrayProxy<BlueprintUnit> CompanionFakeBlueprints
		{
			get
			{
				BlueprintReference<BlueprintUnit>[] companionFakeBlueprints = m_CompanionFakeBlueprints;
				return companionFakeBlueprints;
			}
		}
	}

	[SerializeField]
	[FormerlySerializedAs("Iddqd")]
	private BlueprintBuffReference m_Iddqd;

	[SerializeField]
	[FormerlySerializedAs("Empowered")]
	private BlueprintBuffReference m_Empowered;

	[SerializeField]
	[FormerlySerializedAs("Enemy")]
	private BlueprintUnitReference m_Enemy;

	[SerializeField]
	[FormerlySerializedAs("PrefabUnit")]
	private BlueprintUnitReference m_PrefabUnit;

	[SerializeField]
	[FormerlySerializedAs("FullBuffList")]
	private BlueprintBuffReference[] m_FullBuffList;

	public Party[] DefaultParties;

	public DamageDescription TestDamage = new DamageDescription
	{
		Bonus = 10
	};

	[Tooltip("Press key O for execute")]
	public ActionList TestAction;

	[Header("SillyCheats")]
	public GameObject SillyCheatBlood;

	public float SillyBloodChance = 0.1f;

	[SerializeField]
	[FormerlySerializedAs("SillyShirt")]
	private KingmakerEquipmentEntityReference m_SillyShirt;

	[SerializeField]
	[FormerlySerializedAs("SillyFeedAbility")]
	private BlueprintAbilityReference m_SillyFeedAbility;

	public BlueprintBuff Iddqd => m_Iddqd?.Get();

	public BlueprintBuff Empowered => m_Empowered?.Get();

	public BlueprintUnit Enemy => m_Enemy?.Get();

	public BlueprintUnit PrefabUnit => m_PrefabUnit?.Get();

	public ReferenceArrayProxy<BlueprintBuff> FullBuffList
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] fullBuffList = m_FullBuffList;
			return fullBuffList;
		}
	}

	public KingmakerEquipmentEntity SillyShirt => m_SillyShirt?.Get();

	public BlueprintAbility SillyFeedAbility => m_SillyFeedAbility?.Get();
}

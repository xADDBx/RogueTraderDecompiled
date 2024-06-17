using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

[TypeId("d9dca1c6bd538034ba12f51cee56d95e")]
public class BlueprintPsychicPhenomenaRoot : BlueprintScriptableObject
{
	[Serializable]
	public class PsychicPhenomenaData
	{
		[SerializeField]
		private UnitAsksComponent.Bark m_Bark;

		public bool CheckConditionOnAllPartyMembers;

		[SerializeField]
		public ConditionsChecker ConditionsChecker;

		public int MomentumPenalty;

		public int DefaultMomentumPenalty;

		[SerializeField]
		private GameObject m_OptionalMinorFX;

		public UnitAsksComponent.Bark Bark => m_Bark;

		public GameObject OptionalMinorFX => m_OptionalMinorFX;
	}

	[SerializeField]
	private BlueprintFeatureReference m_SanctionedPsyker;

	[SerializeField]
	private BlueprintFeatureReference m_UnsanctionedPsyker;

	public int MaximumVeilOnAllLocation = 20;

	public int CriticalVeilOnAllLocation = 15;

	public int VeilThicknessPointsToAddForMajor = 3;

	public int VeilThicknessPointsToAddForMinor = 1;

	public float BasePsychicPhenomenaChanceAddition = 5f;

	public float BasePsychicPhenomenaChanceMultiplier = 1f;

	public PsychicPhenomenaData[] PsychicPhenomena;

	public BlueprintAbilityReference[] PerilsOfTheWarpMinor;

	public BlueprintAbilityReference[] PerilsOfTheWarpMajor;

	public BlueprintFeature SanctionedPsyker => m_SanctionedPsyker;

	public BlueprintFeature UnsanctionedPsyker => m_UnsanctionedPsyker;

	public IEnumerable<BlueprintAbilityReference> GetAllPerils()
	{
		BlueprintAbilityReference[] perilsOfTheWarpMajor = PerilsOfTheWarpMajor;
		for (int i = 0; i < perilsOfTheWarpMajor.Length; i++)
		{
			yield return perilsOfTheWarpMajor[i];
		}
		perilsOfTheWarpMajor = PerilsOfTheWarpMinor;
		for (int i = 0; i < perilsOfTheWarpMajor.Length; i++)
		{
			yield return perilsOfTheWarpMajor[i];
		}
	}
}

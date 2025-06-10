using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("d14df58aeeeb46a48e2b448f04eb17a3")]
public class BlueprintWarhammerRoot : BlueprintScriptableObject
{
	[Header("Test")]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaPresetReference m_GamescomPreset;

	[Header("Core")]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_CommonMobFact;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_CommonDestructibleEntityFact;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_CommonSpaceMarineFact;

	[Header("Objects")]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_DefaultMapObjectBlueprint;

	[Header("Roots")]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintCombatRoot.Reference m_CombatRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMomentumRootReference m_MomentumRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPsychicPhenomenaRootReference m_PsychicPhenomenaRoot;

	[SerializeField]
	[ValidateNotNull]
	private UnitConditionBuffsRoot.Reference m_UnitConditionBuffs;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintDestructibleObjectsRoot.Reference m_DestructibleObjectsRoot;

	[SerializeField]
	[ValidateNotNull]
	private SkillCheckRootReference m_SkillCheckRoot;

	[SerializeField]
	[ValidateNotNull]
	private PushSettingRootReference m_PushSettingRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintSystemAnomaliesRoot.Reference m_AnomaliesRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintWarpRoutesSettings.Reference m_WarpRoutesSettings;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintCombatRandomEncountersRoot.Reference m_CombatRandomEncountersRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyRoot.Reference m_ColonyRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintDifficultyRoot.Reference m_DifficultyRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintProfitFactorRoot.Reference m_ProfitFactorRoot;

	[SerializeField]
	[ValidateNotNull]
	private CutscenesRoot.Reference m_CutsceneRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPlanetSettingsRoot.Reference m_BlueprintPlanetSettingsRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintScrapRoot.Reference m_ScrapRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintTraumaRoot.Reference m_TraumaRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintDismembermentRoot.Reference m_DismembermentRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintSoulMarksRoot.Reference m_SoulMarksRoot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintWarpWeatherRoot.Reference m_WarpWeatherRoot;

	[SerializeField]
	private PostProcessingEffectsLibrary.Reference m_PostProcessingEffectsLibrary;

	[SerializeField]
	private LevelUpFxLibrary.Reference m_LevelUpFxLibrary;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintProneRoot.Reference m_ProneRoot;

	[SerializeField]
	[ValidateNotNull]
	private AbilityRedirectRoot.Reference m_AbilityRedirect;

	public static BlueprintWarhammerRoot Instance => BlueprintRoot.Instance.WarhammerRoot;

	public BlueprintAreaPreset GamescomPreset => m_GamescomPreset;

	public BlueprintUnitFact CommonMobFact => m_CommonMobFact;

	public BlueprintMechanicEntityFact CommonDestructibleEntityFact => m_CommonDestructibleEntityFact;

	public BlueprintUnitFactReference CommonSpaceMarineFact => m_CommonSpaceMarineFact;

	public BlueprintMechanicEntityFact DefaultMapObjectBlueprint => m_DefaultMapObjectBlueprint;

	public BlueprintCombatRoot CombatRoot => m_CombatRoot;

	public BlueprintMomentumRoot MomentumRoot => m_MomentumRoot;

	public BlueprintPsychicPhenomenaRoot PsychicPhenomenaRoot => m_PsychicPhenomenaRoot;

	public UnitConditionBuffsRoot UnitConditionBuffs => m_UnitConditionBuffs;

	public BlueprintDestructibleObjectsRoot DestructibleObjectsRoot => m_DestructibleObjectsRoot;

	public SkillCheckRoot SkillCheckRoot => m_SkillCheckRoot;

	public PushSettingRoot PushSettingRoot => m_PushSettingRoot;

	public BlueprintSystemAnomaliesRoot AnomaliesRoot => m_AnomaliesRoot;

	public BlueprintWarpRoutesSettings WarpRoutesSettings => m_WarpRoutesSettings;

	public BlueprintCombatRandomEncountersRoot CombatRandomEncountersRoot => m_CombatRandomEncountersRoot;

	public BlueprintColonyRoot ColonyRoot => m_ColonyRoot.Get();

	public BlueprintDifficultyRoot DifficultyRoot => m_DifficultyRoot;

	public BlueprintProfitFactorRoot ProfitFactorRoot => m_ProfitFactorRoot;

	public CutscenesRoot CutsceneRoot => m_CutsceneRoot;

	public BlueprintPlanetSettingsRoot BlueprintPlanetSettingsRoot => m_BlueprintPlanetSettingsRoot.Get();

	public BlueprintScrapRoot BlueprintScrapRoot => m_ScrapRoot.Get();

	public BlueprintTraumaRoot BlueprintTraumaRoot => m_TraumaRoot.Get();

	public BlueprintDismembermentRoot BlueprintDismembermentRoot => m_DismembermentRoot?.Get();

	public BlueprintSoulMarksRoot SoulMarksRoot => m_SoulMarksRoot?.Get();

	public BlueprintWarpWeatherRoot WarpWeatherRoot => m_WarpWeatherRoot?.Get();

	public PostProcessingEffectsLibrary PostProcessingEffectsLibrary => m_PostProcessingEffectsLibrary?.Get();

	public LevelUpFxLibrary LevelUpFxLibrary => m_LevelUpFxLibrary?.Get();

	public BlueprintProneRoot ProneRoot => m_ProneRoot.Get();

	public AbilityRedirectRoot AbilityRedirect => m_AbilityRedirect;
}

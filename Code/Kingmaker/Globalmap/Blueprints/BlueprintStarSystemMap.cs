using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Localization;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints;

[TypeId("d03eea79843045d18c110d9b79cd5c9f")]
public class BlueprintStarSystemMap : BlueprintArea
{
	[Serializable]
	public class StarAndName
	{
		public BlueprintStar.Reference Star;

		public LocalizedString Name;
	}

	[Serializable]
	public class ConditionToImage
	{
		[JsonProperty]
		public ConditionsReference Conditions;

		[JsonProperty]
		public Sprite Image;
	}

	[Serializable]
	public new class Reference : BlueprintReference<BlueprintStarSystemMap>
	{
	}

	[SerializeField]
	public BlueprintAreaEnterPointReference SpaceCombatArea;

	public StarAndName[] Stars;

	public BlueprintPlanet.Reference[] Planets;

	[SerializeField]
	public List<BlueprintArtificialObject.Reference> OtherObjects;

	[SerializeField]
	public List<BlueprintAnomaly.Reference> Anomalies;

	[SerializeField]
	private List<ConditionToImage> m_SystemScreenshots;

	[Header("SpaceCombatBackgroundComposer")]
	public SpaceCombatBackgroundComposerConfigs BackgroundComposerConfig;

	public int RomeSystemNumber;

	public List<PoiToCondition> PointsForResearchProgress;

	public List<AnomalyToCondition> AnomaliesResearchProgress;

	public override GameModeType AreaStatGameMode => GameModeType.StarSystem;

	public override BlueprintCameraSettings CameraSettings => BlueprintRoot.Instance.CameraRoot.StarSystemMapSettings;

	public override IEnumerable<string> GetActiveSoundBankNames(bool isCurrentPart = false)
	{
		return base.GetActiveSoundBankNames(isCurrentPart).Concat(new string[2] { "SpaceCombat", "AMB_SpaceCombat" });
	}

	public Sprite GetSystemScreenshot()
	{
		return m_SystemScreenshots?.FirstOrDefault((ConditionToImage screenshot) => screenshot?.Conditions?.Get() == null || screenshot.Conditions.Get().Check())?.Image;
	}
}

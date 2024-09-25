using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.SectorMap;

[TypeId("5f359b94cb114b009d53a6b888e5aaa2")]
public class BlueprintWarpRoutesSettings : BlueprintScriptableObject
{
	[Serializable]
	public class DifficultySettings
	{
		public SectorMapPassageEntity.PassageDifficulty Difficulty;

		public int MinDuration;

		public int MaxDuration;

		public float REChance;

		public DialogWeights RandomEncounters;

		public PrefabLink Prefab;

		[Space(10f)]
		public Material WarpRouteMaterial;

		public Vector2 WarpRouteTextureScale;

		public float WarpRouteWidth;
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintWarpRoutesSettings>
	{
	}

	public DifficultySettings[] DifficultySettingsList;

	public int IncreaseScanRadiusCost = 1;

	public int LowerPassageDifficultyCost = 1;

	public int CreateNewPassageCost;

	public int InitialNavigatorResource;

	public int NavigatorResourceGainedForScan;

	public float InitialScanRadius;

	public List<ConditionalRE> UniqueEncounters;
}

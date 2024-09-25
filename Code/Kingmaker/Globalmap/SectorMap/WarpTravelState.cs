using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class WarpTravelState : IHashable
{
	[JsonProperty]
	public bool IsInWarpTravel;

	[JsonProperty]
	public int WarpTravelsCount;

	[JsonProperty]
	public BlueprintSectorMapPoint TravelStart;

	[JsonProperty]
	public BlueprintSectorMapPoint TravelDestination;

	[JsonProperty]
	public int AnomaliesCount;

	[JsonProperty]
	public readonly CountableFlag ForbidRE = new CountableFlag();

	[JsonProperty]
	public int NavigatorResource;

	[JsonProperty]
	public float ScanRadius;

	[JsonProperty]
	public bool IsInitialized;

	[JsonProperty]
	public List<BlueprintDialog> UniqueRE = new List<BlueprintDialog>();

	[JsonProperty(PropertyName = "m_EtudesInWarpQueue")]
	public List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>> EtudesInWarpQueue = new List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>>();

	[JsonProperty(PropertyName = "m_TriggeredEtude")]
	public BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> TriggeredEtude;

	[JsonProperty]
	public List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>> TriggeredEtudeInMiddleOfJump = new List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>>();

	[JsonProperty]
	public bool AllRoutesNotDeadlyFlag;

	[JsonProperty]
	public bool AllRoutesNotDeadlyChanged;

	[JsonProperty]
	public int CreateNewPassageCost;

	public void Init()
	{
		if (!IsInitialized)
		{
			BlueprintWarpRoutesSettings warpRoutesSettings = BlueprintWarhammerRoot.Instance.WarpRoutesSettings;
			ScanRadius = warpRoutesSettings.InitialScanRadius;
			NavigatorResource = warpRoutesSettings.InitialNavigatorResource;
			UniqueRE = (from re in warpRoutesSettings.UniqueEncounters.EmptyIfNull()
				select re.RandomEncounter).ToList();
			CreateNewPassageCost = warpRoutesSettings.CreateNewPassageCost;
			IsInitialized = true;
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref IsInWarpTravel);
		result.Append(ref WarpTravelsCount);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(TravelStart);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(TravelDestination);
		result.Append(ref val2);
		result.Append(ref AnomaliesCount);
		Hash128 val3 = ClassHasher<CountableFlag>.GetHash128(ForbidRE);
		result.Append(ref val3);
		result.Append(ref NavigatorResource);
		result.Append(ref ScanRadius);
		result.Append(ref IsInitialized);
		List<BlueprintDialog> uniqueRE = UniqueRE;
		if (uniqueRE != null)
		{
			for (int i = 0; i < uniqueRE.Count; i++)
			{
				Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(uniqueRE[i]);
				result.Append(ref val4);
			}
		}
		List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>> etudesInWarpQueue = EtudesInWarpQueue;
		if (etudesInWarpQueue != null)
		{
			for (int j = 0; j < etudesInWarpQueue.Count; j++)
			{
				BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> obj = etudesInWarpQueue[j];
				Hash128 val5 = StructHasher<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>>.GetHash128(ref obj);
				result.Append(ref val5);
			}
		}
		BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> obj2 = TriggeredEtude;
		Hash128 val6 = StructHasher<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>>.GetHash128(ref obj2);
		result.Append(ref val6);
		List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>> triggeredEtudeInMiddleOfJump = TriggeredEtudeInMiddleOfJump;
		if (triggeredEtudeInMiddleOfJump != null)
		{
			for (int k = 0; k < triggeredEtudeInMiddleOfJump.Count; k++)
			{
				BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> obj3 = triggeredEtudeInMiddleOfJump[k];
				Hash128 val7 = StructHasher<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>>.GetHash128(ref obj3);
				result.Append(ref val7);
			}
		}
		result.Append(ref AllRoutesNotDeadlyFlag);
		result.Append(ref AllRoutesNotDeadlyChanged);
		result.Append(ref CreateNewPassageCost);
		return result;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class SectorMapPassageEntity : Entity, ISectorMapPassageEntity, IEntity, IDisposable, IHashable
{
	public enum PassageDifficulty
	{
		Safe,
		Unsafe,
		Dangerous,
		Deadly
	}

	public enum ExploreStatus
	{
		UnExplored,
		ExploredFromOneSide,
		Explored
	}

	[JsonProperty]
	public int DurationInDays;

	[JsonProperty]
	public float EncounterChance;

	[JsonProperty]
	public PassageDifficulty CurrentDifficulty;

	[JsonProperty]
	public ExploreStatus CurrentExploreStatus;

	[JsonProperty]
	public BlueprintSectorMapPoint SectorMapScanFrom;

	[JsonProperty]
	public BlueprintSectorMapPoint StarSystem1Blueprint;

	[JsonProperty]
	public BlueprintSectorMapPoint StarSystem2Blueprint;

	[JsonProperty]
	public List<Vector3> CurvedLinePoints = new List<Vector3>();

	public bool IsExplored => CurrentExploreStatus == ExploreStatus.Explored;

	public new SectorMapPassageView View => (SectorMapPassageView)base.View;

	public void Explore()
	{
		if (View.ShouldBeExploredFromBothSystems)
		{
			if (SectorMapScanFrom != null && SectorMapScanFrom != Game.Instance.SectorMapController.CurrentStarSystem.Blueprint)
			{
				CurrentExploreStatus = ExploreStatus.Explored;
			}
			else if (!IsExplored)
			{
				CurrentExploreStatus = ExploreStatus.ExploredFromOneSide;
			}
		}
		else
		{
			CurrentExploreStatus = ExploreStatus.Explored;
		}
		SectorMapScanFrom = Game.Instance.SectorMapController.CurrentStarSystem.Blueprint;
		if (CurrentExploreStatus == ExploreStatus.Explored)
		{
			View.UpdateVisibility();
		}
	}

	public SectorMapPassageEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	public SectorMapPassageEntity(SectorMapPassageView view)
		: this(view.UniqueId, view.IsInGameBySettings)
	{
		CurrentExploreStatus = (view.IsExploredOnStart ? ExploreStatus.Explored : ExploreStatus.UnExplored);
		CurrentDifficulty = view.Difficulty;
		CurvedLinePoints = (from point in view.GetComponentsInChildren<CurvedLinePoint>().EmptyIfNull()
			select point.transform.position).ToList();
		GenerateParameters();
	}

	public SectorMapPassageEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return UnityEngine.Object.Instantiate(BlueprintWarhammerRoot.Instance.WarpRoutesSettings.DifficultySettingsList.FirstOrDefault((BlueprintWarpRoutesSettings.DifficultySettings difficultySettings) => difficultySettings.Difficulty == CurrentDifficulty)?.Prefab.Load(), Vector3.zero, Quaternion.identity).GetComponent<SectorMapPassageView>();
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		View.LoadPassageVisualPoints();
	}

	public void LowerDifficulty(PassageDifficulty difficulty)
	{
		if (CurrentDifficulty != 0)
		{
			CurrentDifficulty = difficulty;
			GenerateParameters();
			View.ChangeVisual(CurrentDifficulty);
		}
	}

	private void GenerateParameters()
	{
		PassagesGenerator.GeneratePassageParametersByDifficulty(this, PersistentRandom.Seed(Game.Instance.Player.GameId.GetHashCode()).MakeGenerator("generate_passage_parameters_by_difficulty_" + base.UniqueId + "_" + CurrentDifficulty));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref DurationInDays);
		result.Append(ref EncounterChance);
		result.Append(ref CurrentDifficulty);
		result.Append(ref CurrentExploreStatus);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(SectorMapScanFrom);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(StarSystem1Blueprint);
		result.Append(ref val3);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(StarSystem2Blueprint);
		result.Append(ref val4);
		List<Vector3> curvedLinePoints = CurvedLinePoints;
		if (curvedLinePoints != null)
		{
			for (int i = 0; i < curvedLinePoints.Count; i++)
			{
				Vector3 obj = curvedLinePoints[i];
				Hash128 val5 = UnmanagedHasher<Vector3>.GetHash128(ref obj);
				result.Append(ref val5);
			}
		}
		return result;
	}
}

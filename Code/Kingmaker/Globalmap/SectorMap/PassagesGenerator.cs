using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.SectorMap;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class PassagesGenerator
{
	public static BlueprintDialog GenerateRandomEncounter(SectorMapPassageEntity passage)
	{
		foreach (ConditionalRE uniqueEncounter in BlueprintWarhammerRoot.Instance.WarpRoutesSettings.UniqueEncounters)
		{
			if (uniqueEncounter.Difficulties.Contains(passage.CurrentDifficulty) && Game.Instance.Player.WarpTravelState.UniqueRE.Contains(uniqueEncounter.RandomEncounter) && uniqueEncounter.Conditions.Check())
			{
				return uniqueEncounter.RandomEncounter;
			}
		}
		PersistentRandom.Generator generator = PersistentRandom.Seed(Game.Instance.Player.GameId.GetHashCode()).MakeGenerator("warp_travel_RE").Branch("random_encounter_type", Game.Instance.Player.WarpTravelState.WarpTravelsCount);
		return Game.Instance.Player.GlobalMapRandomGenerationState.GetRandomEncountersDialogs(passage.CurrentDifficulty)?.GetRandomObject(generator)?.Get();
	}

	public static bool ShouldProceedRE(SectorMapPassageEntity passage)
	{
		float num = PersistentRandom.Seed(Game.Instance.Player.GameId.GetHashCode()).MakeGenerator("warp_travel_RE").Branch("RE_chance", Game.Instance.Player.WarpTravelState.WarpTravelsCount)
			.NextRangeFloat(0f, 100f, 2);
		PFLog.Default.Log($"Random value: {num}, EncounterChance: {passage.EncounterChance}");
		return num <= passage.EncounterChance;
	}

	public static void GeneratePassagesParameters(List<SectorMapPassageEntity> passages)
	{
		PersistentRandom.Generator generator = PersistentRandom.Seed(Game.Instance.Player.GameId.GetHashCode()).MakeGenerator("generate_passage_parameters_by_difficulty");
		foreach (SectorMapPassageEntity passage in passages)
		{
			GeneratePassageParametersByDifficulty(passage, generator);
		}
	}

	public static void GeneratePassageParametersByDifficulty(SectorMapPassageEntity passage, PersistentRandom.Generator generator)
	{
		BlueprintWarpRoutesSettings.DifficultySettings difficultySettings = BlueprintWarhammerRoot.Instance.WarpRoutesSettings.DifficultySettingsList.FirstOrDefault((BlueprintWarpRoutesSettings.DifficultySettings setting) => setting.Difficulty == passage.CurrentDifficulty);
		if (difficultySettings != null)
		{
			passage.DurationInDays = generator.NextRange(difficultySettings.MinDuration, difficultySettings.MaxDuration + 1);
			passage.EncounterChance = difficultySettings.REChance;
		}
	}

	public static SectorMapPassageEntity GeneratePassage(SectorMapObject from, SectorMapObject to, SectorMapPassageEntity.PassageDifficulty Difficulty)
	{
		if (Game.Instance.SectorMapController.FindPassageBetween(from.Data, to.Data) != null)
		{
			return null;
		}
		Vector3 position = from.ViewTransform.position + (to.ViewTransform.position - from.ViewTransform.position) / 2f;
		SectorMapPassageView component = Object.Instantiate(BlueprintWarhammerRoot.Instance.WarpRoutesSettings.DifficultySettingsList.FirstOrDefault((BlueprintWarpRoutesSettings.DifficultySettings difficultySettings) => difficultySettings.Difficulty == Difficulty)?.Prefab.Load(), position, Quaternion.identity).GetComponent<SectorMapPassageView>();
		component.UniqueId = Uuid.Instance.CreateString();
		SectorMapPassageEntity sectorMapPassageEntity = (SectorMapPassageEntity)component.CreateEntityData(load: false);
		sectorMapPassageEntity.CurrentExploreStatus = SectorMapPassageEntity.ExploreStatus.UnExplored;
		sectorMapPassageEntity.CurrentDifficulty = Difficulty;
		component.StarSystem1 = from;
		component.StarSystem2 = to;
		sectorMapPassageEntity.StarSystem1Blueprint = from.Blueprint;
		sectorMapPassageEntity.StarSystem2Blueprint = to.Blueprint;
		component.gameObject.name = "PS: " + from.name + " <--> " + to.name;
		sectorMapPassageEntity.AttachView(component);
		Game.Instance.EntitySpawner.SpawnEntity(sectorMapPassageEntity, null);
		GeneratePassageVisualPoints(sectorMapPassageEntity);
		return sectorMapPassageEntity;
	}

	private static float GetDeviationOfBezierPoint(SectorMapPassageEntity passage)
	{
		return passage.CurrentDifficulty switch
		{
			SectorMapPassageEntity.PassageDifficulty.Safe => SectorMapView.Instance.bezierPointsCurveRangeSafe, 
			SectorMapPassageEntity.PassageDifficulty.Unsafe => SectorMapView.Instance.bezierPointsCurveRangeUnsafe, 
			SectorMapPassageEntity.PassageDifficulty.Dangerous => SectorMapView.Instance.bezierPointsCurveRangeDangerous, 
			SectorMapPassageEntity.PassageDifficulty.Deadly => SectorMapView.Instance.bezierPointsCurveRangeDeadly, 
			_ => SectorMapView.Instance.bezierPointsCurveRangeSafe, 
		};
	}

	private static void GeneratePassageVisualPoints(SectorMapPassageEntity passage)
	{
		int num = Mathf.RoundToInt(Vector3.Distance(passage.View.StarSystem1.ViewTransform.position, passage.View.StarSystem2.ViewTransform.position) / SectorMapView.Instance.distanceBetweenBezierPoints);
		CurvedLineRenderer component = passage.View.GetComponent<CurvedLineRenderer>();
		passage.View.ViewTransform.rotation = Quaternion.AngleAxis(90f, new Vector3(1f, 0f, 0f));
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = passage.View.name.Replace("(Clone)", "") + "_BezierPoint_" + i;
			Vector3 vector = default(Vector3);
			gameObject.AddComponent<CurvedLinePoint>();
			if (i == 0)
			{
				vector = passage.View.StarSystem1.ViewTransform.position;
			}
			else if (i == num - 1)
			{
				vector = passage.View.StarSystem2.ViewTransform.position;
			}
			else
			{
				vector = Vector3.MoveTowards(passage.View.StarSystem1.ViewTransform.position, passage.View.StarSystem2.ViewTransform.position, (float)i * SectorMapView.Instance.distanceBetweenBezierPoints);
				float deviationOfBezierPoint = GetDeviationOfBezierPoint(passage);
				vector = new Vector3(vector.x + PFStatefulRandom.GlobalMap.Range(0f - deviationOfBezierPoint, deviationOfBezierPoint), vector.y, vector.z + PFStatefulRandom.GlobalMap.Range(0f - deviationOfBezierPoint, deviationOfBezierPoint));
			}
			gameObject.transform.position = vector;
			passage.View.Data.CurvedLinePoints.Add(gameObject.transform.position);
			gameObject.transform.parent = passage.View.ViewTransform;
			component.ManualUpdate();
		}
		component.ManualUpdate();
	}
}

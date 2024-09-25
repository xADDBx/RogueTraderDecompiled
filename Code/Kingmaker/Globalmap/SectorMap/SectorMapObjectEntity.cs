using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class SectorMapObjectEntity : MechanicEntity<BlueprintSectorMapPoint>, ISectorMapObjectEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	[JsonProperty]
	public StarSystemData StarSystemParameters;

	[JsonProperty]
	public bool IsExplored;

	[JsonProperty]
	public bool IsVisited;

	[JsonProperty]
	public bool IsScannedFrom;

	[JsonProperty]
	public bool IsAvailable = true;

	[JsonProperty]
	public bool IsHidden;

	public new SectorMapObject View => (SectorMapObject)base.View;

	public BlueprintStarSystemMap StarSystemArea => (base.Blueprint as BlueprintSectorMapPointStarSystem)?.StarSystemToTransit.Get() as BlueprintStarSystemMap;

	public List<BlueprintPlanet> Planets => StarSystemArea?.Planets.Dereference().ToList();

	public List<BlueprintStar> Stars => StarSystemArea?.Stars.Select((BlueprintStarSystemMap.StarAndName star) => star.Star.Get()).ToList();

	public List<BlueprintArtificialObject> OtherObjects => StarSystemArea?.OtherObjects.Dereference().ToList();

	public Sprite StarSystemSprite => StarSystemArea?.GetSystemScreenshot();

	public List<BlueprintAnomaly> Anomalies => StarSystemArea?.AnomaliesResearchProgress?.Select((AnomalyToCondition a) => a.Anomaly.Get()).ToList();

	public List<BlueprintAnomaly> AnomaliesForGlobalMap => StarSystemArea?.Anomalies?.Dereference()?.Where((BlueprintAnomaly anomaly) => anomaly.ShowOnGlobalMap).EmptyIfNull().ToList();

	protected SectorMapObjectEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public SectorMapObjectEntity(string uniqueId, bool isInGame, BlueprintSectorMapPoint blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public SectorMapObjectEntity(SectorMapObject view)
		: this(view.UniqueId, view.IsInGameBySettings, view.Blueprint)
	{
		if (view.IsExploredOnStart || view.IsVisitedOnStart)
		{
			IsExplored = true;
		}
		if (view.IsVisitedOnStart)
		{
			IsVisited = true;
		}
		if (view.Hidden)
		{
			IsHidden = true;
		}
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		if (IsHidden)
		{
			base.IsInGame = false;
		}
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public void Explore()
	{
		if (!IsExplored)
		{
			IsExplored = true;
			View.SetExplored();
		}
	}

	public void Visit()
	{
		IsVisited = true;
		View.SetVisited();
	}

	public void SetDecalVisible(bool state)
	{
		View.SetDecalVisibility(state);
	}

	public void SetDecalColor(SectorMapPassageEntity.PassageDifficulty dif)
	{
		View.SetDecalColor(dif);
	}

	public void SetConsoleFocusState(bool state)
	{
		if (!Game.Instance.IsControllerMouse)
		{
			View.SetConsoleFocusState(state);
		}
	}

	public void SetAvailability(bool availability)
	{
		IsAvailable = availability;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<StarSystemData>.GetHash128(StarSystemParameters);
		result.Append(ref val2);
		result.Append(ref IsExplored);
		result.Append(ref IsVisited);
		result.Append(ref IsScannedFrom);
		result.Append(ref IsAvailable);
		result.Append(ref IsHidden);
		return result;
	}
}

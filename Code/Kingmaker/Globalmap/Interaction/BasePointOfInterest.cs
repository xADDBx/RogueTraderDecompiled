using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class BasePointOfInterest : IStarSystemInteraction, IHashable
{
	public enum ExplorationStatus
	{
		NotExplored,
		NeedInteraction,
		Explored
	}

	[JsonProperty]
	public BlueprintPointOfInterest Blueprint;

	[JsonProperty]
	public ExplorationStatus Status { get; set; }

	public BasePointOfInterest(BlueprintPointOfInterest blueprint)
	{
		Blueprint = blueprint;
		Status = ExplorationStatus.NotExplored;
	}

	public BasePointOfInterest(JsonConstructorMark _)
	{
	}

	public void Scan()
	{
		Status = ExplorationStatus.NeedInteraction;
	}

	public void ChangeStatusToInteracted()
	{
		SetInteracted();
		EventBus.RaiseEvent(delegate(IExplorationHandler h)
		{
			h.HandlePointOfInterestInteracted(this);
		});
	}

	public void SetInteracted()
	{
		if (Status != ExplorationStatus.Explored)
		{
			Status = ExplorationStatus.Explored;
			BlueprintStarSystemMap key = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
			Dictionary<BlueprintStarSystemMap, List<BlueprintPointOfInterest>> interactedPoints = Game.Instance.Player.StarSystemsState.InteractedPoints;
			if (interactedPoints.TryGetValue(key, out var value))
			{
				value.Add(Blueprint);
			}
			else
			{
				interactedPoints.Add(key, new List<BlueprintPointOfInterest> { Blueprint });
			}
			Blueprint.OnInteractedActions?.Run();
		}
	}

	public virtual bool IsVisible()
	{
		return Blueprint?.IsVisible() ?? true;
	}

	public virtual bool IsInteractable()
	{
		return Blueprint?.IsInteractable() ?? true;
	}

	public virtual void Interact(StarSystemObjectEntity entity)
	{
	}

	public static BasePointOfInterest CreatePointOfInterest(BlueprintPointOfInterest blueprint)
	{
		if (!(blueprint is BlueprintPointOfInterestBookEvent blueprint2))
		{
			if (!(blueprint is BlueprintPointOfInterestCargo blueprint3))
			{
				if (!(blueprint is BlueprintPointOfInterestColonyTrait blueprint4))
				{
					if (!(blueprint is BlueprintPointOfInterestExpedition blueprint5))
					{
						if (!(blueprint is BlueprintPointOfInterestGroundOperation blueprint6))
						{
							if (!(blueprint is BlueprintPointOfInterestLoot blueprint7))
							{
								if (!(blueprint is BlueprintPointOfInterestResources blueprint8))
								{
									if (blueprint is BlueprintPointOfInterestStatCheckLoot blueprint9)
									{
										return new PointOfInterestStatCheckLoot(blueprint9);
									}
									return new BasePointOfInterest(blueprint);
								}
								return new PointOfInterestResources(blueprint8);
							}
							return new PointOfInterestLoot(blueprint7);
						}
						return new PointOfInterestGroundOperation(blueprint6);
					}
					return new PointOfInterestExpedition(blueprint5);
				}
				return new PointOfInterestColonyTrait(blueprint4);
			}
			return new PointOfInterestCargo(blueprint3);
		}
		return new PointOfInterestBookEvent(blueprint2);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		ExplorationStatus val2 = Status;
		result.Append(ref val2);
		return result;
	}
}

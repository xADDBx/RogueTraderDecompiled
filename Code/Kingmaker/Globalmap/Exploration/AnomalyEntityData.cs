using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

public class AnomalyEntityData : StarSystemObjectEntity, IHashable
{
	[JsonProperty]
	public new MainAnomalyFact MainFact;

	[JsonProperty]
	public bool IsInteracted;

	[JsonProperty]
	public bool IsInteractedAtLeastOnce;

	private AnomalyInteraction m_CurrentInteraction;

	private StarSystemContextData m_ContextForInteraction;

	[JsonProperty]
	public bool IsMoving;

	[JsonProperty(IsReference = false)]
	public Vector3 Destination;

	[JsonProperty]
	public bool RemoveAfterMove;

	public float Speed;

	public new BlueprintAnomaly Blueprint => (BlueprintAnomaly)base.Blueprint;

	public BlueprintAnomaly.AnomalyInteractTime InteractTime => Blueprint.InteractTime;

	public float InteractDistance
	{
		get
		{
			if (InteractTime != BlueprintAnomaly.AnomalyInteractTime.OnDistance)
			{
				if (InteractTime != 0)
				{
					return float.MaxValue;
				}
				return 1f;
			}
			return Blueprint.InteractDistance;
		}
	}

	public new AnomalyView View => (AnomalyView)base.View;

	public AnomalyEntityData(JsonConstructorMark _)
		: base(_)
	{
	}

	public AnomalyEntityData(AnomalyView view, BlueprintAnomaly blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
		Game.Instance.Player.WarpTravelState.AnomaliesCount++;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		MainFact = Facts.Add(new MainAnomalyFact(Blueprint));
		Speed = Blueprint.GetComponent<AnomalyStarShip>()?.Speed ?? 1f;
	}

	public bool CanInteract()
	{
		if (!IsInteracted)
		{
			return Blueprint.GetComponent<AnomalyInteraction>() != null;
		}
		return false;
	}

	public void Interact()
	{
		if (!IsInteracted)
		{
			AnomalyInteraction component = Blueprint.GetComponent<AnomalyInteraction>();
			m_ContextForInteraction?.Dispose();
			m_CurrentInteraction = component;
			m_ContextForInteraction = ContextData<StarSystemContextData>.Request().Setup(this);
			component?.Interact();
		}
	}

	public void OnInteractionEnded()
	{
		if (!IsInteracted)
		{
			m_CurrentInteraction = null;
			IsInteractedAtLeastOnce = true;
			Dictionary<BlueprintStarSystemMap, List<BlueprintAnomaly>> interactedAnomalies = Game.Instance.Player.StarSystemsState.InteractedAnomalies;
			BlueprintStarSystemMap key = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
			if (interactedAnomalies.TryGetValue(key, out var value))
			{
				value.Add(Blueprint);
			}
			else
			{
				interactedAnomalies.Add(key, new List<BlueprintAnomaly> { Blueprint });
			}
			if (!Blueprint.InfiniteInteraction)
			{
				IsInteracted = true;
			}
			EventBus.RaiseEvent(this, delegate(IAnomalyHandler e)
			{
				e.HandleAnomalyInteracted();
			});
			if (IsInteracted && Blueprint.RemoveAfterInteraction)
			{
				base.IsInGame = false;
			}
		}
	}

	public void Move(Vector3 destination, bool removeAfterMove)
	{
		IsMoving = true;
		Destination = destination;
		RemoveAfterMove = removeAfterMove;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		m_ContextForInteraction?.Dispose();
	}

	public void SetNonInteractable()
	{
		IsInteractedAtLeastOnce = true;
		IsInteracted = true;
		if (Blueprint.RemoveAfterInteraction)
		{
			base.IsInGame = false;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MainAnomalyFact>.GetHash128(MainFact);
		result.Append(ref val2);
		result.Append(ref IsInteracted);
		result.Append(ref IsInteractedAtLeastOnce);
		result.Append(ref IsMoving);
		result.Append(ref Destination);
		result.Append(ref RemoveAfterMove);
		return result;
	}
}

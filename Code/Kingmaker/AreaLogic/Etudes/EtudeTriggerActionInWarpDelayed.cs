using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("d7c3d80beb354d44ab46ace48fc5bf75")]
public class EtudeTriggerActionInWarpDelayed : EtudeBracketTrigger, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, ISectorMapWarpTravelEventHandler, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int WarpTravelStartCount;

		[JsonProperty]
		public bool IsCompleted;

		[JsonProperty]
		public bool IsReadyToTrigger;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref WarpTravelStartCount);
			result.Append(ref IsCompleted);
			result.Append(ref IsReadyToTrigger);
			return result;
		}
	}

	public enum TimeToStartAction
	{
		AfterTravelStart,
		AfterTravelFinished
	}

	public enum EventType
	{
		OncePerTravel,
		MechanicEvent,
		SimpleEvent
	}

	[Tooltip("How much warp travels should pass for ActionList to be invoked")]
	public int WarpTravelTriggerCount;

	[Tooltip("Actions to invoke after required amount of warp travel has passed")]
	public ActionList ActionList;

	[SerializeField]
	public EventType TriggerType;

	[Tooltip("When to invoke actions relatively to last warp travel")]
	[ShowIf("IsOncePerTravel")]
	public TimeToStartAction TimeToStart = TimeToStartAction.AfterTravelFinished;

	[SerializeField]
	[ShowIf("IsOncePerTravel")]
	[Tooltip("The greater number - the higher priority. Priority of deadly encounters is 20")]
	public int Priority = 1;

	private bool IsOncePerTravel => TriggerType == EventType.OncePerTravel;

	public void HandleWarpTravelBeforeStart()
	{
		List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>> etudesInWarpQueue = Game.Instance.Player.WarpTravelState.EtudesInWarpQueue;
		SavableData savableData = RequestSavableData<SavableData>();
		if ((savableData.IsCompleted || EtudeBracketTrigger.Etude.IsCompleted) && etudesInWarpQueue.Contains(this))
		{
			etudesInWarpQueue.Remove(this);
		}
		else if (!savableData.IsReadyToTrigger && IsReadyToTrigger())
		{
			savableData.IsReadyToTrigger = true;
			if (IsOncePerTravel && !etudesInWarpQueue.Contains(this))
			{
				etudesInWarpQueue.Add(this);
			}
		}
		List<BlueprintComponentReference<EtudeTriggerActionInWarpDelayed>> triggeredEtudeInMiddleOfJump = Game.Instance.Player.WarpTravelState.TriggeredEtudeInMiddleOfJump;
		if (savableData.IsReadyToTrigger && !savableData.IsCompleted && TriggerType == EventType.SimpleEvent && !triggeredEtudeInMiddleOfJump.Contains(this))
		{
			triggeredEtudeInMiddleOfJump.Add(this);
		}
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (TriggerType == EventType.MechanicEvent && savableData.IsReadyToTrigger)
		{
			Game.Instance.SectorMapTravelController.PauseTravel();
			ActionList.Run();
			Game.Instance.SectorMapTravelController.UnpauseManual();
			Complete();
		}
	}

	public void HandleWarpTravelStopped()
	{
		RequestSavableData<SavableData>();
		if (TimeToStart == TimeToStartAction.AfterTravelFinished && Game.Instance.Player.WarpTravelState.TriggeredEtude == this)
		{
			ActionList.Run();
			Complete();
		}
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}

	protected override void OnEnter()
	{
		RequestSavableData<SavableData>().WarpTravelStartCount = Game.Instance.Player.WarpTravelState.WarpTravelsCount;
	}

	protected override void OnExit()
	{
		if (Game.Instance.Player.WarpTravelState.EtudesInWarpQueue.Contains(this))
		{
			Game.Instance.Player.WarpTravelState.EtudesInWarpQueue.Remove(this);
		}
		RequestSavableData<SavableData>().IsReadyToTrigger = false;
	}

	protected override void OnResume()
	{
	}

	private bool IsReadyToTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (!savableData.IsCompleted && !EtudeBracketTrigger.Etude.IsCompleted)
		{
			return savableData.WarpTravelStartCount + WarpTravelTriggerCount <= Game.Instance.Player.WarpTravelState.WarpTravelsCount;
		}
		return false;
	}

	private void Complete()
	{
		RequestSavableData<SavableData>().IsCompleted = true;
		if (TriggerType == EventType.OncePerTravel)
		{
			Game.Instance.Player.WarpTravelState.EtudesInWarpQueue.Remove(this);
			Game.Instance.Player.WarpTravelState.TriggeredEtude = null;
		}
	}

	public void HandleStartEventInTheMiddleOfJump(BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> etudeTrigger)
	{
		if (etudeTrigger == this)
		{
			Game.Instance.SectorMapTravelController.PauseTravel();
			ActionList.Run();
			Game.Instance.SectorMapTravelController.UnpauseManual();
			Complete();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

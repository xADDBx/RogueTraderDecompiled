using System;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Assets.Controllers.GlobalMap;

public class SectorMapTravelController : IControllerEnable, IController, IControllerDisable, IControllerTick
{
	private TimeSpan m_TravelStartedTime;

	private int m_TravelDuration;

	private bool m_IsTravelling;

	private bool m_WaitingForTravelToStart;

	private TimeSpan m_TravelWaitingStartedTime;

	private TimeSpan m_TravelPaused;

	private TimeSpan m_TravelResumed;

	private TimeSpan m_TravelStopped;

	private bool m_WaitingForEndAnimationToStop;

	private SectorMapObjectEntity m_From;

	private SectorMapObjectEntity m_To;

	private SectorMapPassageEntity m_Passage;

	private bool m_ShouldProceedRE;

	private bool m_REProceeded;

	private BlueprintDialog m_RandomEncounter;

	private bool m_StartedBookEvent;

	private int m_EtudeTriggerIndex;

	private bool m_EtudesTriggerListEnded = true;

	private TimeSpan m_LastEventEnded;

	public SectorMapObjectEntity From => m_From;

	public SectorMapObjectEntity To => m_To;

	private static TimeSpan CurrentTime => Game.Instance.TimeController.RealTime;

	public bool IsTravelling => Game.Instance.Player.WarpTravelState.IsInWarpTravel;

	public void OnEnable()
	{
		m_IsTravelling = Game.Instance.Player.WarpTravelState.IsInWarpTravel && !Game.Instance.Player.CombatRandomEncounterState.IsInCombatRandomEncounter;
		m_From = Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity point) => point.Blueprint == Game.Instance.Player.WarpTravelState.TravelStart);
		m_To = Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity point) => point.Blueprint == Game.Instance.Player.WarpTravelState.TravelDestination);
		if (m_IsTravelling)
		{
			ResumeTravel();
		}
		m_ShouldProceedRE = false;
		m_REProceeded = true;
	}

	public void OnDisable()
	{
		if (m_IsTravelling)
		{
			PauseTravel();
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (m_WaitingForTravelToStart && (CurrentTime - m_TravelWaitingStartedTime).TotalSeconds > (double)Game.Instance.SectorMapController.VisualParameters.WaitWarpTravelToStartInSeconds)
		{
			StartTravelAfterAnimation();
		}
		else if (m_IsTravelling && m_EtudesTriggerListEnded && (Game.Instance.TimeController.GameTime - m_TravelResumed + m_TravelPaused - m_TravelStartedTime).TotalSegments() > m_TravelDuration)
		{
			BlueprintSectorMapPointStarSystem blueprintSectorMapPointStarSystem = m_To.View.Blueprint as BlueprintSectorMapPointStarSystem;
			BlueprintDialog blueprintDialog = blueprintSectorMapPointStarSystem?.OverrideBookEvent?.Get();
			if (blueprintDialog != null)
			{
				ConditionsChecker bookEventConditions = blueprintSectorMapPointStarSystem.BookEventConditions;
				if ((bookEventConditions == null || bookEventConditions.Check()) && !m_StartedBookEvent)
				{
					DialogData data = DialogController.SetupDialogWithoutTarget(blueprintDialog, null);
					Game.Instance.DialogController.StartDialog(data);
					m_StartedBookEvent = true;
					return;
				}
			}
			if (m_To.View.StarSystemToTransit == null)
			{
				m_To = m_From;
				if (m_To == null)
				{
					m_To = Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity point) => point.View.Blueprint == BlueprintRoot.Instance.SectorMapArea.DefaultStarSystem.Get());
				}
				Game.Instance.Player.WarpTravelState.TravelDestination = m_To.Blueprint;
				m_StartedBookEvent = false;
			}
			else
			{
				StopTravel();
			}
		}
		else if (m_IsTravelling && m_ShouldProceedRE && !m_REProceeded && (float)(Game.Instance.TimeController.GameTime - m_TravelResumed + m_TravelPaused - m_TravelStartedTime).TotalSegments() > (float)m_TravelDuration / 2f)
		{
			ProceedRandomEncounter();
		}
		else if (m_IsTravelling && !m_EtudesTriggerListEnded && ((m_ShouldProceedRE && m_REProceeded) || !m_ShouldProceedRE) && (float)(Game.Instance.TimeController.GameTime - m_TravelResumed + m_TravelPaused - m_TravelStartedTime).TotalSegments() > (float)m_TravelDuration / 2f && (CurrentTime - m_LastEventEnded).TotalSeconds > (double)Game.Instance.SectorMapController.VisualParameters.WaitBetweenEtudeEventsInSeconds)
		{
			BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> trigger = Game.Instance.Player.WarpTravelState.TriggeredEtudeInMiddleOfJump[m_EtudeTriggerIndex];
			EventBus.RaiseEvent(delegate(ISectorMapWarpTravelEventHandler h)
			{
				h.HandleStartEventInTheMiddleOfJump(trigger);
			});
			m_EtudeTriggerIndex++;
			m_LastEventEnded = CurrentTime;
			if (m_EtudeTriggerIndex >= Game.Instance.Player.WarpTravelState.TriggeredEtudeInMiddleOfJump.Count)
			{
				m_EtudesTriggerListEnded = true;
			}
		}
		else if (m_WaitingForEndAnimationToStop && (CurrentTime - m_TravelStopped).TotalSeconds > (double)Game.Instance.SectorMapController.VisualParameters.WaitWarpTravelToEndInSeconds)
		{
			m_WaitingForEndAnimationToStop = false;
			HandleWarpTravelEffectStopped();
		}
	}

	public void WarpTravel(SectorMapObjectEntity from, SectorMapObjectEntity to)
	{
		Game.Instance.GameCommandQueue.StartWarpTravel(from, to);
	}

	public void StartWarpTravel(SectorMapObjectEntity from, SectorMapObjectEntity to)
	{
		ShouldProceedEvent(from, to, out m_ShouldProceedRE, out m_RandomEncounter);
		SectorMapPassageEntity sectorMapPassageEntity = Game.Instance.SectorMapController.FindPassageBetween(from, to);
		RandomWeightsForSave<BlueprintDialogReference> randomEncountersDialogs = Game.Instance.Player.GlobalMapRandomGenerationState.GetRandomEncountersDialogs(sectorMapPassageEntity.CurrentDifficulty);
		EventBus.RaiseEvent((ISectorMapObjectEntity)from, (Action<ISectorMapWarpTravelHandler>)delegate(ISectorMapWarpTravelHandler h)
		{
			h.HandleWarpTravelBeforeStart();
		}, isCheckRuntime: true);
		SetupWarpTravel(from, to);
		SetupEncounter(m_RandomEncounter, randomEncountersDialogs);
		BlueprintSectorMapPointStarSystem blueprintSectorMapPointStarSystem = m_To.Blueprint as BlueprintSectorMapPointStarSystem;
		ConditionsChecker conditionsChecker = blueprintSectorMapPointStarSystem?.ConditionsToVisitAutomatically;
		if (conditionsChecker != null && conditionsChecker.HasConditions && conditionsChecker.Check() && blueprintSectorMapPointStarSystem.StarSystemToTransit != null)
		{
			Game.Instance.Player.StarSystemsState.StarSystemsToVisit.Add(blueprintSectorMapPointStarSystem.StarSystemToTransit?.Get() as BlueprintStarSystemMap);
		}
	}

	private void StartTravelAfterAnimation()
	{
		m_TravelStartedTime = Game.Instance.TimeController.GameTime;
		m_TravelPaused = m_TravelStartedTime;
		m_TravelResumed = m_TravelStartedTime;
		m_WaitingForTravelToStart = false;
		m_IsTravelling = true;
		if (Game.Instance.Player.WarpTravelState.TriggeredEtudeInMiddleOfJump.Empty())
		{
			m_EtudesTriggerListEnded = true;
		}
		EventBus.RaiseEvent((ISectorMapObjectEntity)m_From, (Action<ISectorMapWarpTravelHandler>)delegate(ISectorMapWarpTravelHandler h)
		{
			h.HandleWarpTravelStarted(m_Passage);
		}, isCheckRuntime: true);
	}

	private void StopTravel()
	{
		m_IsTravelling = false;
		WarpTravelState warpTravelState = Game.Instance.Player.WarpTravelState;
		warpTravelState.IsInWarpTravel = false;
		Transform playerShip = Game.Instance.SectorMapController.VisualParameters.PlayerShip;
		playerShip.position = new Vector3(m_To.Position.x, playerShip.position.y, m_To.Position.z);
		m_StartedBookEvent = false;
		Game.Instance.SectorMapController.UpdateCurrentStarSystem(m_To);
		m_WaitingForEndAnimationToStop = true;
		m_TravelStopped = CurrentTime;
		EventBus.RaiseEvent((ISectorMapObjectEntity)m_From, (Action<ISectorMapWarpTravelHandler>)delegate(ISectorMapWarpTravelHandler h)
		{
			h.HandleWarpTravelStopped();
		}, isCheckRuntime: true);
		if (warpTravelState.TriggeredEtude != null)
		{
			warpTravelState.EtudesInWarpQueue.Remove(Game.Instance.Player.WarpTravelState.TriggeredEtude);
			warpTravelState.TriggeredEtude = null;
		}
		m_EtudeTriggerIndex = 0;
		warpTravelState.TriggeredEtudeInMiddleOfJump.Clear();
	}

	public void PauseTravel()
	{
		if (m_IsTravelling)
		{
			m_TravelPaused = Game.Instance.TimeController.GameTime;
			EventBus.RaiseEvent((ISectorMapObjectEntity)m_From, (Action<ISectorMapWarpTravelHandler>)delegate(ISectorMapWarpTravelHandler h)
			{
				h.HandleWarpTravelPaused();
			}, isCheckRuntime: true);
		}
	}

	public void ResumeTravel()
	{
		m_TravelResumed = Game.Instance.TimeController.GameTime;
		EventBus.RaiseEvent((ISectorMapObjectEntity)m_From, (Action<ISectorMapWarpTravelHandler>)delegate(ISectorMapWarpTravelHandler h)
		{
			h.HandleWarpTravelResumed();
		}, isCheckRuntime: true);
	}

	public void UnpauseManual()
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			ResumeTravel();
		}
	}

	private void ProceedRandomEncounter()
	{
		PFLog.Default.Log("Random encounter in warp");
		PauseTravel();
		if (m_RandomEncounter != null)
		{
			DialogData data = DialogController.SetupDialogWithoutTarget(m_RandomEncounter, null);
			Game.Instance.DialogController.StartDialog(data);
		}
		Game.Instance.Player.WarpTravelState.UniqueRE.Remove((BlueprintDialog re) => re == m_RandomEncounter);
		ExitRandomEncounter();
	}

	private void ExitRandomEncounter()
	{
		m_ShouldProceedRE = false;
		m_REProceeded = true;
	}

	private EtudeTriggerActionInWarpDelayed ShouldPlayEtudeTrigger(SectorMapPassageEntity passage)
	{
		int num = ((passage.CurrentDifficulty == SectorMapPassageEntity.PassageDifficulty.Deadly) ? 20 : int.MinValue);
		EtudeTriggerActionInWarpDelayed result = null;
		int num2 = int.MinValue;
		foreach (BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> item in Game.Instance.Player.WarpTravelState.EtudesInWarpQueue)
		{
			EtudeTriggerActionInWarpDelayed etudeTriggerActionInWarpDelayed = item.Get();
			if (etudeTriggerActionInWarpDelayed.Priority > num2 && etudeTriggerActionInWarpDelayed.Priority > num)
			{
				num2 = etudeTriggerActionInWarpDelayed.Priority;
				result = etudeTriggerActionInWarpDelayed;
			}
		}
		return result;
	}

	private static void ShouldProceedEvent(SectorMapObjectEntity from, SectorMapObjectEntity to, out bool shouldProceedRE, out BlueprintDialog randomEncounter)
	{
		SectorMapPassageEntity passage = Game.Instance.SectorMapController.FindPassageBetween(from, to);
		BlueprintSectorMapPointStarSystem blueprintSectorMapPointStarSystem = to.View.Blueprint as BlueprintSectorMapPointStarSystem;
		if (blueprintSectorMapPointStarSystem?.OverrideBookEvent?.Get() != null)
		{
			ConditionsChecker bookEventConditions = blueprintSectorMapPointStarSystem.BookEventConditions;
			if (bookEventConditions == null || bookEventConditions.Check())
			{
				shouldProceedRE = false;
				randomEncounter = null;
				return;
			}
		}
		shouldProceedRE = !Game.Instance.Player.WarpTravelState.ForbidRE && PassagesGenerator.ShouldProceedRE(passage);
		randomEncounter = (shouldProceedRE ? PassagesGenerator.GenerateRandomEncounter(passage) : null);
	}

	private void SetupWarpTravel(SectorMapObjectEntity from, SectorMapObjectEntity to)
	{
		WarpTravelState warpTravelState = Game.Instance.Player.WarpTravelState;
		m_Passage = Game.Instance.SectorMapController.FindPassageBetween(from, to);
		m_TravelDuration = m_Passage.DurationInDays;
		m_From = from;
		m_To = to;
		warpTravelState.TravelStart = m_From.Blueprint;
		warpTravelState.TravelDestination = m_To.Blueprint;
		m_WaitingForTravelToStart = true;
		m_TravelWaitingStartedTime = CurrentTime;
		m_REProceeded = false;
		warpTravelState.WarpTravelsCount++;
		warpTravelState.IsInWarpTravel = true;
		m_EtudesTriggerListEnded = false;
		m_EtudeTriggerIndex = 0;
		m_LastEventEnded = CurrentTime;
	}

	private void SetupEncounter(BlueprintDialog encounter, RandomWeightsForSave<BlueprintDialogReference> weights)
	{
		m_ShouldProceedRE = (Game.Instance.Player.WarpTravelState.TriggeredEtude = ShouldPlayEtudeTrigger(m_Passage)) == null && encounter != null;
		m_RandomEncounter = (m_ShouldProceedRE ? encounter : null);
		Game.Instance.Player.GlobalMapRandomGenerationState.GetRandomEncountersDialogs(m_Passage.CurrentDifficulty).CopyCurrentWeights(weights);
	}

	private void HandleWarpTravelEffectStopped()
	{
		if (Game.Instance.Player.StarSystemsState.StarSystemsToVisit.Contains(m_To.View.StarSystemToTransit as BlueprintStarSystemMap))
		{
			Game.Instance.Player.StarSystemsState.StarSystemsToVisit.Remove(m_To.View.StarSystemToTransit as BlueprintStarSystemMap);
			SectorMapController.VisitStarSystem(m_To);
		}
	}
}

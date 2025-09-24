using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartNotMoveTrigger : UnitPart, IGameTimeChangedHandler, ISubscriber, IUnitMoveHandler<EntitySubscriber>, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitMoveHandler, EntitySubscriber>, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IDialogStartHandler, IDialogFinishHandler, IHashable
{
	public const string IDLE_REACTION_UNIT = "REACTING_UNIT";

	[JsonProperty]
	private TimeSpan m_TimeSinceNotMoving = TimeSpan.Zero;

	[JsonProperty]
	private bool m_Triggered;

	private UnitNotMovingTrigger m_Component;

	private bool m_Paused;

	[JsonProperty]
	private UnitFact m_ReactingFact;

	[JsonProperty]
	private EntityRef<CutscenePlayerData> m_CutsceneDataRef;

	[JsonProperty]
	private bool m_IgnoreDialogue;

	public bool Triggered => m_Triggered;

	public void Add(UnitFact fact, bool allowDuringDialogue, UnitNotMovingTrigger component)
	{
		m_Component = component;
		m_ReactingFact = fact;
		m_IgnoreDialogue = allowDuringDialogue;
	}

	public void Remove(UnitFact fact, UnitNotMovingTrigger component)
	{
		Abort(stopCutscene: true);
		m_Component = null;
		RemoveSelf();
	}

	private void Abort(bool stopCutscene = false)
	{
		if (m_Triggered)
		{
			using (ContextData<FactData>.Request().Setup(m_ReactingFact))
			{
				m_Component.AbortActions.Run();
			}
			m_Triggered = false;
			if (stopCutscene)
			{
				StopCutscene();
			}
			else
			{
				PlayCutscene(m_Component.ReactAnimationAbort);
			}
		}
		m_TimeSinceNotMoving = TimeSpan.Zero;
	}

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (m_Component == null || m_Paused || Game.Instance.CurrentMode != GameModeType.Default || !base.Owner.IsInGame || Game.Instance.TurnController.InCombat)
		{
			return;
		}
		if (base.Owner.IsInCombat || IsFactOwnerControlledByOtherAnimation())
		{
			Abort(stopCutscene: true);
			return;
		}
		UnitMovementAgentBase maybeMovementAgent = base.Owner.MaybeMovementAgent;
		if (maybeMovementAgent == null)
		{
			return;
		}
		if (!maybeMovementAgent.IsReallyMoving)
		{
			m_TimeSinceNotMoving += delta;
		}
		if (base.Owner.IsConscious && m_TimeSinceNotMoving.Seconds >= m_Component.TimerValue && !m_Triggered && Game.Instance.CurrentMode != GameModeType.StarSystem && Game.Instance.CurrentMode != GameModeType.SpaceCombat)
		{
			using (ContextData<FactData>.Request().Setup(m_ReactingFact))
			{
				m_Component.TimerElapsedActions.Run();
			}
			PlayCutscene(m_Component.ReactAnimationStart);
			m_Triggered = true;
		}
	}

	private bool IsFactOwnerControlledByOtherAnimation()
	{
		CutsceneControlledUnit cutsceneControlledUnit = m_ReactingFact.Owner.CutsceneControlledUnit;
		if (cutsceneControlledUnit?.GetCurrentlyActive() == null)
		{
			return false;
		}
		if (!m_CutsceneDataRef.IsNull)
		{
			return !cutsceneControlledUnit.IsMarkedBy(m_CutsceneDataRef.Entity);
		}
		return true;
	}

	private void StopCutscene()
	{
		m_CutsceneDataRef.Entity?.Stop();
		m_CutsceneDataRef = default(EntityRef<CutscenePlayerData>);
	}

	private void PlayCutscene(CutsceneReference cutsceneReference)
	{
		StopCutscene();
		if (base.Owner.IsInGame)
		{
			BaseUnitEntity owner = m_ReactingFact.Owner;
			if (owner != null && owner.IsInGame)
			{
				CutscenePlayerView unityObject = CutscenePlayerView.Play(cutsceneReference.Get(), new ParametrizedContextSetter
				{
					AdditionalParams = { 
					{
						"REACTING_UNIT",
						(object)m_ReactingFact.Owner.FromAbstractUnitEntity()
					} }
				}, queued: false, base.Owner.HoldingState);
				m_CutsceneDataRef = ObjectExtensions.Or(unityObject, null)?.PlayerData;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (m_Component != null)
		{
			Abort();
		}
	}

	public void HandleUnitJoinCombat()
	{
		if (m_Component != null)
		{
			Abort();
		}
	}

	public void HandleUnitLeaveCombat()
	{
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (m_Component == null)
		{
			m_Component = m_ReactingFact.GetComponent<UnitNotMovingTrigger>();
		}
		m_Paused = false;
	}

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		if (base.Owner.IsInGame && !m_IgnoreDialogue)
		{
			Pause();
		}
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		if (!m_IgnoreDialogue)
		{
			Resume();
		}
	}

	private void Pause()
	{
		Abort();
		m_Paused = true;
	}

	private void Resume()
	{
		m_Paused = false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_TimeSinceNotMoving);
		result.Append(ref m_Triggered);
		Hash128 val2 = ClassHasher<UnitFact>.GetHash128(m_ReactingFact);
		result.Append(ref val2);
		EntityRef<CutscenePlayerData> obj = m_CutsceneDataRef;
		Hash128 val3 = StructHasher<EntityRef<CutscenePlayerData>>.GetHash128(ref obj);
		result.Append(ref val3);
		result.Append(ref m_IgnoreDialogue);
		return result;
	}
}

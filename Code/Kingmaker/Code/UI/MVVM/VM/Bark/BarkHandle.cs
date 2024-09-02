using System;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Signals;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class BarkHandle : IBarkHandle, IUpdatable
{
	private readonly Entity m_Entity;

	private VoiceOverStatus m_VoiceOverStatus;

	private float m_RemainingTime;

	private bool m_IsActive = true;

	private SignalWrapper m_StopPlaySignal;

	private BarkHandle(Entity entity, float duration = -1f, VoiceOverStatus voiceOverStatus = null, bool synced = true)
	{
		if (Game.Instance.CustomUpdateController.TryFind((IUpdatable x) => x is BarkHandle barkHandle && barkHandle.m_Entity == entity, out var result))
		{
			((IBarkHandle)result).InterruptBark();
		}
		m_Entity = entity;
		m_VoiceOverStatus = voiceOverStatus;
		m_RemainingTime = ((voiceOverStatus != null) ? 0f : ((duration > 0f) ? duration : UIUtility.DefaultBarkTime));
		m_StopPlaySignal = (synced ? SignalService.Instance.RegisterNext() : SignalWrapper.Empty);
		Game.Instance.CustomUpdateController.Add(this);
	}

	public BarkHandle(Entity entity, string text, float duration = -1f, VoiceOverStatus voiceOverStatus = null, bool synced = true)
		: this(entity, duration, voiceOverStatus, synced)
	{
		EventBus.RaiseEvent((IEntity)entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
		{
			h.HandleOnShowBark(text);
		}, isCheckRuntime: true);
	}

	public BarkHandle(Entity entity, string text, string name, Color nameColor, float duration = -1f, VoiceOverStatus voiceOverStatus = null, bool synced = true)
		: this(entity, duration, voiceOverStatus, synced)
	{
		EventBus.RaiseEvent((IEntity)entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
		{
			h.HandleOnShowBarkWithName(text, name, nameColor);
		}, isCheckRuntime: true);
	}

	public BarkHandle(Entity entity, string text, string encyclopediaLink, float duration = -1f, VoiceOverStatus voiceOverStatus = null)
		: this(entity, duration, voiceOverStatus)
	{
		EventBus.RaiseEvent((IEntity)entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
		{
			h.HandleOnShowLinkedBark(text, encyclopediaLink);
		}, isCheckRuntime: true);
	}

	void IUpdatable.Tick(float delta)
	{
		if (!IsPlayingBark())
		{
			InterruptBark();
			return;
		}
		m_RemainingTime -= Game.Instance.TimeController.DeltaTime;
		if (!m_Entity.IsInGame)
		{
			InterruptBark();
		}
	}

	public bool IsPlayingBark()
	{
		if (!m_IsActive)
		{
			return false;
		}
		bool num;
		if (m_VoiceOverStatus != null)
		{
			if (m_VoiceOverStatus.IsEnded)
			{
				goto IL_0042;
			}
			num = m_RemainingTime > -60f;
		}
		else
		{
			num = m_RemainingTime > 0f;
		}
		if (num)
		{
			return true;
		}
		goto IL_0042;
		IL_0042:
		return !SignalService.Instance.CheckReadyOrSend(ref m_StopPlaySignal, emptyIsOk: true);
	}

	public void InterruptBark()
	{
		if (m_IsActive)
		{
			m_IsActive = false;
			EventBus.RaiseEvent((IEntity)m_Entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
			{
				h.HandleOnHideBark();
			}, isCheckRuntime: true);
			Game.Instance.CustomUpdateController.Remove(this);
			m_VoiceOverStatus?.Stop();
			m_VoiceOverStatus = null;
			m_RemainingTime = -1f;
			m_StopPlaySignal = SignalWrapper.Empty;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Visual.Sound;

[RequireComponent(typeof(AudioObject))]
public class SoundEventsEmitter : MonoBehaviour, IRegistrationCallback
{
	public class Info : RegisteredObjectBase
	{
		private readonly SoundEventsEmitter m_EventsEmitter;

		private readonly AudioObject m_AudioObject;

		private readonly List<uint> m_PostedEvents = new List<uint>();

		private BlueprintAreaPart m_AreaPart;

		private SoundBanksManager.BankHandle m_BankHandle;

		private bool m_IsActivated;

		private bool m_IsEmitterEnabled;

		private bool m_IsPlaying;

		[CanBeNull]
		private CancellationTokenSource m_PostEventsCancellation;

		public Scene Scene { get; }

		private string Bank => m_EventsEmitter.SoundBank;

		public BlueprintAreaPart AreaPart => m_AreaPart ?? (m_AreaPart = AreaService.FindMechanicBoundsContainsPoint(m_EventsEmitter.transform.position));

		public Info(SoundEventsEmitter eventsEmitter, AudioObject audioObject)
		{
			m_EventsEmitter = eventsEmitter;
			m_AudioObject = audioObject;
			Scene = eventsEmitter.gameObject.scene;
		}

		public SoundBanksManager.BankHandle RequestBank()
		{
			return m_BankHandle ?? (m_BankHandle = SoundBanksManager.LoadBank(Bank));
		}

		public void UnloadBank()
		{
			m_BankHandle?.Unload();
			m_BankHandle = null;
		}

		public void EmitActivated()
		{
			m_IsActivated = true;
			PostEvents();
		}

		public void EmitDeactivated()
		{
			m_IsActivated = false;
			StopEvents();
		}

		protected override void OnEnabled()
		{
			base.OnEnabled();
			if (!LoadingProcess.Instance.IsLoadingInProcess)
			{
				m_IsActivated = true;
			}
			m_IsEmitterEnabled = m_AudioObject.isActiveAndEnabled;
			if (m_IsActivated)
			{
				PostEvents();
			}
		}

		protected override void OnDisabled()
		{
			base.OnDisabled();
			m_IsEmitterEnabled = false;
			StopEvents();
		}

		private async void PostEvents()
		{
			if (!m_IsPlaying && m_IsEmitterEnabled && AkAudioService.IsInitialized)
			{
				m_IsPlaying = true;
				m_PostEventsCancellation = new CancellationTokenSource();
				CancellationToken ct = m_PostEventsCancellation.Token;
				SoundBanksManager.BankHandle soundBank = RequestBank();
				while (soundBank.Loading)
				{
					await Task.Delay(TimeSpan.FromMilliseconds(100.0), ct);
				}
				SoundFXSettings[] eventsOnEnable = m_EventsEmitter.EventsOnEnable;
				foreach (SoundFXSettings slot in eventsOnEnable)
				{
					PostEvent(slot, ct);
				}
			}
		}

		private void StopEvents()
		{
			if (!m_IsPlaying)
			{
				return;
			}
			m_IsPlaying = false;
			m_PostEventsCancellation?.Cancel();
			foreach (uint postedEvent in m_PostedEvents)
			{
				StopEvent(postedEvent);
			}
			m_PostedEvents.Clear();
		}

		private async void PostEvent(SoundFXSettings slot, CancellationToken ct)
		{
			try
			{
				await Task.Delay(TimeSpan.FromMilliseconds(slot.Delay), ct);
				uint num = SoundEventsManager.PostEvent(slot.Event, m_EventsEmitter.gameObject);
				if (num != 0)
				{
					AkSoundEngine.SetRTPCValueByPlayingID("SpellGain", slot.Gain, num);
					AkSoundEngine.SetRTPCValueByPlayingID("SpellPitch", slot.Pitch, num);
					m_PostedEvents.Add(num);
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				AkAudioService.Log.Exception(ex2);
			}
		}

		private static void StopEvent(uint eventId)
		{
			try
			{
				AkSoundEngine.StopPlayingID(eventId, 100);
			}
			catch (Exception ex)
			{
				AkAudioService.Log.Exception(ex);
			}
		}
	}

	[AkBankReference]
	public string SoundBank;

	public SoundFXSettings[] EventsOnEnable;

	private Info m_Info;

	private AudioObject m_AudioObject;

	private readonly LinkedListNode<IRegistrationCallback> m_Registration;

	public SoundEventsEmitter()
	{
		m_Registration = new LinkedListNode<IRegistrationCallback>(this);
	}

	private void Awake()
	{
		m_AudioObject = GetComponent<AudioObject>();
		m_Info = new Info(this, m_AudioObject);
	}

	private void OnEnable()
	{
		m_AudioObject.RequestRegister(m_Registration);
	}

	private void OnDisable()
	{
		m_AudioObject.CancelRegistrationRequest(m_Registration);
	}

	public void OnAfterRegister()
	{
		m_Info.Enable();
	}

	public void OnBeforeUnregister()
	{
		m_Info.Disable();
	}
}

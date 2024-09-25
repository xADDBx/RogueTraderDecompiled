using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[RequireComponent(typeof(AudioObject))]
public class SoundFx : MonoBehaviour, IRegistrationCallback, IPooledComponent
{
	private readonly List<SoundFXSettings> m_DelayedEvents = new List<SoundFXSettings>();

	private bool m_ShouldPlayOnDestroy;

	[NonSerialized]
	public bool BlockSoundFXPlaying;

	public SoundFXSettings[] EventsOnStart;

	public SoundFXSettings[] EventsOnDestroy;

	public SoundFx Prefab;

	private AudioObject m_AudioObject;

	private readonly LinkedListNode<IRegistrationCallback> m_RegistrationRequest;

	private static readonly WaitForEndOfFrame WaitForEndOfFrameToken = new WaitForEndOfFrame();

	public SoundFx()
	{
		m_RegistrationRequest = new LinkedListNode<IRegistrationCallback>(this);
	}

	private void Awake()
	{
		m_AudioObject = GetComponent<AudioObject>();
	}

	public void OnEnable()
	{
		if ((bool)m_AudioObject)
		{
			m_AudioObject.RequestRegister(m_RegistrationRequest);
		}
	}

	public void OnDisable()
	{
		if ((bool)m_AudioObject)
		{
			m_AudioObject.CancelRegistrationRequest(m_RegistrationRequest);
		}
	}

	private void PlayOnDestroyEvents()
	{
		if (!m_ShouldPlayOnDestroy)
		{
			return;
		}
		foreach (SoundFXSettings delayedEvent in m_DelayedEvents)
		{
			SoundEventPlayer.PlaySound(delayedEvent, base.gameObject);
		}
		m_DelayedEvents.Clear();
		SoundFXSettings[] array = (Prefab ? Prefab.EventsOnDestroy : EventsOnDestroy);
		for (int i = 0; i < array.Length; i++)
		{
			SoundEventPlayer.PlaySound(array[i], base.gameObject);
		}
		m_ShouldPlayOnDestroy = false;
	}

	public void OnAfterRegister()
	{
		Game.Instance.CoroutinesController.Start(PostponedAfterRegister());
	}

	public void OnBeforeUnregister()
	{
		PlayOnDestroyEvents();
	}

	public void OnClaim()
	{
	}

	public void OnRelease()
	{
		BlockSoundFXPlaying = false;
	}

	private IEnumerator PostponedAfterRegister()
	{
		yield return Application.isBatchMode ? null : WaitForEndOfFrameToken;
		if (!(this == null) && !BlockSoundFXPlaying)
		{
			m_ShouldPlayOnDestroy = true;
			m_DelayedEvents.Clear();
			SoundFXSettings[] array = (Prefab ? Prefab.EventsOnStart : EventsOnStart);
			for (int i = 0; i < array.Length; i++)
			{
				SoundEventPlayer.PlaySound(array[i], base.gameObject);
			}
		}
	}
}

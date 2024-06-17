using System.Collections.Generic;
using System.Linq;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

[RequireComponent(typeof(AudioObject))]
public class DefaultListener : RegisteredBehaviour, IRegistrationCallback
{
	private readonly LinkedListNode<IRegistrationCallback> m_RegistrationNode;

	private AudioObject m_AudioObject;

	public DefaultListener()
	{
		m_RegistrationNode = new LinkedListNode<IRegistrationCallback>(this);
	}

	private void Awake()
	{
		m_AudioObject = GetComponent<AudioObject>();
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if (!m_AudioObject)
		{
			return;
		}
		foreach (DefaultListener item in ObjectRegistry<DefaultListener>.Instance.Where((DefaultListener other) => other != this).ToList())
		{
			AkAudioService.Log.Log("Destroying audio listener: {0} in {1}", item.name, item.gameObject.scene.name);
			Object.Destroy(item);
		}
		Object.DontDestroyOnLoad(base.gameObject);
		m_AudioObject.RequestRegister(m_RegistrationNode);
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		if ((bool)m_AudioObject)
		{
			m_AudioObject.CancelRegistrationRequest(m_RegistrationNode);
		}
	}

	public void OnAfterRegister()
	{
		AkSoundEngine.SetDefaultListeners(new ulong[1] { AkSoundEngine.GetAkGameObjectID(base.gameObject) }, 1u);
	}

	public void OnBeforeUnregister()
	{
		AkSoundEngine.RemoveDefaultListener(base.gameObject);
		AudioZone.RemoveListenerFromAllZones();
	}
}

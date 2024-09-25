using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.GameModes;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

[DisallowMultipleComponent]
public class AudioObject : RegisteredBehaviour
{
	[SerializeField]
	private bool m_ShouldUpdatePosition = true;

	[SerializeField]
	private bool m_ShouldUpdateZones = true;

	[SerializeField]
	private bool m_LogEverything;

	private bool m_IsRegistered;

	private readonly LinkedList<IRegistrationCallback> m_RegisterRequests = new LinkedList<IRegistrationCallback>();

	private EnvironmentData m_EnvData;

	public bool ShouldUpdatePosition
	{
		get
		{
			return m_ShouldUpdatePosition;
		}
		set
		{
			m_ShouldUpdatePosition = value;
		}
	}

	public bool ShouldUpdateZones => m_ShouldUpdateZones;

	public bool EnvironmentChanged => m_EnvData?.Changed ?? false;

	public void RequestRegister(LinkedListNode<IRegistrationCallback> request)
	{
		m_RegisterRequests.AddLast(request);
		if (m_IsRegistered)
		{
			request.Value.OnAfterRegister();
		}
	}

	public void CancelRegistrationRequest(LinkedListNode<IRegistrationCallback> request)
	{
		m_RegisterRequests.Remove(request);
		if (m_IsRegistered)
		{
			request.Value.OnBeforeUnregister();
		}
	}

	public void OnAudioInitialized()
	{
		Register();
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		Register();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		Unregister();
	}

	private void Register()
	{
		if (m_IsRegistered || !AkAudioService.IsInitialized)
		{
			return;
		}
		AKRESULT aKRESULT = AkSoundEngine.RegisterGameObj(base.gameObject, base.gameObject.name);
		m_IsRegistered = aKRESULT == AKRESULT.AK_Success;
		if (!m_IsRegistered)
		{
			return;
		}
		UpdatePosition();
		if (ShouldUpdateZones)
		{
			UpdateZone();
		}
		if (m_LogEverything)
		{
			AkAudioService.Log.Log(this, "Audio object registration: {0}: {1}", base.name, aKRESULT);
		}
		foreach (IRegistrationCallback registerRequest in m_RegisterRequests)
		{
			registerRequest.OnAfterRegister();
		}
	}

	private void UpdatePosition()
	{
		Transform transform = base.transform;
		using (ProfileScope.New("Update Object Position"))
		{
			AkSoundEngine.SetObjectPosition(base.gameObject, transform.position, transform.forward, transform.up);
			transform.hasChanged = false;
		}
	}

	public void UpdateScalingFactor()
	{
		if (Game.HasInstance && Game.Instance?.CurrentMode == GameModeType.StarSystem)
		{
			AkSoundEngine.SetScalingFactor(base.gameObject, BlueprintRoot.Instance.Sound.StarSystemAudioScalingFactor);
		}
	}

	private void UpdateZone()
	{
		using (ProfileScope.New("Update Object Zone"))
		{
			foreach (AudioZone item in ObjectRegistry<AudioZone>.Instance.EmptyIfNull())
			{
				item.UpdateGameObj(this);
			}
			m_EnvData?.UpdateAuxSend(base.gameObject, base.transform.position);
		}
	}

	public void OnUpdate()
	{
		if (ShouldUpdatePosition && base.transform.hasChanged)
		{
			UpdatePosition();
		}
		if (ShouldUpdateZones && EnvironmentChanged)
		{
			UpdateZone();
		}
	}

	public void AddAuxSend(AudioEnvironment env)
	{
		if (m_EnvData == null)
		{
			m_EnvData = new EnvironmentData();
			m_EnvData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		m_EnvData.TryAddEnvironment(env);
		if (m_LogEverything)
		{
			AkAudioService.Log.Log(this, "Audio object add zone: " + base.name + " is in " + env.name);
		}
	}

	public void RemoveAuxSend(AudioEnvironment env)
	{
		if (m_EnvData == null)
		{
			m_EnvData = new EnvironmentData();
			m_EnvData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		m_EnvData.RemoveEnvironment(env);
		if (m_LogEverything)
		{
			AkAudioService.Log.Log(this, "Audio object remove zone: " + base.name + " is out of " + env.name);
		}
	}

	private void Unregister()
	{
		if (!m_IsRegistered)
		{
			return;
		}
		foreach (IRegistrationCallback registerRequest in m_RegisterRequests)
		{
			registerRequest.OnBeforeUnregister();
		}
		AkSoundEngine.UnregisterGameObj(base.gameObject);
		foreach (AudioZone item in ObjectRegistry<AudioZone>.Instance.EmptyIfNull())
		{
			item.OnDisableObject(this);
		}
		m_IsRegistered = false;
		if (m_LogEverything)
		{
			AkAudioService.Log.Log(this, "Audio object unregistered: " + base.name);
		}
	}
}

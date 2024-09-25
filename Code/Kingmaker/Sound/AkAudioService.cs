using System;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Sound;

public class AkAudioService : IService, IDisposable
{
	private class AudioServiceDrivingBehaviour : MonoBehaviour
	{
		private void LateUpdate()
		{
			Instance?.OnLateUpdate();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			AkSoundEngineController.Instance.OnApplicationPause(pauseStatus);
		}

		private void OnApplicationFocus(bool focus)
		{
			AkSoundEngineController.Instance.OnApplicationFocus(focus);
		}

		private void OnApplicationQuit()
		{
			AkSoundEngineController.Instance.Terminate();
		}

		private void OnDestroy()
		{
			Log.Log("WwiseUnity: Audio driver destroyed.");
		}
	}

	public static readonly LogChannel Log = PFLog.Audio;

	public static readonly LogChannel LogRTPC = PFLog.WWiseRTPC;

	private static ServiceProxy<AkAudioService> s_Proxy;

	private bool m_IsInitialized;

	private AudioServiceDrivingBehaviour m_Driver;

	private bool m_UsingDummyListener;

	public static AkAudioService Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<AkAudioService>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public static bool IsInitialized => Instance?.m_IsInitialized ?? false;

	public static void EnsureAudioInitialized()
	{
		if (Instance == null)
		{
			Services.RegisterServiceInstance(new AkAudioService());
		}
		Instance?.Initialize();
	}

	private void Initialize()
	{
		if (IsInitialized)
		{
			return;
		}
		m_IsInitialized = AkSoundEngine.IsInitialized();
		AkLogger.Instance.Init();
		if (IsInitialized)
		{
			Log.Error("WwiseUnity: Sound engine is already initialized.");
			return;
		}
		if (!AkWwiseInitializationSettings.Instance.InitializeSoundEngine())
		{
			Log.Error("WwiseUnity: Failed to initialize sound engine.");
			return;
		}
		if (Application.isEditor)
		{
			AkCallbackManager.SetMonitoringCallback(AkMonitorErrorLevel.ErrorLevel_All, OnAkLog);
		}
		AudioFilePackagesSettings.Instance.LoadPackagesChunk(AudioFilePackagesSettings.AudioChunk.MainGame);
		AkSoundEngine.SetAkCallbacks(new AkSetup());
		Log.Log("WwiseUnity: Audio service started.");
		m_Driver = new GameObject("[AkSoundService]").AddComponent<AudioServiceDrivingBehaviour>();
		UnityEngine.Object.DontDestroyOnLoad(m_Driver);
		m_IsInitialized = true;
		AudioFilePackagesSettings.Instance.LoadBanksChunk(AudioFilePackagesSettings.AudioChunk.MainGame);
		AkSoundEngineController.Instance?.LateUpdate();
		Runner.EnsureBasicAudioBanks();
		foreach (AudioObject item in ObjectRegistry<AudioObject>.Instance.EmptyIfNull())
		{
			item.OnAudioInitialized();
		}
		Log.Log(m_Driver, "WwiseUnity: Audio service driver: " + m_Driver);
	}

	private void OnAkLog(AkMonitorErrorCode in_errorcode, AkMonitorErrorLevel in_errorlevel, uint in_playingid, ulong in_gameobjid, string in_msg)
	{
		GameObject gameObject = null;
		bool flag = false;
		foreach (AudioObject item in ObjectRegistry<AudioObject>.Instance)
		{
			if (item != null && item.gameObject.GetInstanceID() == (int)in_gameobjid)
			{
				gameObject = item.gameObject;
				break;
			}
		}
		if (gameObject != null)
		{
			in_msg += string.Format(" (GameObject: {0}{1})", gameObject, flag ? ", and is not an AudioObject!" : "");
		}
		if (in_errorlevel == AkMonitorErrorLevel.ErrorLevel_Error)
		{
			if (in_msg.Contains("RTPC"))
			{
				LogRTPC.Error(gameObject, in_msg);
			}
			else
			{
				Log.Error(gameObject, in_msg);
			}
		}
		else
		{
			Log.Log(gameObject, in_msg);
		}
	}

	private void OnLateUpdate()
	{
		using (Counters.Audio?.Measure())
		{
			DefaultListener maybeSingle = ObjectRegistry<DefaultListener>.Instance.MaybeSingle;
			using (ProfileScope.New("UpdateListenerPosition"))
			{
				if ((bool)maybeSingle)
				{
					AudioZone.UpdateListenerPosition(maybeSingle.transform);
				}
			}
			using (ProfileScope.New("Dummy Listener"))
			{
				if (m_UsingDummyListener == (bool)maybeSingle)
				{
					if ((bool)maybeSingle)
					{
						AkSoundEngine.RemoveDefaultListener(m_Driver.gameObject);
					}
					else
					{
						AkSoundEngine.AddDefaultListener(m_Driver.gameObject);
						AudioZone.UpdateListenerPosition(m_Driver.transform);
					}
					m_UsingDummyListener = !maybeSingle;
				}
			}
			using (ProfileScope.New("Update Objects"))
			{
				foreach (AudioObject item in ObjectRegistry<AudioObject>.Instance)
				{
					item.OnUpdate();
				}
			}
			using (ProfileScope.New("Update AkSoundEngineController"))
			{
				AkSoundEngine.WakeupFromSuspend();
				AkSoundEngineController.Instance.LateUpdate();
			}
			SoundBanksManager.TryToUnloadBanks();
		}
	}

	public void Dispose()
	{
		if (AkSoundEngine.IsInitialized())
		{
			AkWwiseInitializationSettings.Instance.TerminateSoundEngine();
			m_IsInitialized = false;
			AudioFilePackagesSettings.Instance.UnloadPackagesChunk(AudioFilePackagesSettings.AudioChunk.MainGame);
		}
		if ((bool)m_Driver)
		{
			Log.Log("WwiseUnity: Audio service stopped.");
			UnityEngine.Object.Destroy(m_Driver.gameObject);
		}
	}
}

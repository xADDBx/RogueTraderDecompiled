using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Common;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.GameConst;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class SplashScreenController : MonoBehaviour
{
	[Serializable]
	public class ScreenUnit
	{
		public CanvasGroup CanvasGroup;

		public float InTime = 0.2f;

		public float DelayTime = 0.2f;

		public float OutTime = 0.4f;

		public Ease Ease = Ease.OutCubic;

		public VideoPlayerHelper VideoPlayer;

		[AkEventReference]
		public string AKSoundEvent;

		public uint SoundId;

		public bool FirstLaunchAnotherSoundEvent;

		[ShowIf("FirstLaunchAnotherSoundEvent")]
		[AkEventReference]
		public string AKFirstLaunchSoundEvent;

		[ShowIf("FirstLaunchAnotherSoundEvent")]
		public uint FirstLaunchSoundId;

		public bool HideInFirstLaunch;
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("SplashScreenController");

	[SerializeField]
	[UsedImplicitly]
	private List<ScreenUnit> m_Screens;

	private CanvasGroup m_BaseCanvas;

	private bool m_Enabled;

	private bool m_FirstTime;

	private bool m_EventsPrepared;

	private Sequence m_TweenSequence;

	private CanvasGroup BaseCanvas => m_BaseCanvas ?? (m_BaseCanvas = GetComponent<CanvasGroup>());

	private void Awake()
	{
		if (Runner.BasicAudioBanksLoaded)
		{
			PrepareEvents();
		}
		else
		{
			Runner.OnBasicAudioBanksLoaded += OnBasicAudioBanksLoadedHandler;
		}
	}

	private void Start()
	{
		if (GameStarter.InitComplete)
		{
			ShowSplashScreen();
		}
		else
		{
			GameStarter.OnInitComplete += ShowSplashScreen;
		}
	}

	private void OnDestroy()
	{
		Runner.OnBasicAudioBanksLoaded -= OnBasicAudioBanksLoadedHandler;
	}

	public void ShowSplashScreen()
	{
		if (GameStarter.IsSkippingMainMenu())
		{
			StartCoroutine(SkipWaitingSplashScreens());
			return;
		}
		m_Enabled = true;
		BaseCanvas.alpha = 1f;
		m_FirstTime = PlayerPrefs.GetInt("FirstTimeLogoShow", -1) == -1;
		PlayerPrefs.SetInt("FirstTimeLogoShow", 1);
		m_TweenSequence = DOTween.Sequence();
		m_TweenSequence.AppendInterval(0.1f);
		foreach (ScreenUnit screen in m_Screens)
		{
			if (FirstLaunchSettingsVM.HasShown || !screen.HideInFirstLaunch)
			{
				UIUtilityShowSplashScreen.ShowSplashScreen(screen, m_TweenSequence, base.gameObject);
			}
		}
		m_TweenSequence.AppendCallback(OnComplete);
		StartCoroutine(DelayedShow());
	}

	private IEnumerator SkipWaitingSplashScreens()
	{
		while (MainMenuLoadingScreen.Instance == null)
		{
			yield return null;
		}
		Complete();
	}

	private IEnumerator DelayedShow()
	{
		yield return null;
		yield return null;
		yield return null;
		while (!AkSoundEngine.IsInitialized())
		{
			yield return null;
		}
		PrepareEvents();
		m_TweenSequence.Play().SetUpdate(isIndependentUpdate: true);
	}

	private void Update()
	{
		if (m_Enabled && !m_FirstTime && Input.anyKey)
		{
			Complete();
		}
	}

	private void Complete()
	{
		m_TweenSequence?.Kill();
		foreach (ScreenUnit screen in m_Screens)
		{
			if (!FirstLaunchSettingsVM.HasShown && screen.FirstLaunchAnotherSoundEvent)
			{
				if (screen.FirstLaunchSoundId != 0)
				{
					SoundEventsManager.StopPlayingById(screen.FirstLaunchSoundId);
					Logger.Log($"Skip splash screen audio event={screen.AKFirstLaunchSoundEvent}, soundId={screen.FirstLaunchSoundId}");
					screen.FirstLaunchSoundId = 0u;
				}
			}
			else if (screen.SoundId != 0)
			{
				SoundEventsManager.StopPlayingById(screen.SoundId);
				Logger.Log($"Skip splash screen audio event={screen.AKSoundEvent}, soundId={screen.SoundId}");
				screen.SoundId = 0u;
			}
		}
		OnComplete();
	}

	private void OnComplete()
	{
		m_Enabled = false;
		BaseCanvas.alpha = 0f;
		if (FirstLaunchSettingsVM.HasShown)
		{
			SoundBanksManager.UnloadBank(UIConsts.SplashScreens);
		}
		MainMenuLoadingScreen.Instance.StartLoading(delegate
		{
			GameStarter.Instance.StartGame();
		});
	}

	private void OnBasicAudioBanksLoadedHandler()
	{
		PrepareEvents();
		Runner.OnBasicAudioBanksLoaded -= OnBasicAudioBanksLoadedHandler;
	}

	private void PrepareEvents()
	{
		if (!m_EventsPrepared)
		{
			string[] array = (from x in m_Screens.SelectMany((ScreenUnit x) => new string[2] { x.AKSoundEvent, x.AKFirstLaunchSoundEvent })
				where !string.IsNullOrEmpty(x)
				select x).Distinct().ToArray();
			if (array.Length != 0)
			{
				AkSoundEngine.PrepareEvent(AkPreparationType.Preparation_LoadAndDecode, array, (uint)array.Length);
			}
			Logger.Log("Prepared events : " + string.Join(",", array));
			m_EventsPrepared = true;
		}
	}
}

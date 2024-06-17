using System;
using System.Collections;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Visual.Sound;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class MainMenuLoadingScreen : MonoBehaviour
{
	[CanBeNull]
	public static MainMenuLoadingScreen Instance;

	[Header("Background")]
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_BackgroundAnimator;

	[Header("Logo")]
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_LogoAnimator;

	[Header("Loading")]
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_ProgressBarAnimator;

	[SerializeField]
	private RectTransform m_ProgressParent;

	[SerializeField]
	private RectTransform m_ProgressTransform;

	[SerializeField]
	private TextMeshProUGUI m_PercentText;

	[SerializeField]
	private LoadingScreenGlitchAnimator m_GlitchFx;

	private Action m_StartLoadingCallback;

	private Action m_LoadingFinishedCallback;

	private Tweener m_ProgressTweener;

	public void OnEnable()
	{
		Instance = this;
	}

	public void OnStart()
	{
		PFLog.Default.Log("Logo Show Requested");
		if (GameStarter.IsArbiterMode())
		{
			base.gameObject.SetActive(value: false);
			GameStarter.Instance.StartGame();
			return;
		}
		m_BackgroundAnimator.Initialize();
		m_LogoAnimator.Initialize();
		m_ProgressBarAnimator.Initialize();
		base.gameObject.SetActive(value: true);
		ShowLogo();
	}

	public void OnDestroy()
	{
		Instance = null;
	}

	private void ShowLogo()
	{
		base.gameObject.SetActive(value: true);
		StartCoroutine(DelayedShow());
	}

	private void HideLogo()
	{
		StartCoroutine(DelayedHide());
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
		PFLog.System.Log("Start Show Logo Animation");
		SoundState.Instance.ResetState(SoundStateType.MainMenu);
		Show(state: true);
	}

	private IEnumerator DelayedHide()
	{
		yield return null;
		PFLog.System.Log("Start Hide Logo Animation");
		Show(state: false);
	}

	private void Show(bool state)
	{
		if (state)
		{
			m_BackgroundAnimator.AppearAnimation();
			m_ProgressBarAnimator.AppearAnimation();
			m_LogoAnimator.AppearAnimation(delegate
			{
				m_StartLoadingCallback();
			});
		}
		else
		{
			m_BackgroundAnimator.DisappearAnimation(delegate
			{
				m_LoadingFinishedCallback();
			});
			m_ProgressBarAnimator.DisappearAnimation();
			m_LogoAnimator.DisappearAnimation();
		}
	}

	public void StartLoading(Action callback)
	{
		m_StartLoadingCallback = callback;
		ShowLogo();
	}

	public void EndLoading(Action callback)
	{
		m_LoadingFinishedCallback = callback;
		m_ProgressParent.gameObject.SetActive(value: false);
		m_GlitchFx.StartGlitch(null);
		HideLogo();
	}

	public void SetLoadingProgress(float virtualProgress)
	{
		virtualProgress = Mathf.Clamp01(virtualProgress);
		float progressWidth = m_ProgressParent.rect.width;
		float startValue = m_ProgressTransform.rect.width / progressWidth;
		m_ProgressTweener?.Kill();
		m_ProgressTweener = DOTween.To(delegate(float x)
		{
			m_ProgressTransform.sizeDelta = new Vector2(x * progressWidth, m_ProgressTransform.rect.height);
			m_PercentText.text = Mathf.CeilToInt(x * 100f) + "%";
		}, startValue, virtualProgress, 0.5f).SetEase(Ease.Linear);
	}
}

using System;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.DLC;
using Kingmaker.Stores;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.Common.Animations;

public class MainMenuButtonFx : ContextButtonFx
{
	[Serializable]
	public struct EffectSettings
	{
		public float FirstDelay;

		public float SecondDelay;

		public float FirstStay;

		public float FadeInTime;

		public float FadeOutTime;
	}

	[FormerlySerializedAs("Label")]
	[Header("Components")]
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[FormerlySerializedAs("BlinkBackground")]
	[SerializeField]
	private CanvasGroup m_BlinkBackground;

	[FormerlySerializedAs("GlitchFX")]
	[SerializeField]
	private Image m_GlitchFX;

	[SerializeField]
	private Material m_FxMaterial;

	[FormerlySerializedAs("Button")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_HoverCanvasGroup;

	[Header("Hover")]
	[FormerlySerializedAs("m_HoveredTime")]
	[SerializeField]
	private float m_HoveredImageTime;

	[SerializeField]
	private float m_HoveredGlitchTime;

	[Header("Blink")]
	[SerializeField]
	private float m_ShowDelay;

	[Header("LookAtMe")]
	[SerializeField]
	private bool m_LookAtMeEnable;

	[ShowIf("m_LookAtMeEnable")]
	[SerializeField]
	private bool m_IsAddonsWindow;

	[ShowIf("m_LookAtMeEnable")]
	[SerializeField]
	private CanvasGroup m_LookAtMeCanvasGroup;

	[ShowIf("m_LookAtMeEnable")]
	[SerializeField]
	private float m_LookAtMeFadeIn = 1.5f;

	[ShowIf("m_LookAtMeEnable")]
	[SerializeField]
	private float m_LookAtMeFadeOut = 1.5f;

	[ShowIf("m_LookAtMeEnable")]
	[SerializeField]
	private float m_LookAtMeStayOnScreen;

	[ShowIf("m_LookAtMeEnable")]
	[SerializeField]
	private float m_LookAtMeInterval;

	private Sequence m_HoveredFxSequence;

	private const float DefaultBump = 0.2f;

	private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

	private Sequence m_LookAtMeSequence;

	private void OnEnable()
	{
		if (m_LookAtMeEnable && !(m_LookAtMeCanvasGroup == null))
		{
			if (m_IsAddonsWindow)
			{
				StartLookAtMeAnimationAddons();
			}
			else
			{
				StartLookAtMeAnimation();
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)m_FxMaterial)
		{
			m_FxMaterial.SetFloat(BumpScale, 0.2f);
		}
		m_LookAtMeSequence.Kill();
		m_LookAtMeSequence = null;
	}

	public void PlayFXSequence(EffectSettings effectSettings)
	{
		if (m_BlinkBackground == null || m_GlitchFX == null || m_Label == null || m_Button == null)
		{
			return;
		}
		Sequence s = DOTween.Sequence();
		SetDefaultValues();
		m_Label.alpha = 0f;
		s.AppendInterval(effectSettings.FirstDelay);
		s.AppendCallback(delegate
		{
			if (Game.Instance.RootUiContext.FullScreenUIType != FullScreenUIType.Settings)
			{
				UISounds.Instance.Sounds.MainMenu.ButtonsFirstLaunchFxAnimation.Play();
			}
		});
		s.Append(m_BlinkBackground.DOFade(1f, effectSettings.FadeInTime).SetEase(Ease.Linear));
		s.Join(m_Label.DOFade(1f, effectSettings.FadeInTime)).SetEase(Ease.Linear);
		s.AppendInterval(effectSettings.FirstStay);
		s.Append(m_BlinkBackground.DOFade(0f, effectSettings.FadeOutTime).SetEase(Ease.Linear));
		s.Join(m_Label.DOFade(0f, effectSettings.FadeOutTime).SetEase(Ease.Linear));
		s.AppendInterval(effectSettings.SecondDelay);
		s.Append(m_BlinkBackground.DOFade(1f, effectSettings.FadeInTime).SetEase(Ease.Linear));
		s.Join(m_Label.DOFade(1f, effectSettings.FadeInTime).SetEase(Ease.Linear));
		s.Join(m_GlitchFX.DOFade(1f, 0.1f));
		s.Append(m_GlitchFX.DOFade(0f, 0.1f));
		s.Append(m_BlinkBackground.DOFade(0f, 0.1f).SetEase(Ease.Linear));
	}

	public override void DoHovered(bool state)
	{
		if (!state)
		{
			SetDefaultValues();
			return;
		}
		if ((bool)m_FxMaterial)
		{
			m_FxMaterial.SetFloat(BumpScale, UnityEngine.Random.Range(0.05f, 0.12f));
		}
		m_HoveredFxSequence = DOTween.Sequence();
		m_HoveredFxSequence.Append(m_HoverCanvasGroup.DOFade(1f, m_HoveredImageTime).SetEase(Ease.InElastic).SetUpdate(isIndependentUpdate: true));
		m_HoveredFxSequence.Join(m_GlitchFX.DOFade(1f, m_HoveredGlitchTime).SetEase(Ease.InOutBack).SetUpdate(isIndependentUpdate: true));
		m_HoveredFxSequence.Append(m_GlitchFX.DOFade(0f, m_HoveredGlitchTime).SetEase(Ease.InOutBack).SetUpdate(isIndependentUpdate: true));
	}

	public override void DoBlink()
	{
		if (m_BlinkBackground == null)
		{
			return;
		}
		Sequence s = DOTween.Sequence();
		s.AppendInterval(m_ShowDelay);
		s.AppendCallback(delegate
		{
			if (FirstLaunchSettingsVM.HasShown && Game.Instance.RootUiContext.FullScreenUIType != FullScreenUIType.Settings)
			{
				UISounds.Instance.Sounds.MainMenu.ButtonsFxAnimation.Play();
			}
		});
		s.Append(m_BlinkBackground.DOFade(0.5f, 0.1f)).SetEase(Ease.Linear);
		s.Join(m_Label.DOFade(0.5f, 0.1f)).SetEase(Ease.Linear);
		s.Append(m_BlinkBackground.DOFade(0.2f, 0.1f)).SetEase(Ease.Linear);
		s.Join(m_Label.DOFade(0.2f, 0.1f)).SetEase(Ease.Linear);
		s.Append(m_BlinkBackground.DOFade(1f, 0.1f)).SetEase(Ease.Linear);
		s.Join(m_Label.DOFade(1f, 0.1f)).SetEase(Ease.Linear);
		s.Append(m_BlinkBackground.DOFade(0f, 0.1f)).SetEase(Ease.Linear);
	}

	private void SetDefaultValues()
	{
		m_HoveredFxSequence?.Kill();
		m_GlitchFX.color = new Color(1f, 1f, 1f, 0f);
		m_BlinkBackground.alpha = 0f;
		m_HoverCanvasGroup.alpha = 0f;
	}

	private void StartLookAtMeAnimation()
	{
		m_LookAtMeSequence = DOTween.Sequence();
		m_LookAtMeSequence.Append(m_LookAtMeCanvasGroup.DOFade(1f, m_LookAtMeFadeIn));
		m_LookAtMeSequence.AppendInterval(m_LookAtMeStayOnScreen);
		m_LookAtMeSequence.Append(m_LookAtMeCanvasGroup.DOFade(0f, m_LookAtMeFadeOut));
		m_LookAtMeSequence.AppendInterval(m_LookAtMeInterval);
		m_LookAtMeSequence.Play().SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Restart);
	}

	private void StartLookAtMeAnimationAddons()
	{
		if (!CheckSomeDlcNichtGesehen())
		{
			return;
		}
		m_LookAtMeSequence = DOTween.Sequence();
		m_LookAtMeSequence.Append(m_LookAtMeCanvasGroup.DOFade(1f, m_LookAtMeFadeIn));
		m_LookAtMeSequence.AppendInterval(m_LookAtMeStayOnScreen);
		m_LookAtMeSequence.Append(m_LookAtMeCanvasGroup.DOFade(0f, m_LookAtMeFadeOut));
		m_LookAtMeSequence.AppendInterval(m_LookAtMeInterval);
		m_LookAtMeSequence.OnStepComplete(delegate
		{
			if (!CheckSomeDlcNichtGesehen())
			{
				m_LookAtMeSequence.Kill();
			}
		});
		m_LookAtMeSequence.Play().SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Restart);
	}

	private bool CheckSomeDlcNichtGesehen()
	{
		return (from dlc in StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>()
			where !dlc.HideDlc
			select dlc).Any((BlueprintDlc dlc) => PlayerPrefs.GetInt("DLCMANAGER_I_SAW_" + dlc.name, 0) == 0);
	}
}

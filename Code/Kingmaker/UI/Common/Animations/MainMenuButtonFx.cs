using System;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.UI.Sound;
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

	private Sequence m_HoveredFxSequence;

	private const float DefaultBump = 0.2f;

	private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

	private void OnDisable()
	{
		if ((bool)m_FxMaterial)
		{
			m_FxMaterial.SetFloat(BumpScale, 0.2f);
		}
	}

	public void PlayFXSequence(EffectSettings effectSettings)
	{
		if (!(m_BlinkBackground == null) && !(m_GlitchFX == null) && !(m_Label == null) && !(m_Button == null))
		{
			Sequence s = DOTween.Sequence();
			SetDefaultValues();
			m_Label.alpha = 0f;
			s.AppendInterval(effectSettings.FirstDelay);
			s.AppendCallback(delegate
			{
				UISounds.Instance.Sounds.MainMenu.ButtonsFirstLaunchFxAnimation.Play();
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
			if (FirstLaunchSettingsVM.HasShown)
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
}

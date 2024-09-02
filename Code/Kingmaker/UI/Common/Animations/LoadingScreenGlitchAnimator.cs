using System;
using System.Collections;
using Kingmaker.UI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common.Animations;

public class LoadingScreenGlitchAnimator : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_ShowCanvasGroup;

	[SerializeField]
	private Image m_MainImage;

	[SerializeField]
	private Sprite m_DefaultSprite;

	[SerializeField]
	private Image m_GlitchImage;

	[SerializeField]
	private Material m_GlitchMaterial;

	[Header("AnimationValues")]
	[SerializeField]
	private float m_FadeTime = 1f;

	[SerializeField]
	private float m_GlitchStep = 10f;

	[SerializeField]
	private float m_GlitchTime = 1f;

	[SerializeField]
	private float m_DelayBeforeGlitch = 0.3f;

	[SerializeField]
	private float m_GlitchStrength = 0.2f;

	private static readonly int OffsetScale = Shader.PropertyToID("_OffsetScale");

	private static readonly int OffsetLinesFrequency = Shader.PropertyToID("_OffsetLinesFrequency");

	private float m_NextChangeTime;

	private Coroutine m_GlitchCo;

	private Action m_OnComplete;

	private void OnDisable()
	{
		Clear();
	}

	public void SetGlitchImage(Sprite sprite)
	{
		m_GlitchImage.sprite = sprite;
	}

	public void StartGlitch(Action onComplete)
	{
		m_OnComplete = onComplete;
		if (m_GlitchImage.sprite != null)
		{
			m_GlitchImage.gameObject.SetActive(value: true);
			UISounds.Instance.Play(UISounds.Instance.Sounds.LoadingScreen.FinishGlitch, isButton: false, playAnyway: true);
			m_GlitchCo = StartCoroutine(GlitchCo());
		}
		else
		{
			m_OnComplete?.Invoke();
		}
	}

	public void ClearGlitch()
	{
		Clear();
	}

	private void Clear()
	{
		if (m_GlitchCo != null)
		{
			StopCoroutine(m_GlitchCo);
		}
		m_GlitchMaterial.SetFloat(OffsetLinesFrequency, 0f);
		m_GlitchMaterial.SetFloat(OffsetScale, 0f);
		m_ShowCanvasGroup.alpha = 0f;
		m_GlitchImage.sprite = m_DefaultSprite;
		m_GlitchImage.color = new Color(1f, 1f, 1f, 0f);
		if ((bool)m_MainImage)
		{
			m_MainImage.material = null;
		}
	}

	private IEnumerator GlitchCo()
	{
		float currentTime = 0f;
		m_GlitchImage.color = new Color(1f, 1f, 1f, 1f);
		while (currentTime < m_FadeTime)
		{
			currentTime += Time.unscaledDeltaTime;
			float alpha = currentTime / m_FadeTime;
			m_ShowCanvasGroup.alpha = alpha;
			yield return null;
		}
		if ((bool)m_MainImage)
		{
			m_MainImage.material = m_GlitchMaterial;
		}
		yield return new WaitForSecondsRealtime(m_DelayBeforeGlitch);
		m_GlitchMaterial.SetFloat(OffsetScale, m_GlitchStrength);
		currentTime = 0f;
		while (true)
		{
			float timeToWait = UnityEngine.Random.Range(0.3f * m_GlitchStep, 1.7f * m_GlitchStep);
			float value = UnityEngine.Random.Range(0.5f, 2.5f);
			m_GlitchMaterial.SetFloat(OffsetLinesFrequency, value);
			yield return new WaitForSecondsRealtime(timeToWait);
			currentTime += timeToWait;
			if (currentTime < m_GlitchTime)
			{
				m_OnComplete?.Invoke();
			}
		}
	}
}

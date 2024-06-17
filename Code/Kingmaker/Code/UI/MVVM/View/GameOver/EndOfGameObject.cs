using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public class EndOfGameObject : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Material m_Material;

	[SerializeField]
	private Image m_Image;

	[Header("Timing")]
	[SerializeField]
	private float m_DelayBeforeStart = 2f;

	[SerializeField]
	private float m_FadeOut = 2f;

	[SerializeField]
	private float m_DelayBeforeText = 0.5f;

	[SerializeField]
	private float m_AppearText = 2f;

	[SerializeField]
	private float m_FadeIn = 2f;

	[Header("EaseFunctions")]
	[SerializeField]
	private AnimationCurve m_FadeOutEase;

	[SerializeField]
	private AnimationCurve m_FadeInEase;

	private Sequence m_Sequence;

	private bool m_InGameSeqEnd;

	private bool m_MainMenuSeqBegin;

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		OnShow();
	}

	private void OnShow()
	{
		m_InGameSeqEnd = false;
		m_MainMenuSeqBegin = false;
		ShowInGameSequence();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (m_InGameSeqEnd && !m_MainMenuSeqBegin && RootUIContext.Instance.IsMainMenu)
		{
			ShowMainMenuSequence();
		}
	}

	private void ShowInGameSequence()
	{
		m_CanvasGroup.alpha = 0f;
		Vector4 min = new Vector4(-1f, 0f, 0f, 0f);
		m_Material.SetVector("_Min", min);
		m_Sequence = DOTween.Sequence();
		m_Sequence.AppendInterval(m_DelayBeforeStart);
		m_Sequence.Append(m_CanvasGroup.DOFade(1f, m_FadeOut).SetEase(m_FadeOutEase));
		m_Sequence.AppendInterval(m_DelayBeforeText);
		m_Sequence.Append(DOTween.To(() => min, SetMinToMaterial, Vector4.zero, m_AppearText));
		m_Sequence.OnComplete(delegate
		{
			Game.Instance.ResetToMainMenu();
			m_InGameSeqEnd = true;
		});
		void SetMinToMaterial(Vector4 newVector)
		{
			min = newVector;
			m_Material.SetVector("_Min", min);
		}
	}

	private void ShowMainMenuSequence()
	{
		m_MainMenuSeqBegin = true;
		m_CanvasGroup.alpha = 1f;
		m_Sequence = DOTween.Sequence();
		m_Sequence.AppendInterval(1f);
		m_Sequence.Append(m_CanvasGroup.DOFade(0f, m_FadeIn).SetEase(m_FadeInEase));
		m_Sequence.OnComplete(Hide);
	}
}

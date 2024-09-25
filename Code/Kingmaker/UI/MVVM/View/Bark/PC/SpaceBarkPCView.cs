using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Bark.PC;

public class SpaceBarkPCView : ViewBase<SpaceBarkVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_UnitName;

	[SerializeField]
	private Image m_UnitPortrait;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Vector2 m_StartPos;

	[SerializeField]
	private Vector2 m_MidPos;

	[SerializeField]
	private Vector2 m_EndPos;

	[SerializeField]
	private float m_AppearTime;

	[SerializeField]
	private float m_HalfDisappearTime;

	[SerializeField]
	private float m_HoldTime;

	[SerializeField]
	private float m_DisappearTime;

	private readonly List<Tweener> m_StartedTweeners = new List<Tweener>();

	public void Initialize()
	{
		PlayAppearAnimation();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Text.Subscribe(delegate(string val)
		{
			m_Text.text = val;
		}));
		AddDisposable(base.ViewModel.UnitName.Subscribe(delegate(string val)
		{
			m_UnitName.text = val;
		}));
		AddDisposable(base.ViewModel.UnitPortrait.Subscribe(delegate(Sprite val)
		{
			m_UnitPortrait.sprite = val;
		}));
		AddDisposable(base.ViewModel.HideCommand.Subscribe(delegate
		{
			HideAnimated();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		StopAllCoroutines();
		m_StartedTweeners.ForEach(delegate(Tweener t)
		{
			t.Kill();
		});
		m_StartedTweeners.Clear();
	}

	private void PlayAppearAnimation()
	{
		m_StartedTweeners.Add(m_Container.DOAnchorPos(m_MidPos, m_AppearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(1f, m_AppearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}

	private void HideAnimated()
	{
		StopAllCoroutines();
		StartCoroutine(PlayHideAnimationCoroutine());
	}

	private IEnumerator PlayHideAnimationCoroutine()
	{
		PlayHalfDisappearAnimation();
		yield return new WaitForSeconds(m_HoldTime);
		PlayDisappearAnimation();
		yield return new WaitForSeconds(m_DisappearTime);
		base.gameObject.SetActive(value: false);
	}

	private void PlayHalfDisappearAnimation()
	{
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(0.5f, m_HalfDisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}

	private void PlayDisappearAnimation()
	{
		m_StartedTweeners.Add(m_Container.DOAnchorPos(m_EndPos, m_DisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(0f, m_DisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}
}

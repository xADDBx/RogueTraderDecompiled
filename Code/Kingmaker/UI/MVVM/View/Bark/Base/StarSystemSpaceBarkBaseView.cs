using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Bark.Base;

public class StarSystemSpaceBarkBaseView : ViewBase<StarSystemSpaceBarkVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_LinkText;

	[SerializeField]
	private OwlcatButton m_OpenEncyclopediaButton;

	[SerializeField]
	private Image m_UnitPortrait;

	[SerializeField]
	private RectTransform m_MessageContainer;

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

	private IDisposable m_ShowLastMessageHandle;

	private bool m_IsLastMessageShown;

	public void Initialize()
	{
		m_LinkText.text = UIStrings.Instance.EncyclopediaTexts.SeeInEncyclopedia;
		base.gameObject.SetActive(value: true);
		ShowAnimated();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Text.Subscribe(delegate(string val)
		{
			m_Text.text = val;
		}));
		AddDisposable(base.ViewModel.UnitPortrait.Subscribe(SetPortrait));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.HideCommand, delegate
		{
			HideAnimated();
		}));
		AddDisposable(m_LinkText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		AddDisposable(ObservableExtensions.Subscribe(m_OpenEncyclopediaButton.OnLeftClickAsObservable(), delegate
		{
			OpenEncyclopedia();
		}));
		AddDisposable(base.ViewModel.EncyclopediaLink.Subscribe(SetLinkVisibility));
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

	public void AddInput(InputLayer inputLayer)
	{
		AddSystemMapInputImpl(inputLayer);
	}

	protected virtual void AddSystemMapInputImpl(InputLayer inputLayer)
	{
	}

	private void OpenEncyclopedia()
	{
		EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
		{
			x.HandleEncyclopediaPage(base.ViewModel.EncyclopediaLink.Value);
		});
	}

	private void SetPortrait(Sprite sprite)
	{
		m_UnitPortrait.enabled = sprite != null;
		if (sprite != null)
		{
			m_UnitPortrait.sprite = sprite;
		}
	}

	private void SetLinkVisibility(string link)
	{
		m_LinkText.gameObject.SetActive(!string.IsNullOrEmpty(link));
	}

	protected void ShowLastMessage()
	{
		if (!m_IsLastMessageShown)
		{
			ShowAnimated();
		}
		m_ShowLastMessageHandle?.Dispose();
		m_ShowLastMessageHandle = DelayedInvoker.InvokeInTime(delegate
		{
			HideAnimated();
			m_IsLastMessageShown = false;
			m_ShowLastMessageHandle = null;
		}, 5f);
	}

	private void ShowAnimated()
	{
		PlayAppearAnimation();
		ShowAnimatedImpl();
	}

	protected virtual void ShowAnimatedImpl()
	{
	}

	private void PlayAppearAnimation()
	{
		m_CanvasGroup.interactable = true;
		m_CanvasGroup.blocksRaycasts = true;
		m_StartedTweeners.Add(m_MessageContainer.DOAnchorPos(m_MidPos, m_AppearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
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
		ResetAnimation();
	}

	private void PlayHalfDisappearAnimation()
	{
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(0.5f, m_HalfDisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}

	private void PlayDisappearAnimation()
	{
		m_StartedTweeners.Add(m_MessageContainer.DOAnchorPos(m_EndPos, m_DisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(0f, m_DisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		m_CanvasGroup.interactable = false;
		m_CanvasGroup.blocksRaycasts = false;
	}

	private void ResetAnimation()
	{
		m_MessageContainer.anchoredPosition = m_StartPos;
		m_CanvasGroup.interactable = false;
		m_CanvasGroup.blocksRaycasts = false;
		ResetAnimationImpl();
	}

	protected virtual void ResetAnimationImpl()
	{
	}
}

using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;

public abstract class BaseCareerPathSelectionTabPCView<TViewModel> : BaseCareerPathSelectionTabCommonView<TViewModel>, ICareerPathSelectionTabPCView, ICareerPathSelectionTabView where TViewModel : class, IViewModel
{
	[Header("Button block")]
	[SerializeField]
	private bool m_ButtonsSetFromParent = true;

	[Header("Title")]
	[SerializeField]
	private CanvasGroup m_TitleHighlightGroup;

	[SerializeField]
	private CanvasGroup m_CompleteHighlightGroup;

	[SerializeField]
	protected OwlcatMultiButton m_HighlightButton;

	[SerializeField]
	[ConditionalHide("m_ButtonsSetFromParent")]
	private CareerButtonsBlock m_ButtonsBlock;

	protected readonly ReactiveProperty<string> HintText = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_NextButtonInteractable = new ReactiveProperty<bool>(initialValue: true);

	private readonly ReactiveProperty<bool> m_BackButtonInteractable = new ReactiveProperty<bool>(initialValue: true);

	public bool CanCommit { get; protected set; }

	public void SetButtonsBlock(CareerButtonsBlock buttonsBlock)
	{
		m_ButtonsBlock = buttonsBlock;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (!(m_ButtonsBlock == null))
		{
			AddDisposable(m_ButtonsBlock.NextButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				HandleClickNext();
			}));
			AddDisposable(m_ButtonsBlock.NextButton.SetHint(HintText));
			AddDisposable(m_ButtonsBlock.FinishButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				HandleClickFinish();
			}));
			AddDisposable(m_NextButtonInteractable.Subscribe(m_ButtonsBlock.NextButton.SetInteractable));
			AddDisposable(m_BackButtonInteractable.Subscribe(m_ButtonsBlock.BackButton.SetInteractable));
			AddDisposable(m_ButtonsBlock.BackButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				HandleClickBack();
			}));
			AddDisposable(NextButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.NextButtonLabel.text = value;
			}));
			AddDisposable(BackButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.BackButtonLabel.text = value;
			}));
			AddDisposable(FinishButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.FinishButtonLabel.text = value;
			}));
			m_HighlightButton.Or(null)?.OnLeftClickAsObservable().Subscribe(delegate
			{
				OnHighlightButtonClick();
			});
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HighlightButton.Or(null)?.gameObject.SetActive(value: false);
	}

	protected void SetNextButtonInteractable(bool value)
	{
		m_NextButtonInteractable.Value = value;
	}

	protected void SetBackButtonInteractable(bool value)
	{
		m_BackButtonInteractable.Value = value;
	}

	protected void SetButtonVisibility(bool value)
	{
		m_ButtonsBlock.Or(null)?.gameObject.SetActive(value);
	}

	protected void SetFinishInteractable(bool value)
	{
		if ((bool)m_ButtonsBlock)
		{
			m_ButtonsBlock.FinishButton.Interactable = value;
		}
	}

	protected void SetButtonSound(UISounds.ButtonSoundsEnum soundType)
	{
		if ((bool)m_ButtonsBlock)
		{
			UISounds.Instance.SetClickSound(m_ButtonsBlock.NextButton, soundType);
		}
	}

	private void OnHighlightButtonClick()
	{
		if ((bool)m_TitleHighlightGroup)
		{
			m_TitleHighlightGroup.DOKill();
			m_CompleteHighlightGroup.DOKill();
			CanvasGroup highlightGroup = (CanCommit ? m_CompleteHighlightGroup : m_TitleHighlightGroup);
			highlightGroup.DOFade(1f, 0.1f).SetLoops(4, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
				.OnComplete(delegate
				{
					highlightGroup.alpha = 0f;
				});
		}
	}

	void ICareerPathSelectionTabView.Unbind()
	{
		Unbind();
	}
}

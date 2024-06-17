using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
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

	[SerializeField]
	[ConditionalHide("m_ButtonsSetFromParent")]
	private CareerButtonsBlock m_ButtonsBlock;

	protected ReactiveProperty<string> HintText = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsButtonInteractable = new ReactiveProperty<bool>(initialValue: true);

	private readonly ReactiveProperty<bool> m_HasFirstSelectable = new ReactiveProperty<bool>(initialValue: true);

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
			AddDisposable(m_ButtonsBlock.FirstSelectableButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				HandleFirstSelectableClick();
			}));
			AddDisposable(NextButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.NextButtonLabel.text = value;
			}));
			AddDisposable(m_IsButtonInteractable.Subscribe(m_ButtonsBlock.NextButton.SetInteractable));
			AddDisposable(m_HasFirstSelectable.Subscribe(m_ButtonsBlock.FirstSelectableButton.gameObject.SetActive));
			AddDisposable(m_ButtonsBlock.BackButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				HandleClickBack();
			}));
			AddDisposable(BackButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.BackButtonLabel.text = value;
			}));
		}
	}

	protected void SetButtonInteractable(bool value)
	{
		m_IsButtonInteractable.Value = value;
	}

	protected void SetButtonVisibility(bool value)
	{
		m_ButtonsBlock.Or(null)?.gameObject.SetActive(value);
	}

	protected void SetFirstSelectableInteractable(bool value)
	{
		if ((bool)m_ButtonsBlock)
		{
			m_ButtonsBlock.FirstSelectableButton.Interactable = value;
		}
	}

	protected void SetFirstSelectableVisibility(bool value)
	{
		m_HasFirstSelectable.Value = value;
	}

	protected void SetButtonSound(UISounds.ButtonSoundsEnum soundType)
	{
		if ((bool)m_ButtonsBlock)
		{
			UISounds.Instance.SetClickSound(m_ButtonsBlock.NextButton, soundType);
		}
	}

	void ICareerPathSelectionTabView.Unbind()
	{
		Unbind();
	}
}

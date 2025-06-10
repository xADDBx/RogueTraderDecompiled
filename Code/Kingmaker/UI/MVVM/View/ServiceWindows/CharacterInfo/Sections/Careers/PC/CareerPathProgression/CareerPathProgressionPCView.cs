using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.CharGen.PC.Phases;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression;

public class CareerPathProgressionPCView : CareerPathProgressionCommonView
{
	[Header("Inside Buttons")]
	[SerializeField]
	private OwlcatButton m_ReturnButton;

	[SerializeField]
	private TextMeshProUGUI m_ReturnLabel;

	[Header("ExpandButtons")]
	[ConditionalShow("m_CanMove")]
	[SerializeField]
	private OwlcatMultiButton m_StatsButton;

	[ConditionalShow("m_CanMove")]
	[SerializeField]
	private OwlcatMultiButton m_TooltipButton;

	[Header("OutsideButtons")]
	[SerializeField]
	private bool m_HasButtons;

	[SerializeField]
	[ConditionalShow("m_HasButtons")]
	private CareerButtonsBlock m_ButtonsBlock;

	[ConditionalShow("m_CanMove")]
	[SerializeField]
	private OwlcatMultiButton m_ToTooltipsButton;

	[ConditionalShow("m_CanMove")]
	[SerializeField]
	private OwlcatMultiButton m_ToStatsButton;

	[SerializeField]
	private CharGenChangeNameMessageBoxPCView m_PetChangeNameMessageBox;

	public override void Initialize(Action<bool> returnAction)
	{
		if (m_HasButtons)
		{
			(m_CareerPathSelectionPartCommonView as CareerPathSelectionTabsPCView)?.SetButtonsBlock(m_ButtonsBlock);
			m_ButtonsBlock.SetActive(state: false);
		}
		m_PetChangeNameMessageBox?.Initialize();
		base.Initialize(returnAction);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ReturnButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleReturn();
		}));
		AddDisposable(base.ViewModel.OnCommit.Subscribe(delegate
		{
			UpdateButtonsState();
		}));
		AddDisposable(base.ViewModel.PetChangeNameVM.Subscribe(delegate(CharGenChangeNameMessageBoxVM value)
		{
			m_PetChangeNameMessageBox.Or(null)?.Bind(value);
		}));
		if (m_CanMove)
		{
			AddDisposable(m_StatsButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				SwitchDescriptionShowed(false);
			}));
			AddDisposable(m_TooltipButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				SwitchDescriptionShowed(true);
			}));
			AddDisposable(m_ToStatsButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				SwitchDescriptionShowed(false);
			}));
			AddDisposable(m_ToTooltipsButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				SwitchDescriptionShowed(true);
			}));
			AddDisposable(base.ViewModel.IsDescriptionShowed.Subscribe(delegate(bool value)
			{
				m_ToTooltipsButton.gameObject.SetActive(!value);
				m_ToStatsButton.gameObject.SetActive(value);
			}));
		}
		else
		{
			m_ToTooltipsButton.Or(null)?.gameObject.SetActive(value: false);
			m_ToStatsButton.Or(null)?.gameObject.SetActive(value: false);
		}
		m_ReturnLabel.text = UIStrings.Instance.CharacterSheet.BackToCareersList;
		m_AttentionSign.SetHint(UIStrings.Instance.CharacterSheet.AlreadyInLevelUp);
		UpdateButtonsState();
		TextHelper.AppendTexts(m_ReturnLabel);
		UISounds.Instance.SetClickAndHoverSound(m_StatsButton, UISounds.ButtonSoundsEnum.NoSound);
		UISounds.Instance.SetClickAndHoverSound(m_TooltipButton, UISounds.ButtonSoundsEnum.NoSound);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ButtonsBlock.Or(null)?.SetActive(state: false);
	}

	private void UpdateButtonsState()
	{
		m_ButtonsBlock.Or(null)?.SetActive(base.ViewModel.IsInLevelupProcess);
	}
}

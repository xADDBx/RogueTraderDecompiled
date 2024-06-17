using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NewGame.Common;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class TempNewGamePCView : TempNewGameCommonView
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_NextButton;

	[SerializeField]
	private OwlcatButton m_BackButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_TitleLabel;

	[SerializeField]
	private TextMeshProUGUI m_NextButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_BackButtonLabel;

	protected override void BindViewImplementation()
	{
		m_TitleLabel.text = UIStrings.Instance.NewGameWin.MenuTitleNewGame;
		m_NextButtonLabel.text = UIStrings.Instance.CharGen.Next;
		m_BackButtonLabel.text = UIStrings.Instance.CharGen.Back;
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnBack));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBack();
		}));
		AddDisposable(m_BackButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBack();
		}));
		AddDisposable(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnNext();
		}));
		AddDisposable(m_CloseButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBack();
		}));
		AddDisposable(m_BackButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBack();
		}));
		AddDisposable(m_NextButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnNext();
		}));
		base.BindViewImplementation();
	}

	protected override void BuildNavigation()
	{
		NavigationBehaviour = new GridConsoleNavigationBehaviour();
		NavigationBehaviour.AddRow(new List<IConsoleEntity> { m_CloseButton });
		NavigationBehaviour.AddRow(new List<IConsoleEntity> { m_PregenSelectorView, m_SettingsEntityDropdownGameDifficultyViewPrefab });
		NavigationBehaviour.AddRow(new List<IConsoleEntity> { m_BackButton, m_NextButton });
		NavigationBehaviour.FocusOnFirstValidEntity();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(OnConfirmClick, 8));
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		IConsoleEntity currentEntity = NavigationBehaviour.CurrentEntity;
		if (!(currentEntity is TempPregenSelectorPCView))
		{
			if (currentEntity is SettingsEntityDropdownGameDifficultyPCView)
			{
				NavigationBehaviour.FocusOnEntityManual(m_NextButton);
			}
			else
			{
				(NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
			}
		}
		else
		{
			NavigationBehaviour.FocusOnEntityManual(m_SettingsEntityDropdownGameDifficultyViewPrefab);
		}
	}
}

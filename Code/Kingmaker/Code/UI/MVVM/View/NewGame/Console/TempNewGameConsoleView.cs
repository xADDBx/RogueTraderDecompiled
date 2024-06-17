using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NewGame.Common;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class TempNewGameConsoleView : TempNewGameCommonView
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	protected override void BuildNavigation()
	{
		NavigationBehaviour = new GridConsoleNavigationBehaviour();
		NavigationBehaviour.AddRow(new List<IConsoleEntity> { m_PregenSelectorView, m_SettingsEntityDropdownGameDifficultyViewPrefab });
		NavigationBehaviour.FocusOnFirstValidEntity();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_HintsWidget.BindHint(inputLayer.AddButton(OnConfirmClick, 8), UIStrings.Instance.CharGen.Next, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_HintsWidget.BindHint(inputLayer.AddButton(OnDeclineClick, 9), UIStrings.Instance.CharGen.Back, ConsoleHintsWidget.HintPosition.Left));
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		IConsoleEntity currentEntity = NavigationBehaviour.CurrentEntity;
		if (!(currentEntity is TempPregenSelectorCommonView))
		{
			if (currentEntity is SettingsEntityDropdownGameDifficultyPCView)
			{
				base.ViewModel.OnNext();
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

	private void OnDeclineClick(InputActionEventData obj)
	{
		IConsoleEntity currentEntity = NavigationBehaviour.CurrentEntity;
		if (!(currentEntity is TempPregenSelectorCommonView))
		{
			if (currentEntity is SettingsEntityDropdownGameDifficultyPCView)
			{
				NavigationBehaviour.FocusOnEntityManual(m_PregenSelectorView);
			}
			else
			{
				(NavigationBehaviour.CurrentEntity as IDeclineClickHandler)?.OnDeclineClick();
			}
		}
		else
		{
			base.ViewModel.OnBack();
		}
	}
}

using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGamePhaseDifficultyConsoleView : NewGamePhaseDifficultyBaseView
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderConsoleView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolConsoleView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownConsoleView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderConsoleView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyConsoleView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntityBoolOnlyOneSaveConsoleView m_SettingsEntityBoolOnlyOneSaveViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
		}
	}

	[SerializeField]
	private SettingsViews m_SettingsViews;

	public void Initialize()
	{
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.SettingEntities));
		base.BindViewImplementation();
	}

	public void UpdateFirstFocus()
	{
		GridConsoleNavigationBehaviour navigationBehaviour = m_VirtualList.GetNavigationBehaviour();
		if (navigationBehaviour != null)
		{
			IConsoleEntity consoleEntity = navigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => !((e as VirtualListElement)?.ConsoleEntityProxy is SettingsEntityHeaderConsoleView));
			if (consoleEntity != null)
			{
				navigationBehaviour.FocusOnEntityManual(consoleEntity);
			}
			else
			{
				navigationBehaviour.FocusOnFirstValidEntity();
			}
		}
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * base.InfoView.ScrollRectExtended.scrollSensitivity);
		base.InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenDefaultSettingsDialog();
		}, 11, base.ViewModel.IsEnabled.And(base.ViewModel.IsDefaultButtonInteractable).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.SettingsUI.ResetToDefaultHold));
	}
}

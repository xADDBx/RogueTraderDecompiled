using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Base;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Console;
using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Console;

public class DlcManagerConsoleView : DlcManagerBaseView
{
	[Header("Views")]
	[SerializeField]
	private DlcManagerTabDlcsConsoleView m_DlcManagerTabDlcsConsoleView;

	[SerializeField]
	private DlcManagerTabModsConsoleView m_DlcManagerTabModsConsoleView;

	[SerializeField]
	private DlcManagerTabSwitchOnDlcsConsoleView m_DlcManagerTabSwitchOnDlcsConsoleView;

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_ApplyHintHint;

	[SerializeField]
	private ConsoleHint m_DefaultHint;

	[SerializeField]
	private ConsoleHint m_OpenModSettingsHint;

	[SerializeField]
	private ConsoleHint m_InstallDlcHint;

	[SerializeField]
	private ConsoleHint m_DeleteDlcHint;

	[SerializeField]
	private ConsoleHint m_PlayPauseVideoHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	private readonly BoolReactiveProperty m_ModSettingsIsAvailable = new BoolReactiveProperty();

	protected override void InitializeImpl()
	{
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Initialize();
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Initialize();
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Disposable = new CompositeDisposable();
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Bind(base.ViewModel.DlcsVM);
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Bind(base.ViewModel.SwitchOnDlcsVM);
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Bind(base.ViewModel.ModsVM);
		}
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateInput();
		UpdateNavigation();
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ChangeTab, delegate
		{
			UpdateNavigation();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Disposable?.Clear();
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	private void UpdateNavigation()
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		if (base.ViewModel.SelectedMenuEntity.Value.DlcManagerTabVM == base.ViewModel.DlcsVM && !base.ViewModel.InGame)
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_DlcManagerTabDlcsConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DlcManagerTabDlcsConsoleView.ScrollToTop();
			m_Disposable?.Clear();
			m_Disposable?.Add(m_NavigationBehaviour.Focus.Subscribe(m_DlcManagerTabDlcsConsoleView.ScrollList));
		}
		else if (base.ViewModel.SelectedMenuEntity.Value.DlcManagerTabVM == base.ViewModel.ModsVM && !base.ViewModel.IsConsole)
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_DlcManagerTabModsConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DlcManagerTabModsConsoleView.ScrollToTop();
			m_Disposable?.Clear();
			m_Disposable?.Add(m_NavigationBehaviour.Focus.Subscribe(m_DlcManagerTabModsConsoleView.ScrollList));
		}
		else if (base.ViewModel.SelectedMenuEntity.Value.DlcManagerTabVM == base.ViewModel.SwitchOnDlcsVM && base.ViewModel.InGame)
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_DlcManagerTabSwitchOnDlcsConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DlcManagerTabSwitchOnDlcsConsoleView.ScrollToTop();
			m_Disposable?.Clear();
			m_Disposable?.Add(m_NavigationBehaviour.Focus.Subscribe(m_DlcManagerTabSwitchOnDlcsConsoleView.ScrollList));
		}
		m_InputLayer.Bind();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "DlcManager"
		});
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_InputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(m_DeclineHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9)));
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		if (!base.ViewModel.IsConsole)
		{
			AddDisposable(m_PrevHint.Bind(inputLayer.AddButton(delegate
			{
				m_Selector.OnPrev();
			}, 14)));
			AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
			{
				m_Selector.OnNext();
			}, 15)));
			if (base.ViewModel.InGame)
			{
				AddDisposable(m_ApplyHintHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.CheckToReloadGame(null);
				}, 8, base.ViewModel.IsModsWindow.Or(base.ViewModel.IsSwitchOnDlcsWindow).And(base.ViewModel.SwitchOnDlcsVM?.NeedResave.Or(base.ViewModel.ModsVM?.NeedReload)).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
				AddDisposable(m_DefaultHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.RestoreAllToPreviousState();
				}, 11, base.ViewModel.IsModsWindow.Or(base.ViewModel.IsSwitchOnDlcsWindow).And(base.ViewModel.SwitchOnDlcsVM?.NeedResave.Or(base.ViewModel.ModsVM?.NeedReload)).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
			}
			else
			{
				AddDisposable(m_ApplyHintHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.CheckToReloadGame(null);
				}, 8, base.ViewModel.IsModsWindow.And(base.ViewModel.ModsVM.NeedReload).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
				AddDisposable(m_DefaultHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.RestoreAllToPreviousState();
				}, 11, base.ViewModel.IsModsWindow.And(base.ViewModel.ModsVM.NeedReload).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
			}
			m_ApplyHintHint.SetLabel(UIStrings.Instance.SettingsUI.Apply);
			m_DefaultHint.SetLabel(UIStrings.Instance.SettingsUI.Default);
		}
		else if (base.ViewModel.IsConsole && base.ViewModel.InGame)
		{
			AddDisposable(m_ApplyHintHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.CheckToReloadGame(null);
			}, 8, base.ViewModel.IsSwitchOnDlcsWindow.And(base.ViewModel.SwitchOnDlcsVM?.NeedResave).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
			AddDisposable(m_DefaultHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.RestoreAllToPreviousState();
			}, 11, base.ViewModel.IsSwitchOnDlcsWindow.And(base.ViewModel.SwitchOnDlcsVM?.NeedResave).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
			m_ApplyHintHint.SetLabel(UIStrings.Instance.SettingsUI.Apply);
			m_DefaultHint.SetLabel(UIStrings.Instance.SettingsUI.Default);
		}
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget, m_PurchaseHint, m_InstallDlcHint, m_DeleteDlcHint, m_PlayPauseVideoHint);
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget, m_OpenModSettingsHint, m_ModSettingsIsAvailable);
		}
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		if (m_NavigationBehaviour.Entities.Any())
		{
			if (entity is DlcManagerDlcEntityConsoleView dlcManagerDlcEntityConsoleView)
			{
				m_DeleteDlcHint.gameObject.SetActive(dlcManagerDlcEntityConsoleView.IsDlcCanBeDeleted);
			}
			if (entity is DlcManagerModEntityConsoleView dlcManagerModEntityConsoleView && !base.ViewModel.IsConsole)
			{
				m_ModSettingsIsAvailable.Value = dlcManagerModEntityConsoleView.GetAvailableSettings();
			}
		}
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		if (base.ViewModel.SelectedMenuEntity.Value.DlcManagerTabVM == base.ViewModel.DlcsVM && !base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Scroll(obj, value);
		}
		else if (base.ViewModel.SelectedMenuEntity.Value.DlcManagerTabVM == base.ViewModel.ModsVM && !base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Scroll(obj, value);
		}
		else if (base.ViewModel.SelectedMenuEntity.Value.DlcManagerTabVM == base.ViewModel.SwitchOnDlcsVM && base.ViewModel.InGame)
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Scroll(obj, value);
		}
	}
}

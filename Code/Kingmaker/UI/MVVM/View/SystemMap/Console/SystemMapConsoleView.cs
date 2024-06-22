using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Bark.Console;
using Kingmaker.UI.MVVM.View.CombatLog.Console;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMap.Console;

public class SystemMapConsoleView : SystemMapView, IExplorationUIHandler, ISubscriber, IVendorUIHandler, ISubscriber<IMechanicEntity>, IDialogInteractionHandler, IFullScreenUIHandler
{
	[SerializeField]
	private SystemMapOvertipsConsoleView m_SystemMapOvertipsConsoleView;

	[SerializeField]
	private StarSystemSpaceBarksHolderConsoleView m_StarSystemSpaceBarksHolderConsoleView;

	[SerializeField]
	private CombatLogConsoleView m_CombatLogConsoleView;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_InteractHint;

	[SerializeField]
	private ConsoleHint m_ToggleTooltipHint;

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	private IInteractableSystemMapOvertip m_CurrentOvertip;

	private bool m_ShowTooltip;

	private readonly ReactiveProperty<bool> m_CanShowTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NavigationEnabled = new ReactiveProperty<bool>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		m_StarSystemSpaceBarksHolderConsoleView.Bind(base.ViewModel.StarSystemSpaceBarksHolderVM);
	}

	protected override void DestroyViewImplementation()
	{
		m_ShowTooltip = false;
		base.DestroyViewImplementation();
	}

	public void AddSystemMapInput(InputLayer inputLayer)
	{
		AddDisposable(m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters));
		CreateNavigation();
		CreateInput(inputLayer);
		m_StarSystemSpaceBarksHolderConsoleView.AddInput(inputLayer);
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
	}

	public void SwitchCursor(bool cursorEnabled)
	{
		if (cursorEnabled)
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
			OnClearOvertips();
		}
		else
		{
			OnDisableCursor();
		}
		m_NavigationEnabled.Value = !cursorEnabled;
	}

	private void OnDisableCursor()
	{
		foreach (IFloatConsoleNavigationEntity item in m_SystemMapOvertipsConsoleView.SystemMapObjectsCollection)
		{
			OnAddOvertip(item);
			if (item is ISystemMapOvertip systemMapOvertip)
			{
				systemMapOvertip.UnfocusButton();
			}
		}
	}

	private void CreateNavigation()
	{
		m_SystemMapOvertipsConsoleView.SystemMapObjectsCollection.ForEach(OnAddOvertip);
		AddDisposable(m_SystemMapOvertipsConsoleView.SystemMapObjectsCollection.ObserveAdd().Subscribe(delegate(CollectionAddEvent<IFloatConsoleNavigationEntity> value)
		{
			OnAddOvertip(value.Value);
		}));
		AddDisposable(m_SystemMapOvertipsConsoleView.SystemMapObjectsCollection.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<IFloatConsoleNavigationEntity> value)
		{
			OnRemoveOvertip(value.Value);
		}));
		AddDisposable(m_SystemMapOvertipsConsoleView.SystemMapObjectsCollection.ObserveReset().Subscribe(OnClearOvertips));
		m_NavigationEnabled.Value = true;
	}

	private void CreateInput(InputLayer inputLayer)
	{
		m_NavigationBehaviour.GetInputLayer(inputLayer, m_NavigationEnabled);
		m_CombatLogConsoleView.AddInput(inputLayer);
		AddDisposable(m_InteractHint.Bind(inputLayer.AddButton(delegate
		{
			InteractOvertip();
		}, 8, base.ViewModel.IsControllable)));
		m_InteractHint.SetLabel(UIStrings.Instance.ActionTexts.Move);
		AddDisposable(inputLayer.AddButton(delegate
		{
			InteractOvertip();
		}, 8, base.ViewModel.IsControllable.Not().ToReactiveProperty()));
		AddDisposable(m_ToggleTooltipHint.Bind(inputLayer.AddButton(ToggleTooltip, 19, m_CanShowTooltip, InputActionEventType.ButtonJustReleased)));
		m_ToggleTooltipHint.SetLabel(UIStrings.Instance.CommonTexts.Information);
	}

	private void OnAddOvertip(IFloatConsoleNavigationEntity overtip)
	{
		m_NavigationBehaviour.AddEntity(overtip);
	}

	private void OnRemoveOvertip(IFloatConsoleNavigationEntity overtip)
	{
		m_NavigationBehaviour.RemoveEntity(overtip);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		if (m_NavigationBehaviour.Entities.Any())
		{
			if (!(entity is IInteractableSystemMapOvertip interactableSystemMapOvertip))
			{
				m_CurrentOvertip = null;
				m_CanShowTooltip.Value = false;
				return;
			}
			m_CurrentOvertip = interactableSystemMapOvertip;
			bool flag = interactableSystemMapOvertip.CanShowTooltip();
			m_CanShowTooltip.Value = flag;
			m_ShowTooltip &= flag;
			ShowTooltip(interactableSystemMapOvertip);
		}
	}

	private void OnClearOvertips()
	{
		m_NavigationBehaviour.Clear();
		m_CurrentOvertip = null;
	}

	private void ShowTooltip(IInteractableSystemMapOvertip overtip)
	{
		if (m_ShowTooltip)
		{
			overtip.ShowTooltip();
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void InteractOvertip()
	{
		if (m_NavigationEnabled.Value)
		{
			m_CurrentOvertip?.Interact();
		}
	}

	private void HideFocused()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		TooltipHelper.HideTooltip();
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		HideFocused();
	}

	public void CloseExplorationScreen()
	{
	}

	public void HandleTradeStarted()
	{
		HideFocused();
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		HideFocused();
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state)
		{
			HideFocused();
		}
	}
}

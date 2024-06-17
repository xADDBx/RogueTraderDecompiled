using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorSelectingWindowConsoleView : VendorSelectingWindowBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private VendorInfoFactionReputationItemConsoleView m_CurrentSelectedFaction;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigation();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(delegate(IConsoleEntity value)
		{
			HandleTooltip(value);
		}));
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		HandleTooltip(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Vendor Selecting Window Console View"
		});
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnCloseClick();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			m_CurrentSelectedFaction.TryTrade();
		}, 8, m_CanConfirm), UIStrings.Instance.Vendor.Trade));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		if (entity == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	public void CreateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		GridConsoleNavigationBehaviour entity = new GridConsoleNavigationBehaviour();
		foreach (IWidgetView visibleEntry in m_WidgetList.VisibleEntries)
		{
			if (visibleEntry is VendorInfoFactionReputationItemConsoleView vendorInfoFactionReputationItemConsoleView)
			{
				m_NavigationBehaviour.AddEntityHorizontal(vendorInfoFactionReputationItemConsoleView.GetNavigation());
			}
		}
		m_NavigationBehaviour.AddEntityGrid(entity);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		CreateInput();
	}

	protected override void Close()
	{
		base.Close();
		TooltipHelper.HideTooltip();
		m_ShowTooltip.Value = false;
		GamePad.Instance.PopLayer(m_InputLayer);
	}

	private void OnEntityFocused(IConsoleEntity currentFocus)
	{
		m_CurrentSelectedFaction = currentFocus as VendorInfoFactionReputationItemConsoleView;
		m_CanConfirm.Value = m_CurrentSelectedFaction != null && m_CurrentSelectedFaction.HasVendors;
	}
}

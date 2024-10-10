using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class ActionBarConvertedConsoleView : ViewBase<ActionBarConvertedVM>, IClickMechanicActionBarSlotHandler, ISubscriber
{
	[SerializeField]
	private ActionBarBaseSlotView m_SlotView;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	protected RectTransform m_TooltipPlace;

	[SerializeField]
	private int ColumnsCount = 2;

	private bool m_IsInit;

	private readonly List<ActionBarBaseSlotView> m_Slots = new List<ActionBarBaseSlotView>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private bool m_ShowTooltip;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private VisibilityController m_Visibility;

	private void Awake()
	{
		m_Visibility = VisibilityController.Control(base.gameObject);
		m_Visibility.SetVisible(visible: false);
	}

	protected override void BindViewImplementation()
	{
		TryFindConsoleHintWidget();
		CreateInput();
		foreach (ActionBarSlotVM slot in base.ViewModel.Slots)
		{
			ActionBarBaseSlotView widget = WidgetFactory.GetWidget(m_SlotView);
			widget.Initialize();
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Bind(slot);
			m_Slots.Add(widget);
		}
		AddDisposable(EventBus.Subscribe(this));
		SetConsoleEntities();
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		m_Visibility.SetVisible(visible: true);
	}

	protected override void DestroyViewImplementation()
	{
		m_Slots.ForEach(WidgetFactory.DisposeWidget);
		m_Slots.Clear();
		m_InputLayer = null;
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_ShowTooltip = false;
		m_Visibility.SetVisible(visible: false);
	}

	private void SetConsoleEntities()
	{
		m_NavigationBehaviour.Clear();
		List<IConsoleNavigationEntity> list = m_Slots.Select((ActionBarBaseSlotView x) => (IConsoleNavigationEntity)x).ToList();
		m_NavigationBehaviour.AddRow(list.GetRange(0, ColumnsCount));
		m_NavigationBehaviour.AddRow(list.GetRange(ColumnsCount, list.Count - ColumnsCount));
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(1, 0));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ActionBarConvertedConsoleView"
		});
		if (!(m_HintsWidget == null))
		{
			InputBindStruct inputBindStruct = m_InputLayer.AddButton(OnDecline, 9);
			AddDisposable(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Cancel, ConsoleHintsWidget.HintPosition.Left));
			AddDisposable(inputBindStruct);
			InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
			AddDisposable(m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left));
			AddDisposable(inputBindStruct2);
			AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
		}
	}

	private void OnDecline(InputActionEventData data)
	{
		base.ViewModel.Close();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip.Value = tooltipBaseTemplate != null;
		if (m_ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowTooltip(tooltipBaseTemplate, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2>
				{
					new Vector2(0.5f, 0f)
				}
			});
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

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		base.ViewModel.Close();
	}

	private void TryFindConsoleHintWidget()
	{
		if (!(m_HintsWidget != null))
		{
			ConsoleHintWidgetContainer componentInParent = GetComponentInParent<ConsoleHintWidgetContainer>();
			if ((bool)componentInParent)
			{
				m_HintsWidget = componentInParent.GetConsoleHintWidget();
			}
		}
	}
}

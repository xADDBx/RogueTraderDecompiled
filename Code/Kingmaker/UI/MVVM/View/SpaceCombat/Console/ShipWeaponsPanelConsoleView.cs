using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.View;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class ShipWeaponsPanelConsoleView : ViewBase<ShipWeaponsPanelVM>
{
	[Header("General")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_InspectHint;

	[SerializeField]
	private MonoBehaviour[] m_AnimatorObjects;

	[SerializeField]
	protected RectTransform m_TooltipPlace;

	[Header("Weapon Groups")]
	[SerializeField]
	private WeaponAbilitiesGroupConsoleView m_PortGroup;

	[SerializeField]
	private WeaponAbilitiesGroupConsoleView m_ProwGroup;

	[SerializeField]
	private WeaponAbilitiesGroupConsoleView m_DorsalGroup;

	[SerializeField]
	private WeaponAbilitiesGroupConsoleView m_StarboardGroup;

	[Header("Ability Groups")]
	[SerializeField]
	private AbilitiesGroupConsoleView m_AbilitiesGroup;

	[SerializeField]
	private AbilitiesGroupConsoleView m_SecondAbilitiesGroup;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private bool m_ShowTooltip;

	private readonly BoolReactiveProperty m_CanFunc02 = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsInspectVisible = new BoolReactiveProperty();

	private List<IUIAnimator> m_Animators;

	private readonly BoolReactiveProperty m_IsPlayerTurn = new BoolReactiveProperty();

	public void Initialize()
	{
		m_Animators = m_AnimatorObjects.SelectMany((MonoBehaviour o) => o.GetComponents<IUIAnimator>()).ToList();
	}

	protected override void BindViewImplementation()
	{
		CreateInput();
		m_PortGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Port]);
		m_ProwGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Prow]);
		m_DorsalGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Dorsal]);
		m_StarboardGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Starboard]);
		m_AbilitiesGroup.Bind(base.ViewModel.AbilitiesGroup);
		m_SecondAbilitiesGroup.Bind(base.ViewModel.AbilitiesGroup);
		AddDisposable(base.ViewModel.IsActive.Subscribe(OnActive));
		AddDisposable(base.ViewModel.IsPlayerTurn.Subscribe(delegate(bool val)
		{
			m_IsPlayerTurn.Value = val;
		}));
		AddDisposable(base.ViewModel.HighlightedUnit.Subscribe(OnHighlightedUnit));
	}

	protected override void DestroyViewImplementation()
	{
		m_ShowTooltip = false;
	}

	private void SetConsoleEntities()
	{
		if (base.ViewModel.IsTorpedoesTurn.Value)
		{
			SetTorpedoesEntities();
		}
		else
		{
			SetMainWeaponsEntities();
		}
	}

	private void SetMainWeaponsEntities()
	{
		List<IConsoleNavigationEntity> consoleEntities = m_AbilitiesGroup.GetConsoleEntities();
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		list.Add(consoleEntities[1]);
		list.AddRange(m_ProwGroup.GetConsoleEntities());
		list.Add(consoleEntities[0]);
		List<IConsoleNavigationEntity> list2 = new List<IConsoleNavigationEntity>();
		list2.AddRange(m_PortGroup.GetConsoleEntities());
		list2.AddRange(m_DorsalGroup.GetConsoleEntities());
		list2.AddRange(m_StarboardGroup.GetConsoleEntities());
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow(list);
		m_NavigationBehaviour.AddRow(list2);
	}

	private void SetTorpedoesEntities()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow(m_SecondAbilitiesGroup.GetConsoleEntities());
	}

	public void AddInput(InputLayer inputLayer)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(OnFunc01, 10, m_IsPlayerTurn);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.HUDTexts.WeaponsBar, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			OnInspectUnit();
		}, 19, m_IsInspectVisible, InputActionEventType.ButtonJustLongPressed);
		AddDisposable(m_InspectHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		m_InspectHint.SetLabel(UIStrings.Instance.MainMenu.Inspect);
	}

	public void OnHighlightedUnit(UnitEntityView unit)
	{
		m_IsInspectVisible.Value = unit != null;
	}

	private void OnInspectUnit()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.HighlightedUnit.Value.EntityData);
		});
	}

	private void OnFunc01(InputActionEventData data)
	{
		if (!(Game.Instance.CurrentMode != GameModeType.SpaceCombat) && (!Game.Instance.TurnController.TurnBasedModeActive || Game.Instance.TurnController.IsPlayerTurn))
		{
			base.ViewModel.IsActive.Value = true;
		}
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(1, 0));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ShipWeaponsPanelConsoleView"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(OnDecline, 9, base.ViewModel.IsActive);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Cancel, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(inputBindStruct);
		AddDisposable(m_InputLayer.AddButton(OnDecline, 10, base.ViewModel.IsActive));
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
		}, 11, m_CanFunc02, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Expand, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(inputBindStruct3);
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
	}

	private void OnActive(bool active)
	{
		if (active)
		{
			SetConsoleEntities();
			m_Animators.ForEach(delegate(IUIAnimator a)
			{
				a.AppearAnimation();
			});
			GamePad.Instance.PushLayer(m_InputLayer);
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}
		else
		{
			m_Animators.ForEach(delegate(IUIAnimator a)
			{
				a.DisappearAnimation();
			});
			GamePad.Instance.PopLayer(m_InputLayer);
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
	}

	private void OnDecline(InputActionEventData data)
	{
		base.ViewModel.Deactivate();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_CanFunc02.Value = (entity as IFunc02ClickHandler)?.CanFunc02Click() ?? false;
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
}

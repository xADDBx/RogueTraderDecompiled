using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.Code.UI.MVVM.View.SelectorWindow;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.Console;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarPartAbilitiesConsoleView : SurfaceActionBarPartAbilitiesBaseView, ICullFocusHandler, ISubscriber, ICharInfoAbilitiesChooseModeHandler, INetPingActionBarAbility
{
	private class PingData
	{
		public IDisposable PingDelay { get; set; }

		public GameObject PingObject { get; set; }

		public bool PingInProgress { get; set; }
	}

	[SerializeField]
	private SurfaceActionBarAbilitiesRowView m_Row;

	[SerializeField]
	private SurfaceActionBarSlotAbilityConsoleView m_SlotView;

	[SerializeField]
	protected PageNavigationConsole m_PageNavigation;

	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private ConsoleHint m_ActivateHint;

	[SerializeField]
	private ConsoleHint m_ActivateInCombatHint;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private VeilThicknessConsoleView m_VeilThicknessConsoleView;

	[SerializeField]
	private SurfaceMomentumConsoleView m_MomentumConsoleView;

	[SerializeField]
	private AbilitySelectorWindowConsoleView m_AbilitySelectorWindowConsoleView;

	[Header("Outside Ping")]
	[SerializeField]
	private Image m_LeftOutsidePing;

	[SerializeField]
	private RectTransform m_LeftOutsidePingRectTransform;

	[SerializeField]
	private Image m_LeftOutsidePingFrame;

	[SerializeField]
	private Image m_RightOutsidePing;

	[SerializeField]
	private RectTransform m_RightOutsidePingRectTransform;

	[SerializeField]
	private Image m_RightOutsidePingFrame;

	private readonly IntReactiveProperty m_CurrentRowIndex = new IntReactiveProperty();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private InputLayer m_MoveAbilityInputLayer;

	private bool m_ShowTooltip = true;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasContextMenu = new BoolReactiveProperty();

	private SurfaceActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	private SurfaceActionBarSlotAbilityConsoleView m_LastAbilitySlot;

	private MechanicActionBarSlot m_CurrentMechanicSlot;

	private int m_CurrentIndex;

	private IConsoleEntity m_CulledFocus;

	private readonly Dictionary<string, PingData> m_AbilityPingData = new Dictionary<string, PingData>();

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	public override void Initialize()
	{
		m_MoveAnimator.Or(null)?.Initialize();
		m_AbilitySelectorWindowConsoleView.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		CreateInput();
		AddDisposable(m_HintsWidget);
		AddDisposable(m_CurrentRowIndex.Subscribe(OnIndexChanged));
		AddDisposable(base.ViewModel.UnitChanged.Subscribe(OnUnitChanged));
		AddDisposable(base.ViewModel.SlotCountChanged.Subscribe(OnUnitChanged));
		AddDisposable(base.ViewModel.IsActive.Subscribe(OnActive));
		AddDisposable(base.ViewModel.MoveAbilityMode.Subscribe(OnMoveMode));
		if ((bool)m_AbilitySelectorWindowConsoleView)
		{
			AddDisposable(base.ViewModel.AbilitySelectorWindowVM.Subscribe(m_AbilitySelectorWindowConsoleView.Bind));
		}
		AddDisposable(base.ViewModel.AbilitySelectorWindowVM.Subscribe(delegate(AbilitySelectorWindowVM value)
		{
			if (value == null && (bool)m_LastAbilitySlot)
			{
				IConsoleEntity entity = m_NavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is SurfaceActionBarSlotAbilityConsoleView surfaceActionBarSlotAbilityConsoleView && surfaceActionBarSlotAbilityConsoleView == m_CurrentAbilitySlot);
				m_NavigationBehaviour.FocusOnEntityManual(entity);
				m_LastAbilitySlot = null;
			}
		}));
		AddDisposable(EventBus.Subscribe(this));
		OnUnitChanged();
		if (m_LeftOutsidePingRectTransform != null)
		{
			m_LeftOutsidePingRectTransform.gameObject.SetActive(value: false);
		}
		if (m_RightOutsidePingRectTransform != null)
		{
			m_RightOutsidePingRectTransform.gameObject.SetActive(value: false);
		}
	}

	protected override void DestroyViewImplementation()
	{
		HideAllPings();
		m_PageNavigation.Dispose();
		OnMoveMode(on: false);
		OnActive(active: false);
		m_Row.Dispose();
		m_ShowTooltip = true;
		m_InputLayer = null;
		m_MoveAbilityInputLayer = null;
	}

	private void OnIndexChanged(int newIndex)
	{
		if (base.ViewModel.Slots.Count != 0)
		{
			base.ViewModel.RowIndex = newIndex;
			CheckPingInRow();
			DrawSlots();
		}
	}

	private void OnUnitChanged()
	{
		if (base.ViewModel.Slots.Count != 0)
		{
			HideAllPings();
			DelayedInvoker.InvokeInFrames(DrawPaginator, 1);
		}
	}

	private void DrawPaginator()
	{
		m_CurrentRowIndex.SetValueAndForceNotify(base.ViewModel.RowIndex);
		m_PageNavigation.Initialize(Mathf.CeilToInt((float)base.ViewModel.Slots.Count / (float)base.SlotsInRow), m_CurrentRowIndex);
	}

	private void DrawSlots()
	{
		int num = m_NavigationBehaviour.Entities.IndexOf(m_NavigationBehaviour.Focus.Value);
		List<ActionBarSlotVM> list = new List<ActionBarSlotVM>();
		for (int i = base.ViewModel.RowIndex * base.SlotsInRow; i < (base.ViewModel.RowIndex + 1) * base.SlotsInRow; i++)
		{
			list.Add(base.ViewModel.Slots[i]);
		}
		m_Row.DrawEntries(list, m_SlotView);
		m_NavigationBehaviour.Clear();
		if ((bool)m_MomentumConsoleView)
		{
			List<IConsoleNavigationEntity> desperateMeasureSlots = m_MomentumConsoleView.GetDesperateMeasureSlots();
			List<IConsoleNavigationEntity> heroicActSlots = m_MomentumConsoleView.GetHeroicActSlots();
			List<IConsoleNavigationEntity> list2 = new List<IConsoleNavigationEntity>();
			List<IConsoleNavigationEntity> list3 = new List<IConsoleNavigationEntity>();
			for (int j = 0; j < desperateMeasureSlots.Count; j++)
			{
				((j <= 1) ? list2 : list3).Add(desperateMeasureSlots[j]);
			}
			for (int k = 0; k < heroicActSlots.Count; k++)
			{
				((k <= 1) ? list2 : list3).Add(heroicActSlots[k]);
			}
			if (!list3.Empty())
			{
				m_NavigationBehaviour.AddRow(list3);
			}
			m_NavigationBehaviour.AddRow(list2);
			m_NavigationBehaviour.AddRow(new IConsoleNavigationEntity[2] { m_VeilThicknessConsoleView, m_MomentumConsoleView });
		}
		m_NavigationBehaviour.AddRow(m_Row.GetConsoleEntities());
		if (num != -1)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_NavigationBehaviour.Entities.ElementAt(num));
		}
		else if (base.ViewModel.IsActive.Value)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_Row.GetFirstValidEntity());
		}
	}

	public SurfaceActionBarSlotAbilityConsoleView GetFirstEmptySlot()
	{
		IEnumerable<SurfaceActionBarSlotAbilityConsoleView> enumerable = m_Row.GetSlots().Cast<SurfaceActionBarSlotAbilityConsoleView>();
		foreach (SurfaceActionBarSlotAbilityConsoleView item in enumerable)
		{
			if (item.IsEmpty)
			{
				return item;
			}
		}
		return enumerable.FirstOrDefault();
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enable, bool inCombat)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			Activate();
		}, 11, enable);
		AddDisposable(inCombat ? m_ActivateInCombatHint.Bind(inputBindStruct) : m_ActivateHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
	}

	public void Activate()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.Default) && (!Game.Instance.TurnController.TurnBasedModeActive || Game.Instance.TurnController.IsPlayerTurn))
		{
			base.ViewModel.IsActive.Value = true;
		}
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(1, 0));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SurfaceActionBarPartAbilitiesConsoleView"
		});
		m_MoveAbilityInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "MoveAbility"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(OnDecline, 9, base.ViewModel.IsActive);
		AddDisposable(inputBindStruct);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Cancel));
		InputBindStruct disposable = m_InputLayer.AddButton(OnDecline, 11, base.ViewModel.IsActive);
		AddDisposable(disposable);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		AddDisposable(inputBindStruct2);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information));
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu.And(base.ViewModel.IsNotControllableCharacter?.Not()).ToReactiveProperty());
		AddDisposable(inputBindStruct3);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ContextMenu.ContextMenu));
		InputBindStruct inputBindStruct4 = m_MoveAbilityInputLayer.AddButton(delegate
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.DeleteSlot(m_CurrentIndex);
			});
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.SetMoveAbilityMode(on: false);
			});
		}, 9, base.ViewModel.MoveAbilityMode);
		AddDisposable(inputBindStruct4);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Cancel));
		InputBindStruct inputBindStruct5 = m_MoveAbilityInputLayer.AddButton(delegate
		{
		}, 8, base.ViewModel.MoveAbilityMode);
		AddDisposable(inputBindStruct5);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ActionTexts.MoveItem));
		m_PageNavigation.AddInput(m_MoveAbilityInputLayer, base.ViewModel.MoveAbilityMode);
		m_PageNavigation.AddInput(m_InputLayer, base.ViewModel.IsActive);
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
	}

	public void AddInputToPages(InputLayer inputLayer, BoolReactiveProperty active)
	{
		m_PageNavigation.AddInput(inputLayer, active);
		m_ShowTooltip = false;
	}

	private void OnActive(bool active)
	{
		if (active)
		{
			m_MoveAnimator.Or(null)?.AppearAnimation();
			Game.Instance.ClickEventsController.ClearPointerMode();
			GamePad.Instance.PushLayer(m_InputLayer);
			GridConsoleNavigationBehaviour navigationBehaviour = m_NavigationBehaviour;
			IConsoleNavigationEntity entity;
			if (!(m_LastAbilitySlot != null))
			{
				entity = m_Row.GetFirstValidEntity();
			}
			else
			{
				IConsoleNavigationEntity lastAbilitySlot = m_LastAbilitySlot;
				entity = lastAbilitySlot;
			}
			navigationBehaviour.FocusOnEntityManual(entity);
		}
		else
		{
			m_MoveAnimator.Or(null)?.DisappearAnimation();
			GamePad.Instance.PopLayer(m_InputLayer);
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
	}

	private void OnDecline(InputActionEventData data)
	{
		base.ViewModel.IsActive.Value = false;
		m_LastAbilitySlot = null;
	}

	private void OnMoveMode(bool on)
	{
		if (on)
		{
			GamePad.Instance.PushLayer(m_MoveAbilityInputLayer);
		}
		else
		{
			GamePad.Instance.PopLayer(m_MoveAbilityInputLayer);
		}
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CurrentAbilitySlot = entity as SurfaceActionBarSlotAbilityConsoleView;
		if (entity is SurfaceActionBarSlotAbilityConsoleView lastAbilitySlot)
		{
			m_LastAbilitySlot = lastAbilitySlot;
		}
		m_MomentumConsoleView.Or(null)?.OnFocusEntity(entity);
		m_HasContextMenu.Value = (bool)m_CurrentAbilitySlot && m_CurrentAbilitySlot.Index != -1;
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip.Value = tooltipBaseTemplate != null;
		if (m_ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowTooltip(tooltipBaseTemplate, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2>
				{
					new Vector2(1f, 0f)
				}
			});
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
		if (base.ViewModel.MoveAbilityMode.Value && (bool)m_CurrentAbilitySlot && m_CurrentMechanicSlot != null)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(m_CurrentMechanicSlot, m_CurrentIndex, m_CurrentAbilitySlot.Index);
			});
		}
		if ((bool)m_CurrentAbilitySlot)
		{
			m_CurrentMechanicSlot = m_CurrentAbilitySlot.MechanicActionBarSlot;
			m_CurrentIndex = m_CurrentAbilitySlot.Index;
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void ShowContextMenu(InputActionEventData data)
	{
		if ((bool)m_CurrentAbilitySlot && m_CurrentAbilitySlot.Index != -1)
		{
			m_CurrentAbilitySlot.ShowContextMenu(m_CurrentAbilitySlot.ContextMenuEntities);
		}
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}

	public void HandleChooseMode(bool active)
	{
		if (!active)
		{
			(from s in m_Row.GetSlots()
				select s as SurfaceActionBarSlotAbilityConsoleView).ForEach(delegate(SurfaceActionBarSlotAbilityConsoleView slot)
			{
				slot.SetSelectionActiveState(isActive: false);
			});
		}
	}

	public void HandlePingActionBarAbility(NetPlayer player, string keyName, Entity characterEntityRef, int slotIndex, WeaponSlotType weaponSlotType)
	{
		if (string.IsNullOrWhiteSpace(keyName) || characterEntityRef != base.ViewModel.Unit.Entity)
		{
			return;
		}
		int num = base.ViewModel.RowIndex * base.SlotsInRow;
		int num2 = (base.ViewModel.RowIndex + 1) * base.SlotsInRow;
		ActionBarSlotVM actionBarSlotVM = base.ViewModel.Slots.FirstOrDefault((ActionBarSlotVM s) => s.MechanicActionBarSlot.KeyName == keyName);
		int index = player.Index - 1;
		if (actionBarSlotVM == null)
		{
			return;
		}
		int num3 = base.ViewModel.Slots.IndexOf(actionBarSlotVM);
		if (num3 >= num && num3 < num2)
		{
			return;
		}
		if (m_AbilityPingData.ContainsKey(keyName))
		{
			PingData pingData = m_AbilityPingData[keyName];
			pingData.PingInProgress = false;
			pingData.PingDelay?.Dispose();
		}
		else
		{
			m_AbilityPingData[keyName] = new PingData();
		}
		PingData newPingData = m_AbilityPingData[keyName];
		if (num3 < num && m_LeftOutsidePing != null && m_LeftOutsidePingRectTransform != null)
		{
			m_LeftOutsidePing.sprite = actionBarSlotVM.Icon.Value;
			if (m_LeftOutsidePingFrame != null)
			{
				m_LeftOutsidePingFrame.color = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
			}
			newPingData.PingObject = m_LeftOutsidePingRectTransform.gameObject;
		}
		else if (num3 >= num2 && m_RightOutsidePing != null && m_RightOutsidePingRectTransform != null)
		{
			m_RightOutsidePing.sprite = actionBarSlotVM.Icon.Value;
			if (m_RightOutsidePingFrame != null)
			{
				m_RightOutsidePingFrame.color = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
			}
			newPingData.PingObject = m_RightOutsidePingRectTransform.gameObject;
		}
		newPingData.PingObject.Or(null)?.SetActive(value: true);
		newPingData.PingInProgress = true;
		newPingData.PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			if (m_AbilityPingData.Any((KeyValuePair<string, PingData> apd) => apd.Value != newPingData && apd.Value.PingObject == newPingData.PingObject && apd.Value.PingInProgress))
			{
				newPingData.PingInProgress = false;
			}
			else
			{
				newPingData.PingObject.SetActive(value: false);
				newPingData.PingInProgress = false;
			}
		}, 7.5f);
	}

	private void HideAllPings()
	{
		foreach (KeyValuePair<string, PingData> abilityPingDatum in m_AbilityPingData)
		{
			abilityPingDatum.Value.PingObject.SetActive(value: false);
			abilityPingDatum.Value.PingInProgress = false;
			abilityPingDatum.Value.PingDelay?.Dispose();
		}
	}

	private void CheckPingInRow()
	{
		int num = base.ViewModel.RowIndex * base.SlotsInRow;
		int num2 = (base.ViewModel.RowIndex + 1) * base.SlotsInRow;
		foreach (KeyValuePair<string, PingData> apd in m_AbilityPingData)
		{
			ActionBarSlotVM actionBarSlotVM = base.ViewModel.Slots.FirstOrDefault((ActionBarSlotVM s) => s.MechanicActionBarSlot.KeyName == apd.Key);
			if (actionBarSlotVM != null)
			{
				int num3 = base.ViewModel.Slots.IndexOf(actionBarSlotVM);
				if (num3 >= num && num3 < num2)
				{
					apd.Value.PingObject.SetActive(value: false);
					apd.Value.PingInProgress = false;
					apd.Value.PingDelay?.Dispose();
				}
			}
		}
	}
}

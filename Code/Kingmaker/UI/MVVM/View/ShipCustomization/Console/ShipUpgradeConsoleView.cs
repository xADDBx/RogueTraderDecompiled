using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.SelectorWindow;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipUpgradeConsoleView : ShipUpgradeBaseView<ShipInventoryStashConsoleView, ShipComponentSlotConsoleView, ShipUpgradeStructureSlotConsoleView, ShipUpgradeProwRamSlotConsoleView, ShipSelectorWindowConsoleView>
{
	[Header("Console")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	protected OwlcatMultiButton m_ExperienceButton;

	[SerializeField]
	private Image m_TooltipPlace;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private FloatConsoleNavigationBehaviour m_SlotsNavigation;

	private InputLayer m_ChooseSlotInputLayer;

	private readonly BoolReactiveProperty m_CanEquip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanChoose = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasContextMenu = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsRightWindow = new BoolReactiveProperty();

	private IItemSlotView m_CurrentEntity;

	private bool m_InputAdded;

	private float m_PrevValue;

	private IEnumerator m_Back;

	private Dictionary<ShipComponentSlotVM, ShipComponentSlotConsoleView> m_SlotsMap = new Dictionary<ShipComponentSlotVM, ShipComponentSlotConsoleView>();

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	public override void Initialize()
	{
		base.Initialize();
		m_InventoryStashView.Initialize();
		m_SelectorWindowView.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_InventoryStashView.Bind(base.ViewModel.ShipInventoryStashVM);
		if ((bool)m_SelectorWindowView)
		{
			AddDisposable(base.ViewModel.ShipSelectorWindowVM.Subscribe(m_SelectorWindowView.Bind));
		}
		AddDisposable(ObservableExtensions.Subscribe(base.ShipStash.ItemsFilter.FilterChanged, delegate
		{
			Refocus();
		}));
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_SlotsNavigation = new FloatConsoleNavigationBehaviour(m_Parameters));
		AddDisposable(m_NavigationBehaviour?.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		AddDisposable(base.ViewModel.ShipSelectorWindowVM.Subscribe(delegate(ShipItemSelectorWindowVM val)
		{
			DelFocus(val);
		}));
		AddDisposable(RootUIContext.Instance.CommonVM.ContextMenuVM.Subscribe(delegate(ContextMenuVM val)
		{
			DelFocus(val);
		}));
		AddDisposable(m_PlasmaDrives.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		AddDisposable(m_VoidShieldGenerator.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		AddDisposable(m_AugerArray.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		AddDisposable(m_ArmorPlating.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		ShipComponentSlotConsoleView[] weaponSlots = m_WeaponSlots;
		foreach (ShipComponentSlotConsoleView shipComponentSlotConsoleView in weaponSlots)
		{
			AddDisposable(shipComponentSlotConsoleView.SlotVM.Item.Subscribe(delegate
			{
				RotateToDefaultPosition();
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TooltipHelper.HideTooltip();
		m_Back = null;
		m_InputAdded = false;
	}

	private void Refocus()
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	public ConsoleNavigationBehaviour GetNavigation(List<IFloatConsoleNavigationEntity> additionalEntities)
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
			AddDisposable(m_SlotsNavigation = new FloatConsoleNavigationBehaviour(m_Parameters));
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		FloatConsoleNavigationBehaviour floatConsoleNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters);
		floatConsoleNavigationBehaviour.AddEntity(new SimpleConsoleNavigationEntity(m_ExperienceButton, m_ExperienceTooltip));
		floatConsoleNavigationBehaviour.AddEntities(new List<ShipComponentSlotConsoleView> { m_PlasmaDrives, m_VoidShieldGenerator, m_AugerArray, m_ArmorPlating });
		floatConsoleNavigationBehaviour.AddEntities(m_WeaponSlots);
		floatConsoleNavigationBehaviour.AddEntities(additionalEntities);
		floatConsoleNavigationBehaviour.AddEntities(new List<IFloatConsoleNavigationEntity> { m_UpgradeStructureSlot, m_UpgradeProwRamSlot });
		ConsoleNavigationBehaviour navigation = m_InventoryStashView.GetNavigation();
		m_NavigationBehaviour.AddEntityGrid(floatConsoleNavigationBehaviour);
		m_NavigationBehaviour.AddEntityGrid(navigation);
		m_ChooseSlotInputLayer = m_SlotsNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "ChooseSlot"
		});
		m_NavigationBehaviour.FocusOnEntityManual(m_PlasmaDrives);
		CreateChooseSlotNavigation();
		return m_NavigationBehaviour;
	}

	protected override void UpdateSlots()
	{
		m_PlasmaDrives.Bind(base.ViewModel.PlasmaDrives);
		m_SlotsMap.Add(base.ViewModel.PlasmaDrives, m_PlasmaDrives);
		m_VoidShieldGenerator.Bind(base.ViewModel.VoidShieldGenerator);
		m_SlotsMap.Add(base.ViewModel.VoidShieldGenerator, m_VoidShieldGenerator);
		m_AugerArray.Bind(base.ViewModel.AugerArray);
		m_SlotsMap.Add(base.ViewModel.AugerArray, m_AugerArray);
		m_ArmorPlating.Bind(base.ViewModel.ArmorPlating);
		m_SlotsMap.Add(base.ViewModel.ArmorPlating, m_ArmorPlating);
		for (int i = 0; i < m_WeaponSlots.Length; i++)
		{
			m_WeaponSlots[i].Bind(base.ViewModel.Weapons[i]);
			m_SlotsMap.Add(base.ViewModel.Weapons[i], m_WeaponSlots[i]);
		}
		m_UpgradeStructureSlot.Bind(base.ViewModel.InternalStructure);
		m_UpgradeProwRamSlot.Bind(base.ViewModel.ProwRam);
	}

	public void AddHints(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		AddDisposable(inputLayer.AddAxis(Scroll, 2, enabledHints));
	}

	private void DelFocus(IViewModel vm)
	{
		if (vm == null)
		{
			DelayedInvoker.InvokeInFrames(DeepestNestedFocus, 5);
		}
	}

	private void DeepestNestedFocus()
	{
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CanEquip.Value = entity is InventorySlotConsoleView inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.CanUse.Value;
		HandleTooltip(entity);
		m_CurrentEntity = entity as IItemSlotView;
		bool value = m_CurrentEntity != null && (m_CurrentEntity.SlotVM?.ContextMenu?.Value.Any((ContextMenuCollectionEntity item) => item.IsEnabled)).GetValueOrDefault();
		m_HasContextMenu.Value = value;
		m_CanChoose.Value = entity is ShipComponentSlotConsoleView || entity is ShipUpgradeStructureSlotConsoleView || entity is ShipUpgradeProwRamSlotConsoleView;
		IsRightWindow.Value = entity is InventorySlotConsoleView;
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
		else if (entity is ShipUpgradeProwRamSlotConsoleView shipUpgradeProwRamSlotConsoleView)
		{
			m_HasTooltip.Value = shipUpgradeProwRamSlotConsoleView.TooltipTemplate() != null;
			TooltipConfig tooltipConfig = default(TooltipConfig);
			tooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0f)
			};
			TooltipConfig config = tooltipConfig;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(shipUpgradeProwRamSlotConsoleView.TooltipTemplate(), m_NavigationBehaviour, config);
			}
		}
		else if (entity is ShipUpgradeStructureSlotConsoleView shipUpgradeStructureSlotConsoleView)
		{
			m_HasTooltip.Value = shipUpgradeStructureSlotConsoleView.TooltipTemplate() != null;
			TooltipConfig tooltipConfig = default(TooltipConfig);
			tooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0f)
			};
			TooltipConfig config2 = tooltipConfig;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(shipUpgradeStructureSlotConsoleView.TooltipTemplate(), m_NavigationBehaviour, config2);
			}
		}
		else if (entity is SimpleConsoleNavigationEntity simpleConsoleNavigationEntity)
		{
			m_HasTooltip.Value = simpleConsoleNavigationEntity.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				MonoBehaviour tooltipPlace = simpleConsoleNavigationEntity.GetTooltipPlace();
				if ((bool)tooltipPlace)
				{
					tooltipPlace.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour);
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour);
				}
			}
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = list.Count > 0;
			if (m_HasTooltip.Value && m_ShowTooltip.Value)
			{
				if (list.Count > 1)
				{
					monoBehaviour.ShowComparativeTooltip(hasTooltipTemplates.TooltipTemplates());
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(list.ElementAt(0), m_NavigationBehaviour);
				}
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void Scroll(InputActionEventData obj, float x)
	{
		if (x == 0f)
		{
			m_CharacterController.OnEndDrag(null);
		}
		else if (m_PrevValue == 0f)
		{
			m_CharacterController.OnBeginDrag(null);
		}
		m_PrevValue = x;
		m_CharacterController.Rotate(x * 2f);
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
		if (!m_InputAdded)
		{
			base.ShipStash.ItemsFilter.AddInput(inputLayer, IsRightWindow);
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				OnConfirm();
			}, 8, m_CanEquip);
			AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.ContextMenu.Equip, ConsoleHintsWidget.HintPosition.Right));
			AddDisposable(inputBindStruct);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				OnConfirm();
			}, 8, m_CanChoose);
			AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Select, ConsoleHintsWidget.HintPosition.Right));
			AddDisposable(inputBindStruct2);
			InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
			{
			}, 2, InputActionEventType.AxisActive);
			AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ActionTexts.Rotate, ConsoleHintsWidget.HintPosition.Left));
			AddDisposable(inputBindStruct3);
			InputBindStruct inputBindStruct4 = inputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
			AddDisposable(hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Right));
			AddDisposable(inputBindStruct4);
			InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
			{
				RotateToDefaultPosition();
			}, 18, base.ViewModel.CanSetDefaultPosition);
			AddDisposable(hintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ShipCustomization.ToDefaultPosition, ConsoleHintsWidget.HintPosition.Left));
			AddDisposable(inputBindStruct5);
			InputBindStruct inputBindStruct6 = inputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu, InputActionEventType.ButtonJustReleased);
			AddDisposable(hintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.ContextMenu.ContextMenu, ConsoleHintsWidget.HintPosition.Right));
			AddDisposable(inputBindStruct6);
			AddHints(inputLayer);
			m_InputAdded = true;
		}
	}

	private void OnConfirm()
	{
		DelayedInvoker.InvokeInFrames(DeepestNestedFocus, 10);
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void ShowContextMenu(InputActionEventData obj)
	{
		if (m_CurrentEntity is IConsoleEntity consoleEntity)
		{
			TooltipHelper.HideTooltip();
			((consoleEntity as MonoBehaviour) ?? (consoleEntity as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu(m_CurrentEntity.SlotVM?.ContextMenu?.Value);
		}
	}

	private void CreateChooseSlotNavigation()
	{
		AddDisposable(base.ViewModel.ChooseSlotMode.Skip(1).Subscribe(delegate(bool on)
		{
			if (on)
			{
				PushChooseSlotNavigation();
			}
			else
			{
				PopChooseSlotNavigation();
			}
		}));
		if ((bool)m_ConsoleHintsWidget)
		{
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseSlotInputLayer.AddButton(delegate
			{
				base.ViewModel.ChooseSlotMode.Value = false;
			}, 9, base.ViewModel.ChooseSlotMode), UIStrings.Instance.CommonTexts.Cancel));
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseSlotInputLayer.AddButton(delegate
			{
			}, 8, base.ViewModel.ChooseSlotMode), UIStrings.Instance.CommonTexts.Select));
		}
	}

	private void PushChooseSlotNavigation()
	{
		m_SlotsNavigation.Clear();
		foreach (var (shipComponentSlotVM2, entities) in m_SlotsMap)
		{
			if (shipComponentSlotVM2.IsPossibleTarget(base.ViewModel.ItemToSlotView.Item))
			{
				m_SlotsNavigation.AddEntity(entities);
				shipComponentSlotVM2.SetPossibleTargetState(state: false);
			}
		}
		GamePad.Instance.PushLayer(m_ChooseSlotInputLayer);
		m_SlotsNavigation.FocusOnFirstValidEntity();
	}

	private void PopChooseSlotNavigation()
	{
		GamePad.Instance.PopLayer(m_ChooseSlotInputLayer);
		m_SlotsNavigation.Clear();
		foreach (KeyValuePair<ShipComponentSlotVM, ShipComponentSlotConsoleView> item in m_SlotsMap)
		{
			item.Deconstruct(out var key, out var _);
			key.SetPossibleTargetState(state: true);
		}
		base.ViewModel.ItemToSlotView.ReleaseSlot();
		Refocus();
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}
}

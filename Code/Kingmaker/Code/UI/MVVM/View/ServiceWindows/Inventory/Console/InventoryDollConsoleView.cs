using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class InventoryDollConsoleView : InventoryDollView<InventoryEquipSlotConsoleView>
{
	[Header("Customization Values")]
	[SerializeField]
	private float m_RotateFactor = 4f;

	[SerializeField]
	private float m_ZoomFactor = 0.2f;

	[SerializeField]
	private float m_ZoomThresholdValue = 0.17f;

	[Header("Console")]
	[SerializeField]
	protected InventorySelectorWindowConsoleView m_SelectorWindowView;

	[Header("Character Visual Settings")]
	[SerializeField]
	private CharacterVisualSettingsConsoleView m_VisualSettingsConsoleView;

	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_ChooseSlotInputLayer;

	private List<IConsoleEntity> m_RightSlots = new List<IConsoleEntity>();

	private List<IConsoleEntity> m_LeftSlots = new List<IConsoleEntity>();

	private IConsoleEntity m_PrevFocused;

	private WeaponSetSelectorConsoleView WeaponSetConsoleView => m_WeaponSetSelector as WeaponSetSelectorConsoleView;

	public override void Initialize()
	{
		base.Initialize();
		m_SelectorWindowView.Or(null)?.Initialize();
		m_VisualSettingsConsoleView.Or(null)?.Initialize();
		m_VisualSettingsConsoleView.Or(null)?.SetDollRoomController(m_CharacterController, m_RotateFactor, m_ZoomFactor, m_ZoomThresholdValue);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_RightSlots = new List<IConsoleEntity> { m_Neck, m_Ring2, m_Gloves, m_Back, m_Boots };
		m_RightSlots.AddRange(m_QuickSlots);
		m_LeftSlots = new List<IConsoleEntity> { m_HeadArmor, m_BodyArmor, m_Ring1 };
		m_LeftSlots.AddRange(WeaponSetConsoleView.GetNavigationEntities());
		if ((bool)m_SelectorWindowView)
		{
			AddDisposable(base.ViewModel.InventorySelectorWindowVM.Subscribe(m_SelectorWindowView.Bind));
		}
		CreateNavigationIfNeeded();
		CreateChooseSlotNavigation();
		AddDisposable(base.ViewModel.VisualSettingsVM.Subscribe(m_VisualSettingsConsoleView.Bind));
	}

	private void CreateChooseSlotNavigation()
	{
		AddDisposable(base.ViewModel.ChooseSlotMode.Skip(1).Subscribe(delegate(bool on)
		{
			if (on)
			{
				m_PrevFocused = m_NavigationBehaviour.CurrentEntity;
				AddDisposable(GamePad.Instance.PushLayer(m_ChooseSlotInputLayer));
				foreach (IConsoleEntity entity in m_NavigationBehaviour.Entities)
				{
					if (entity is InventoryEquipSlotConsoleView inventoryEquipSlotConsoleView)
					{
						bool available = (inventoryEquipSlotConsoleView.SlotVM as EquipSlotVM)?.ItemSlot.CanInsertItem(base.ViewModel.ItemToSlotView.Item) ?? false;
						inventoryEquipSlotConsoleView.SetAvailable(available);
					}
				}
				m_NavigationBehaviour.FocusOnFirstValidEntity();
				TooltipHelper.HideTooltip();
			}
			else
			{
				GamePad.Instance.PopLayer(m_ChooseSlotInputLayer);
				foreach (IConsoleEntity entity2 in m_NavigationBehaviour.Entities)
				{
					if (entity2 is InventoryEquipSlotConsoleView inventoryEquipSlotConsoleView2)
					{
						inventoryEquipSlotConsoleView2.SetAvailable(value: true);
					}
				}
				m_NavigationBehaviour.FocusOnEntityManual(m_PrevFocused);
				m_NavigationBehaviour.UnFocusCurrentEntity();
				base.ViewModel.ItemToSlotView.ReleaseSlot();
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

	private void CreateNavigationIfNeeded()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
			List<IConsoleEntity> navigationEntities = WeaponSetConsoleView.GetNavigationEntities();
			GridConsoleNavigationBehaviour navigationBehaviour = m_NavigationBehaviour;
			IConsoleEntity[][] array = new IConsoleEntity[7][];
			IConsoleEntity[] array2 = new InventoryEquipSlotConsoleView[2] { m_HeadArmor, m_Neck };
			array[0] = array2;
			array2 = new InventoryEquipSlotConsoleView[2] { m_BodyArmor, m_Ring2 };
			array[1] = array2;
			array2 = new InventoryEquipSlotConsoleView[2] { m_Ring1, m_Gloves };
			array[2] = array2;
			array[3] = new IConsoleEntity[2]
			{
				navigationEntities?.ElementAtOrDefault(0),
				m_Back
			};
			array[4] = new IConsoleEntity[2]
			{
				navigationEntities?.ElementAtOrDefault(1),
				m_Boots
			};
			array[5] = new IConsoleEntity[3]
			{
				navigationEntities?.ElementAtOrDefault(2),
				m_QuickSlots[0],
				m_QuickSlots[1]
			};
			array[6] = new IConsoleEntity[3]
			{
				navigationEntities?.ElementAtOrDefault(3),
				m_QuickSlots[2],
				m_QuickSlots[3]
			};
			navigationBehaviour.SetEntities(array);
			m_NavigationBehaviour.FocusOnEntityManual(m_Neck);
			m_ChooseSlotInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
			{
				ContextName = "ChooseSlot"
			});
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		CreateNavigationIfNeeded();
		return m_NavigationBehaviour;
	}

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		AddDisposable(inputLayer.AddAxis(RotateDoll, 2));
		AddDisposable(inputLayer.AddAxis(ZoomDoll, 3));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowVisualSettings();
		}, 18), UIStrings.Instance.CharGen.ShowVisualSettings));
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_CharacterController.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_CharacterController.Zoom(x * m_ZoomFactor);
		}
	}

	public bool IsFocusOnRightSlots(IConsoleEntity entity)
	{
		return m_RightSlots.Contains(entity);
	}

	public bool IsSlot(IConsoleEntity entity)
	{
		if (!m_LeftSlots.Contains(entity))
		{
			return m_RightSlots.Contains(entity);
		}
		return true;
	}
}

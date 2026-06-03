using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.View.ActionBar.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Augmentations;

public class AugmentationsEquipSlotConsoleView : AugmentationsEquipSlotBaseView<ItemSlotConsoleView>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	protected SurfaceActionBarSlotAbilityView m_OverchargeAbility;

	[SerializeField]
	protected ConsoleHint m_InstallButtonHint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		m_ItemSlotView.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.Item.Subscribe(delegate(ItemEntity value)
		{
			m_EmptyPlaceholder.SetActive(value == null);
		}));
		AddDisposable(m_EmptyPlaceholderButton.SetHint(SetupHint(base.ViewModel.SlotType)));
		if (m_PossibleTargetHighlight != null)
		{
			AddDisposable(base.ViewModel.PossibleTarget.Subscribe(delegate(bool value)
			{
				m_PossibleTargetHighlight.SetActive(value);
			}));
		}
		AddDisposable(base.ViewModel.PlayerSize.Subscribe(delegate(AugmentationsPlayerSize value)
		{
			SetLinesType(value);
		}));
		AddDisposable(base.ViewModel.ToggleLinesCommand.Subscribe(delegate(bool value)
		{
			ToggleLines(value);
		}));
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.RefreshOverchargeAbility, delegate
		{
			SetOverchargeAbilitySlot();
			SetOverchargeButtonState();
		}));
		SetSlotState(isActive: true);
		SetOverchargeButtonState();
		SetInstallButtonInitialState();
		SetOverchargeAbilitySlot();
		SetSlotState(isActive: true);
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
			m_isOverchargeSlot = base.ViewModel.IsOverchargeSlot;
		}
	}

	public void SetFocus(bool value)
	{
		if (base.ViewModel == null)
		{
			return;
		}
		if (base.ViewModel.ShowPossibleTarget.Value)
		{
			if (value)
			{
				OnHoverStart();
			}
			else
			{
				OnHoverEnd();
			}
		}
		m_ItemSlotView.SetFocus(value);
	}

	public IDisposable SetConsoleHint(InputBindStruct bindStruct)
	{
		return m_InstallButtonHint.Bind(bindStruct);
	}

	public bool IsValid()
	{
		return m_ItemSlotView.IsValid();
	}

	public Vector2 GetPosition()
	{
		return m_ItemSlotView.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public IFloatConsoleNavigationEntity GetOverchargeAbilitySlot()
	{
		return m_OverchargeAbility as SurfaceActionBarSlotAbilityConsoleView;
	}

	public bool CanConfirmClick()
	{
		return !m_IsOverchargeSlotView;
	}

	public void OnConfirmClick()
	{
		OnClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void TryOverdrive()
	{
		if (!Game.Instance.TurnController.InCombat && m_OverchargeButton.isActiveAndEnabled && m_OverchargeButton.Interactable)
		{
			int currentLayer = m_OverchargeButton.ActiveLayerIndex;
			EventBus.RaiseEvent(delegate(IAugmentOverdriveToggleHandler h)
			{
				h.HandleAugmentOverdriveToggle(base.ViewModel.BlueprintAugmentSlot, currentLayer);
			});
			switch (currentLayer)
			{
			case 1:
				m_OverchargeButton.SetActiveLayer(2);
				break;
			case 2:
				m_OverchargeButton.SetActiveLayer(1);
				break;
			}
		}
	}

	public void OnInstallButtonConsole()
	{
		m_InstallButtonFade.DisappearAnimation();
		base.ViewModel.ApplyInstallation();
	}

	private void SetOverchargeButtonState()
	{
		ItemSlot itemSlot = base.ViewModel.ItemSlot;
		if (itemSlot != null && !itemSlot.HasItem)
		{
			if (m_OverchargeButton != null)
			{
				m_OverchargeButton.gameObject.SetActive(value: false);
			}
			return;
		}
		AugmentationsSlotVM viewModel = base.ViewModel;
		if (viewModel == null || !(viewModel.ItemSlot is AugmentSlot augmentSlot))
		{
			if (!(m_OverchargeButton == null))
			{
				m_OverchargeButton.gameObject.SetActive(value: false);
			}
			return;
		}
		bool flag = augmentSlot.ItemBlueprint.OverdriveAbility != null && !base.ViewModel.IsDirty.Value;
		bool flag2 = base.ViewModel.Unit.GetOptional<PartUnitBody>()?.Augments.OverdriveSlot == base.ViewModel.BlueprintAugmentSlot;
		if (!(m_OverchargeButton == null))
		{
			m_OverchargeButton.gameObject.SetActive(flag);
			m_OverchargeButton.Interactable = flag && !Game.Instance.TurnController.InCombat;
			if (flag && !Game.Instance.TurnController.InCombat && !flag2)
			{
				m_OverchargeButton.SetActiveLayer(1);
			}
			else if (flag2)
			{
				m_OverchargeButton.SetActiveLayer(2);
			}
			else
			{
				m_OverchargeButton.SetActiveLayer(0);
			}
		}
	}

	public void SetOverchargeAbilitySlot()
	{
		if (!(base.ViewModel.ItemSlot is AugmentSlot augmentSlot))
		{
			ActionBarSlotVM viewModel = new ActionBarSlotVM(new MechanicActionBarSlotEmpty
			{
				Unit = base.ViewModel.Unit
			}, -1, isInCharScreen: false, new BoolReactiveProperty(initialValue: false));
			m_OverchargeAbility.Initialize();
			m_OverchargeAbility.Bind(viewModel);
			m_OverchargeAbility.SetGreyscale(value: true);
			return;
		}
		if (augmentSlot.ItemBlueprint == null)
		{
			if (base.ViewModel.IsOverchargeSlot)
			{
				ActionBarSlotVM viewModel2 = new ActionBarSlotVM(new MechanicActionBarSlotEmpty
				{
					Unit = base.ViewModel.Unit
				}, -1, isInCharScreen: true, new BoolReactiveProperty(initialValue: false));
				m_OverchargeAbility.Initialize();
				m_OverchargeAbility.Bind(viewModel2);
				m_OverchargeAbility.SetGreyscale(value: true);
			}
			else
			{
				m_OverchargeAbility.gameObject.SetActive(value: false);
			}
			return;
		}
		BlueprintAbility overdriveAbility = augmentSlot.ItemBlueprint.OverdriveAbility;
		if (overdriveAbility == null)
		{
			if (base.ViewModel.IsOverchargeSlot)
			{
				ActionBarSlotVM viewModel3 = new ActionBarSlotVM(new MechanicActionBarSlotEmpty
				{
					Unit = base.ViewModel.Unit
				}, -1, isInCharScreen: true, new BoolReactiveProperty(initialValue: false));
				m_OverchargeAbility.Initialize();
				m_OverchargeAbility.Bind(viewModel3);
				m_OverchargeAbility.SetGreyscale(value: true);
			}
			else
			{
				m_OverchargeAbility.gameObject.SetActive(value: false);
			}
		}
		else
		{
			Ability ability;
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				ability = new Ability(overdriveAbility, base.ViewModel.Unit);
			}
			ActionBarSlotVM viewModel4 = new ActionBarSlotVM(new MechanicActionBarSlotAugmentsOverchargeAbility(ability, base.ViewModel.Unit), -1, isInCharScreen: true, new BoolReactiveProperty(initialValue: false));
			m_OverchargeAbility.Initialize();
			m_OverchargeAbility.Bind(viewModel4);
			m_OverchargeAbility.SetGreyscale(value: true);
		}
	}
}

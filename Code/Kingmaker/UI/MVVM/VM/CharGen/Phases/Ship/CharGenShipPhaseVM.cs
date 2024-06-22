using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.DLC;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.LevelUp;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Ship;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Ship;

public class CharGenShipPhaseVM : CharGenPhaseBaseVM, ICharGenShipPhaseHandler, ISubscriber
{
	public readonly ReactiveProperty<CharGenChangeNameMessageBoxVM> MessageBoxVM = new ReactiveProperty<CharGenChangeNameMessageBoxVM>();

	public readonly ReactiveProperty<string> ShipName = new ReactiveProperty<string>();

	public readonly ReactiveCommand<Action> InterruptHandler = new ReactiveCommand<Action>();

	public readonly ReactiveProperty<CharGenShipItemVM> SelectedShipEntity = new ReactiveProperty<CharGenShipItemVM>();

	private readonly ReactiveCollection<CharGenShipItemVM> m_ShipEntitiesList = new ReactiveCollection<CharGenShipItemVM>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly BoolReactiveProperty CurrentPageIsFirst = new BoolReactiveProperty();

	public readonly BoolReactiveProperty CurrentPageIsLast = new BoolReactiveProperty();

	private SelectionStateShip m_SelectionStateShip;

	private bool m_ShipNameWasEdited;

	private bool m_Subscribed;

	public SelectionGroupRadioVM<CharGenShipItemVM> ShipSelectionGroup { get; }

	public CharGenShipPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Ship)
	{
		base.DollRoomType = CharGenDollRoomType.Ship;
		base.DollPosition = CharacterDollPosition.Ship;
		base.CanInterruptChargen = true;
		base.ShowVisualSettings.Value = false;
		base.HasPortrait = false;
		ShipName.Value = string.Empty;
		ShipSelectionGroup = new SelectionGroupRadioVM<CharGenShipItemVM>(m_ShipEntitiesList, SelectedShipEntity);
		AddDisposable(ShipSelectionGroup);
		AddDisposable(SelectedShipEntity.Subscribe(SetShip));
		CreateTooltipSystem();
		BlueprintRoot.Instance.CharGenRoot.EnsureShipPregens(delegate(List<ChargenUnit> ships)
		{
			foreach (ChargenUnit item in ships.Where((ChargenUnit ship) => !ship.Blueprint.IsDlcRestricted()))
			{
				m_ShipEntitiesList.Add(AddDisposableAndReturn(new CharGenShipItemVM(item)));
			}
		});
		AddDisposable(IsCompletedAndAvailable.Subscribe(delegate(bool value)
		{
			OverrideConfirmHintLabel.Value = (value ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CharGen.EditName);
		}));
		AddDisposable(SelectedShipEntity.Subscribe(delegate(CharGenShipItemVM value)
		{
			CurrentPageIsFirst.Value = m_ShipEntitiesList.FirstOrDefault() == value;
			CurrentPageIsLast.Value = m_ShipEntitiesList.LastOrDefault() == value;
		}));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_ShipEntitiesList.Clear();
	}

	protected override bool CheckIsCompleted()
	{
		SelectionStateShip selectionStateShip = m_SelectionStateShip;
		if (selectionStateShip != null && selectionStateShip.IsMade && selectionStateShip.IsValid)
		{
			return m_ShipNameWasEdited;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		if (!m_Subscribed)
		{
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			m_Subscribed = true;
		}
		TrySelectItem();
		if (!m_ShipNameWasEdited)
		{
			SetName(GetDefaultName());
		}
	}

	protected virtual void Clear()
	{
		SelectedShipEntity.Value = null;
		m_ShipNameWasEdited = false;
	}

	protected virtual void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager != null)
		{
			BlueprintSelectionShip selectionByType = CharGenUtility.GetSelectionByType<BlueprintSelectionShip>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStateShip = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateShip;
				UpdateIsCompleted();
			}
		}
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	private void SetShip(CharGenShipItemVM shipItemVM)
	{
		if (shipItemVM != null && shipItemVM.BlueprintStarship != null)
		{
			Game.Instance.GameCommandQueue.CharGenSetShip(shipItemVM.BlueprintStarship);
		}
	}

	void ICharGenShipPhaseHandler.HandleSetShip(BlueprintStarship blueprintStarship)
	{
		CharGenShipItemVM charGenShipItemVM = m_ShipEntitiesList.FirstOrDefault((CharGenShipItemVM elem) => elem.BlueprintStarship == blueprintStarship);
		if (charGenShipItemVM == null)
		{
			PFLog.UI.Error("[HandleSetShip] Item was not found! Blueprint=" + blueprintStarship.AssetGuid);
			return;
		}
		if (!UINetUtility.IsControlMainCharacter())
		{
			SelectedShipEntity.Value = charGenShipItemVM;
		}
		m_SelectionStateShip?.SelectShip(charGenShipItemVM.ChargenUnit.Unit);
		if (!m_ShipNameWasEdited)
		{
			ShipName.Value = charGenShipItemVM.Title;
		}
		SetupTooltipTemplate();
		UpdateIsCompleted();
	}

	private void TrySelectItem()
	{
		if (SelectedShipEntity.Value == null)
		{
			ShipSelectionGroup.TrySelectFirstValidEntity();
		}
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = TooltipTemplate();
	}

	private TooltipBaseTemplate TooltipTemplate()
	{
		BlueprintStarship blueprintStarship = SelectedShipEntity.Value?.BlueprintStarship;
		if (blueprintStarship == null)
		{
			return null;
		}
		return new TooltipTemplatePlayerStarship(blueprintStarship);
	}

	public void SetName(string shipName)
	{
		Game.Instance.GameCommandQueue.CharGenSetShipName(shipName);
	}

	void ICharGenShipPhaseHandler.HandleSetName(string shipName)
	{
		ShipName.Value = shipName;
		m_SelectionStateShip?.Ship?.Description.SetName(shipName);
		m_ShipNameWasEdited = true;
		UpdateIsCompleted();
	}

	public override void InterruptChargen(Action onComplete)
	{
		Action parameter = (Game.Instance.IsControllerGamepad ? ((Action)delegate
		{
		}) : onComplete);
		InterruptHandler.Execute(parameter);
	}

	public string GetRandomName()
	{
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetRandomShipName(ShipName.Value);
	}

	private string GetDefaultName()
	{
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetDefaultShipName(string.Empty);
	}

	public void ShowChangeNameMessageBox(Action onComplete = null)
	{
		DisposeMessageBox();
		MessageBoxVM.Value = new CharGenChangeNameMessageBoxVM(UIStrings.Instance.CharGen.ChooseName, UIStrings.Instance.SettingsUI.DialogApply, delegate(string text)
		{
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				SetName(text);
			}
			onComplete?.Invoke();
		}, ShipName.Value, GetRandomName, DisposeMessageBox);
	}

	private void DisposeMessageBox()
	{
		DisposeAndRemove(MessageBoxVM);
	}

	public bool GoNextPage()
	{
		return ShipSelectionGroup.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return ShipSelectionGroup.SelectPrevValidEntity();
	}
}

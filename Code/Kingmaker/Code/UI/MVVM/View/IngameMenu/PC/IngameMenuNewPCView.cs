using Code.UI.Pointer;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.IngameMenu.PC;

public class IngameMenuNewPCView : IngameMenuBasePCView<IngameMenuVM>
{
	[Header("Buttons Part")]
	[SerializeField]
	private OwlcatMultiButton m_Inventory;

	[SerializeField]
	private OwlcatMultiButton m_Character;

	[SerializeField]
	private OwlcatMultiButton m_Journal;

	[SerializeField]
	private OwlcatMultiButton m_Map;

	[SerializeField]
	private OwlcatMultiButton m_Encyclopedia;

	[SerializeField]
	private OwlcatMultiButton m_ShipCustomization;

	[SerializeField]
	private OwlcatMultiButton m_ColonyManagement;

	[SerializeField]
	private OwlcatMultiButton m_CargoManagement;

	[SerializeField]
	private OwlcatMultiButton m_Formation;

	[Header("Highlighter")]
	[SerializeField]
	private UIHighlighter m_UIHighlighter;

	private bool m_IsInit;

	public override void Initialize()
	{
		if (m_IsInit)
		{
			return;
		}
		if (m_UIHighlighter != null)
		{
			m_UIHighlighter.Initialize(() => m_CargoManagement.gameObject.activeSelf);
			m_UIHighlighter.SetKey(LootVM.CargoButtonInMenuHighlighterKey);
		}
		m_IsInit = true;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		SetPlastickButtonsSoundsTypes();
		AddDisposable(ObservableExtensions.Subscribe(m_Map.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenMap();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Journal.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenJournal();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Encyclopedia.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenEncyclopedia();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Character.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenCharScreen();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Inventory.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenInventory();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_ShipCustomization.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenShipCustomization();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_ColonyManagement.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenColonyManagement();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_CargoManagement.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenCargoManagement();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Formation.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenFormation();
		}));
		AddDisposable(m_Map.SetHint(UIStrings.Instance.MainMenu.LocalMap, "OpenMap"));
		AddDisposable(m_Journal.SetHint(UIStrings.Instance.MainMenu.Journal, "OpenJournal"));
		AddDisposable(m_Encyclopedia.SetHint(UIStrings.Instance.MainMenu.Encyclopedia, "OpenEncyclopedia"));
		AddDisposable(m_Character.SetHint(UIStrings.Instance.MainMenu.CharacterInfo, "OpenCharacterScreen"));
		AddDisposable(m_Inventory.SetHint(UIStrings.Instance.MainMenu.Inventory, "OpenInventory"));
		AddDisposable(m_ShipCustomization.SetHint(UIStrings.Instance.MainMenu.ShipCustomization, "OpenShipCustomization"));
		AddDisposable(m_ColonyManagement.SetHint(UIStrings.Instance.MainMenu.ColonyManagement, "OpenColonyManagement"));
		AddDisposable(m_CargoManagement.SetHint(UIStrings.Instance.MainMenu.CargoManagement, "OpenCargoManagement"));
		AddDisposable(m_Formation.SetHint(UIStrings.Instance.EscapeMenu.EscMenuFormation, "OpenFormation"));
		AddDisposable(base.ViewModel.IsInventoryActive.Subscribe(delegate(bool value)
		{
			m_Inventory.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsCharScreenActive.Subscribe(delegate(bool value)
		{
			m_Character.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsJournalActive.Subscribe(delegate(bool value)
		{
			m_Journal.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsLocalMapActive.Subscribe(delegate(bool value)
		{
			m_Map.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsEncyclopediaActive.Subscribe(delegate(bool value)
		{
			m_Encyclopedia.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsShipCustomization.Subscribe(delegate(bool value)
		{
			m_ShipCustomization.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsColonyManagementActive.Subscribe(delegate(bool value)
		{
			m_ColonyManagement.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsCargoManagementActive.Subscribe(delegate(bool value)
		{
			m_CargoManagement.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.IsFormationActive.Subscribe(delegate(bool value)
		{
			m_Formation.SetActiveLayer(value ? 1 : 0);
		}));
		if (m_UIHighlighter != null)
		{
			AddDisposable(m_UIHighlighter.Subscribe());
		}
		CheckEnabledInGameMenuButtons();
		AddDisposable(base.ViewModel.CheckCanAccessStarshipInventoryButtons.Subscribe(CheckEnabledInGameMenuButtons));
		AddDisposable(base.ViewModel.CheckCanAccessColonizationButton.Subscribe(CheckEnabledColonizationButton));
	}

	private void CheckEnabledInGameMenuButtons()
	{
		bool canAccessStarshipInventory = Game.Instance.Player.CanAccessStarshipInventory;
		m_ShipCustomization.gameObject.SetActive(canAccessStarshipInventory);
		CheckEnabledColonizationButton();
	}

	private void CheckEnabledColonizationButton()
	{
		bool canAccessStarshipInventory = Game.Instance.Player.CanAccessStarshipInventory;
		bool flag = Game.Instance.Player.ColoniesState.ForbidColonization;
		m_ColonyManagement.gameObject.SetActive(canAccessStarshipInventory && !flag);
	}

	private void SetPlastickButtonsSoundsTypes()
	{
		UISounds.Instance.SetClickAndHoverSound(m_Inventory, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Character, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Journal, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Map, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Encyclopedia, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ShipCustomization, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ColonyManagement, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CargoManagement, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Formation, UISounds.ButtonSoundsEnum.PlastickSound);
	}

	public void OnUIReset()
	{
		AddDisposable(m_Map.SetHint(UIStrings.Instance.MainMenu.LocalMap, "OpenMap"));
		AddDisposable(m_Journal.SetHint(UIStrings.Instance.MainMenu.Journal, "OpenJournal"));
		AddDisposable(m_Encyclopedia.SetHint(UIStrings.Instance.MainMenu.Encyclopedia, "OpenEncyclopedia"));
		AddDisposable(m_Character.SetHint(UIStrings.Instance.MainMenu.CharacterInfo, "OpenCharacterScreen"));
		AddDisposable(m_Inventory.SetHint(UIStrings.Instance.MainMenu.Inventory, "OpenInventory"));
		AddDisposable(m_ShipCustomization.SetHint(UIStrings.Instance.MainMenu.ShipCustomization, "OpenShipCustomization"));
		AddDisposable(m_ColonyManagement.SetHint(UIStrings.Instance.MainMenu.ColonyManagement, "OpenColonyManagement"));
		AddDisposable(m_CargoManagement.SetHint(UIStrings.Instance.MainMenu.CargoManagement, "OpenCargoManagement"));
		AddDisposable(m_Formation.SetHint(UIStrings.Instance.MainMenu.CargoManagement, "OpenFormation"));
	}
}

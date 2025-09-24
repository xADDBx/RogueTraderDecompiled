using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.IngameMenu;

public class IngameMenuVM : IngameMenuBaseVM, ICanAccessStarshipInventoryHandler, ISubscriber, ICanAccessColonizationHandler, ICanAccessServiceWindowsHandler, ICanAccessSelectedWindowsHandler
{
	private List<UnitReference> m_PartyCharacters;

	public readonly ReactiveCommand CheckCanAccessStarshipInventoryButtons = new ReactiveCommand();

	public readonly ReactiveCommand CheckCanAccessColonizationButton = new ReactiveCommand();

	public readonly ReactiveCommand CheckServiceWindowsBlocked = new ReactiveCommand();

	public bool IsShown;

	public IngameMenuVM()
	{
		UpdatePartyCharacters();
	}

	public bool IsInSpace()
	{
		if (!Game.Instance.IsModeActive(GameModeType.SpaceCombat) && !Game.Instance.IsModeActive(GameModeType.StarSystem))
		{
			return Game.Instance.IsModeActive(GameModeType.GlobalMap);
		}
		return true;
	}

	public void OpenInventory()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenInventory();
		});
	}

	public void OpenCharScreen()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfo();
		});
	}

	public void OpenJournal()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenJournal();
		});
	}

	public void OpenEncyclopedia()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenEncyclopedia();
		});
	}

	public void OpenMap()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenLocalMap();
		});
	}

	public void OpenColonyManagement()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenColonyManagement();
		});
	}

	public void OpenCargoManagement()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenCargoManagement();
		});
	}

	public void OpenFormation()
	{
		if (!IsFormationActive.Value)
		{
			EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
			{
				h.HandleOpenFormation();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
			{
				h.HandleCloseFormation();
			});
		}
	}

	public void OpenEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void OpenShipCustomization()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenShipCustomization();
		});
	}

	public void OpenShipLevelUp()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenShipCustomizationPage(ShipCustomizationTab.Skills);
		});
	}

	public void OpenLevelUpOnFirstDecentUnit()
	{
		if (Game.Instance.SelectionCharacter.FirstSelectedUnit == null || !LevelUpController.CanLevelUp(Game.Instance.SelectionCharacter.FirstSelectedUnit))
		{
			UnitReference unitReference = m_PartyCharacters.FirstOrDefault((UnitReference c) => LevelUpController.CanLevelUp(c.ToBaseUnitEntity()));
			if (unitReference != null)
			{
				Game.Instance.SelectionCharacter.SetSelected(unitReference.ToBaseUnitEntity());
			}
		}
		OpenLevelUp();
	}

	public void OpenLevelUp()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfoPage(CharInfoPageType.LevelProgression, null);
		});
	}

	public void OnOpenGroupChanger()
	{
		StartChangedPartyOnGlobalMap();
	}

	private void ChangePartyOnMapRequest(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			StartChangedPartyOnGlobalMap();
		}
	}

	private void StartChangedPartyOnGlobalMap()
	{
		UpdatePartyCharacters();
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(ChangePartyOnMap, null, isCapital: true);
		});
	}

	private void ChangePartyOnMap()
	{
		_ = m_PartyCharacters?.Select((UnitReference r) => r.Entity).SequenceEqual(Game.Instance.Player.Party) ?? true;
	}

	private void UpdatePartyCharacters()
	{
		m_PartyCharacters = Game.Instance.Player.Party.Select((BaseUnitEntity u) => UnitReference.FromIAbstractUnitEntity(u)).ToList();
	}

	public bool HasLevelUp()
	{
		UpdatePartyCharacters();
		return m_PartyCharacters.Any((UnitReference c) => LevelUpController.CanLevelUp(c.ToBaseUnitEntity()));
	}

	public bool HasShipLvlUp()
	{
		return LevelUpController.CanLevelUp(Game.Instance.Player.PlayerShip);
	}

	public void HandleCanAccessStarshipInventory()
	{
		CheckCanAccessStarshipInventoryButtons.Execute();
	}

	public void HandleCanAccessColonization()
	{
		CheckCanAccessColonizationButton.Execute();
	}

	public void HandleServiceWindowsBlocked()
	{
		CheckServiceWindowsBlocked.Execute();
	}

	public void HandleSelectedWindowsBlocked()
	{
		CheckServiceWindowsBlocked.Execute();
	}
}

using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INewServiceWindowUIHandler : ISubscriber
{
	void HandleCloseAll();

	void HandleOpenWindowOfType(ServiceWindowsType type);

	void HandleOpenInventory();

	void HandleOpenEncyclopedia(INode page = null);

	void HandleOpenCharacterInfo();

	void HandleOpenCharacterInfoPage(CharInfoPageType pageType, BaseUnitEntity unitEntity);

	void HandleOpenShipCustomizationPage(ShipCustomizationTab pageType);

	void HandleOpenJournal();

	void HandleOpenLocalMap();

	void HandleOpenColonyManagement();

	void HandleOpenCargoManagement();

	void HandleOpenShipCustomization(bool force = false);
}

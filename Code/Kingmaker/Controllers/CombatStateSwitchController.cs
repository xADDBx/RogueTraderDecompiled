using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;

namespace Kingmaker.Controllers;

public class CombatStateSwitchController : IController, IPartyCombatHandler, ISubscriber
{
	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat && !Game.Instance.SelectionCharacter.IsSingleSelected.Value && Game.Instance.SelectionCharacter.FirstSelectedUnit != null)
		{
			UIAccess.SelectionManager.SelectUnit(Game.Instance.SelectionCharacter.FirstSelectedUnit.View);
		}
	}
}

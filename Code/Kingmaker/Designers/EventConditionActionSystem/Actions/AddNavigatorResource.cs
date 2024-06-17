using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("126c1804a65a4391bdd281d1eb7fd260")]
[PlayerUpgraderAllowed(false)]
public class AddNavigatorResource : GameAction
{
	[SerializeField]
	public int AddCount;

	public override string GetCaption()
	{
		return "Add navigator resource";
	}

	public override void RunAction()
	{
		Game.Instance.SectorMapController.ChangeNavigatorResourceCount(AddCount);
	}
}

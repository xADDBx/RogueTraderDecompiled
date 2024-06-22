using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("4306440a32ed467cb035876eb8223c3d")]
[PlayerUpgraderAllowed(false)]
public class AddStarSystemToVisit : GameAction
{
	public BlueprintStarSystemMapReference StarSystemMap;

	public override string GetCaption()
	{
		return "Add StarSystem " + StarSystemMap.NameSafe() + " To Visit when warp travel to it";
	}

	protected override void RunAction()
	{
		if (StarSystemMap != null && !Game.Instance.Player.StarSystemsState.StarSystemsToVisit.Contains(StarSystemMap))
		{
			Game.Instance.Player.StarSystemsState.StarSystemsToVisit.Add(StarSystemMap);
		}
	}
}

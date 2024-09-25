using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("3864bbd5febf4338a505e91fecbeb187")]
public class SetCurrentStarSystem : GameAction
{
	[SerializeField]
	private BlueprintStarSystemMap.Reference m_StarSystem;

	public BlueprintStarSystemMap StarSystem => m_StarSystem?.Get();

	public override string GetCaption()
	{
		return "Set " + StarSystem?.Name + " as current star system";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.CurrentStarSystem = StarSystem;
		Game.Instance.Player.PreviousVisitedArea = StarSystem;
		Game.Instance.Player.LastPositionOnPreviousVisitedArea = null;
	}
}

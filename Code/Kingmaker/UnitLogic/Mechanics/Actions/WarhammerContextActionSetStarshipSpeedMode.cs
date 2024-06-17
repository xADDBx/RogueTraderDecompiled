using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5a62434637b179e448bf2f822d6eb945")]
public class WarhammerContextActionSetStarshipSpeedMode : ContextAction
{
	[SerializeField]
	private PartStarshipNavigation.SpeedModeType SpeedMode;

	public override string GetCaption()
	{
		return $"Set speed mode to {SpeedMode}";
	}

	public override void RunAction()
	{
		if (base.Target.Entity is StarshipEntity starshipEntity)
		{
			starshipEntity.Navigation.SpeedMode = SpeedMode;
		}
	}
}

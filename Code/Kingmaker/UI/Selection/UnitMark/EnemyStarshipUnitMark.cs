using Kingmaker.SpaceCombat.StarshipLogic.Parts;

namespace Kingmaker.UI.Selection.UnitMark;

public class EnemyStarshipUnitMark : StarshipUnitMark
{
	protected override void CheckStarshipShieldsVisibility()
	{
		PartStarshipShields starshipShieldsOptional = base.Unit.GetStarshipShieldsOptional();
		CurrentShipDecalData.SetShieldsVisible(starshipShieldsOptional != null && starshipShieldsOptional.ShieldsSum > 0, IsDirect);
	}
}

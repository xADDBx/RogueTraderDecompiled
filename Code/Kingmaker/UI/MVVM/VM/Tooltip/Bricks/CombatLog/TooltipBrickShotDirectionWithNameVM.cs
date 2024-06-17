using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickShotDirectionWithNameVM : TooltipBaseBrickVM
{
	public readonly int ShotNumber;

	public readonly RuleRollScatterShotHitDirection.Ray ShotDeviationType;

	public readonly int DeviationMin;

	public readonly int DeviationMax;

	public readonly int DeviationValue;

	public TooltipBrickShotDirectionWithNameVM(int shotNumber, RuleRollScatterShotHitDirection.Ray shotDeviationType, int deviationMin, int deviationMax, int deviationValue)
	{
		ShotNumber = shotNumber;
		ShotDeviationType = shotDeviationType;
		DeviationMin = deviationMin;
		DeviationMax = deviationMax;
		DeviationValue = deviationValue;
	}
}

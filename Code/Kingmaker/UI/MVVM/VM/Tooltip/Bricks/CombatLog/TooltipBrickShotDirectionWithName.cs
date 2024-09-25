using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickShotDirectionWithName : ITooltipBrick
{
	private readonly int m_ShotNumber;

	private readonly RuleRollScatterShotHitDirection.Ray m_ShotDeviationType;

	private readonly int m_DeviationMin;

	private readonly int m_DeviationMax;

	private readonly int m_DeviationValue;

	public TooltipBrickShotDirectionWithName(int shotNumber, RuleRollScatterShotHitDirection.Ray shotDeviationType, int deviationMin, int deviationMax, int currentValue)
	{
		m_ShotNumber = shotNumber;
		m_ShotDeviationType = shotDeviationType;
		m_DeviationMin = deviationMin;
		m_DeviationMax = deviationMax;
		m_DeviationValue = currentValue;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickShotDirectionWithNameVM(m_ShotNumber, m_ShotDeviationType, m_DeviationMin, m_DeviationMax, m_DeviationValue);
	}
}

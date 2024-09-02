using Kingmaker.Designers.Mechanics.Facts.Restrictions;

namespace Kingmaker.Controllers.TurnBased;

public class InterruptionData
{
	public bool AsExtraTurn;

	public bool WaitForCommandsToFinish;

	public RestrictionCalculator RestrictionsOnInterrupt = new RestrictionCalculator();
}

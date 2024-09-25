namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

public interface ICanTargetDeadUnits
{
	bool CanTargetDead { get; }

	bool CanTargetAlive { get; }
}

namespace Kingmaker.Networking;

public abstract class BaseDesyncDetectionStrategy
{
	protected const int RecoveryDelayInSec = 5;

	protected const int RecoveryDelayInTicks = 100;

	protected const int DesyncTickDefaultValue = -32768;

	public bool WasDesync { get; protected set; }

	public abstract bool HasDesync { get; }

	public abstract void ReportState();

	public abstract void Reset();
}

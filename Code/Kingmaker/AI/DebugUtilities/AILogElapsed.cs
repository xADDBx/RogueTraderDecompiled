namespace Kingmaker.AI.DebugUtilities;

public class AILogElapsed : AILogObject
{
	private readonly long ms;

	public AILogElapsed(long ms)
	{
		this.ms = ms;
	}

	public override string GetLogString()
	{
		return $"Completed in {ms} ms";
	}
}

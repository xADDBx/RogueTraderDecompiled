namespace Kingmaker.AI.DebugUtilities;

public class AILogMessage : AILogObject
{
	private readonly string message;

	public AILogMessage(string message)
	{
		this.message = message;
	}

	public override string GetLogString()
	{
		return message;
	}
}

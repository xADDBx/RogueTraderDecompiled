namespace Core.Cheats;

public static class ExecutionPolicyHelper
{
	public static bool IsAllowedNow(this ExecutionPolicy executionPolicy, bool isPlaying)
	{
		switch (executionPolicy)
		{
		case ExecutionPolicy.All:
			return true;
		case ExecutionPolicy.PlayMode:
			if (isPlaying)
			{
				return true;
			}
			break;
		case ExecutionPolicy.EditMode:
			if (!isPlaying)
			{
				return true;
			}
			break;
		}
		return false;
	}
}

namespace Kingmaker.Controllers.Clicks;

public struct HandlerPriorityResult
{
	public float Priority { get; }

	public bool ShowOvertip { get; }

	public HandlerPriorityResult(float p)
	{
		Priority = p;
		ShowOvertip = false;
	}

	public HandlerPriorityResult(float p, bool s)
	{
		Priority = p;
		ShowOvertip = s;
	}
}

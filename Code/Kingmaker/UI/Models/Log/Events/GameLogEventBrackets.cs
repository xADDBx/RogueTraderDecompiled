namespace Kingmaker.UI.Models.Log.Events;

public abstract class GameLogEventBrackets<TSelf> : GameLogEvent<TSelf> where TSelf : GameLogEvent<TSelf>
{
	private bool m_Ready;

	public sealed override bool IsReady => m_Ready;

	protected void MarkReady()
	{
		m_Ready = true;
	}
}

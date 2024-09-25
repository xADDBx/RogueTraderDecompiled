namespace Kingmaker.UI.Models.Log.Events;

public interface IGameLogEventHandler<in T> where T : GameLogEvent
{
	void HandleEvent(T evt);
}

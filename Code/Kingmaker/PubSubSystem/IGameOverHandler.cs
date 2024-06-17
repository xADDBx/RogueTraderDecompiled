using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGameOverHandler : ISubscriber
{
	void HandleGameOver(Player.GameOverReasonType reason);
}

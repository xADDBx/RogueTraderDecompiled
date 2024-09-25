using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGameModeHandler : ISubscriber
{
	void OnGameModeStart(GameModeType gameMode);

	void OnGameModeStop(GameModeType gameMode);
}

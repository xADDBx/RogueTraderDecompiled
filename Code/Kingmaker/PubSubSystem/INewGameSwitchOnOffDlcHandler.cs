using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INewGameSwitchOnOffDlcHandler : ISubscriber
{
	void HandleNewGameSwitchOnOffDlc(BlueprintDlc dlc, bool value);
}

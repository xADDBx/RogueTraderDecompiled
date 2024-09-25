using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;

namespace Kingmaker.PubSubSystem;

public interface ILevelUpDollHandler : ISubscriber
{
	void HandleDollStateUpdated(DollState dollState);
}

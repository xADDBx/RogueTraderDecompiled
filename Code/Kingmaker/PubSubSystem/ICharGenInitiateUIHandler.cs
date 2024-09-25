using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen;

namespace Kingmaker.PubSubSystem;

public interface ICharGenInitiateUIHandler : ISubscriber
{
	void HandleStartCharGen(CharGenConfig config, bool isCustomCompanionChargen);
}

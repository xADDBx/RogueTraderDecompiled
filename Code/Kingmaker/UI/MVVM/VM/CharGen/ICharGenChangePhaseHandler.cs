using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenChangePhaseHandler : ISubscriber
{
	void HandlePhaseChange(CharGenPhaseType phaseType);
}

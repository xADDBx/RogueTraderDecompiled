using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenSummaryPhaseHandler : ISubscriber
{
	void HandleSetName([NotNull] string name);
}

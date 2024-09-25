using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.QA.Arbiter;

internal interface IArbiterEventHandler : ISubscriber
{
	void ArbiterFinished(Arbiter arbiter);
}

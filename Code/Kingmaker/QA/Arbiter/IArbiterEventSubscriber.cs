using System;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.QA.Arbiter;

public interface IArbiterEventSubscriber : ISubscriber
{
	void ArbiterFailed(Exception ex);

	void ArbiterFinished();

	void Update();

	void Stop();
}

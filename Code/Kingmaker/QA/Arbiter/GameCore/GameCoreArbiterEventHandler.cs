using System;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.QA.Arbiter.GameCore;

public class GameCoreArbiterEventHandler : IArbiterEventHandler
{
	public virtual void ArbiterFinished()
	{
		PFLog.SmartConsole.Log("Arbiter finished");
		EventBus.RaiseEvent(delegate(IArbiterEventSubscriber wrn)
		{
			wrn.ArbiterFinished();
		});
	}

	public void Update()
	{
		EventBus.RaiseEvent(delegate(IArbiterEventSubscriber wrn)
		{
			wrn.Update();
		});
	}

	public void ArbiterTerminated()
	{
		EventBus.RaiseEvent(delegate(IArbiterEventSubscriber wrn)
		{
			wrn.Stop();
		});
	}

	public void Initialized()
	{
		ArbiterIntegration.SetQaMode(b: false);
	}

	public virtual void ArbiterFailed(Exception ex)
	{
		PFLog.SmartConsole.Log("Arbiter failed");
		EventBus.RaiseEvent(delegate(IArbiterEventSubscriber wrn)
		{
			wrn.ArbiterFailed(ex);
		});
	}
}

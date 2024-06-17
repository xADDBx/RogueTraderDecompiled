using System;

namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookEvent
{
	bool IsTriggered { get; }

	IMechanicEntity Initiator { get; }

	bool IsGameLogDisabled { get; }

	Type RootType { get; }

	IMechanicEntity GetRuleTarget();

	void OnDidTrigger();

	void OnTrigger(RulebookEventContext rulebookEventContext);
}

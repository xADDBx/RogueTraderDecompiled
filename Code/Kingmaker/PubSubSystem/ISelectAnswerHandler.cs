using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISelectAnswerHandler : ISubscriber
{
	void HandleSelectAnswer(BlueprintAnswer answer);
}

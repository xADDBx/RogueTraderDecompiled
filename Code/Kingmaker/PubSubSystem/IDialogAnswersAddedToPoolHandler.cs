using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogAnswersAddedToPoolHandler : ISubscriber
{
	void HandleDialogAnswersAddedToPool(BlueprintAnswer answer);
}

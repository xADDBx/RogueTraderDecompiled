using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogStartHandler : ISubscriber
{
	void HandleDialogStarted(BlueprintDialog dialog);
}

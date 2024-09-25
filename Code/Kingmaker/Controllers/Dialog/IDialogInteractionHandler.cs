using JetBrains.Annotations;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.Dialog;

public interface IDialogInteractionHandler : ISubscriber
{
	void StartDialogInteraction([NotNull] BlueprintDialog dialog);

	void StopDialogInteraction([NotNull] BlueprintDialog dialog);
}

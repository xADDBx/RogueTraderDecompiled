using JetBrains.Annotations;

namespace Kingmaker.Controllers.Dialog;

internal interface IDialogControllerStartScheduledDialogImmediately
{
	void StartScheduledDialogImmediately([NotNull] DialogData dialog);
}

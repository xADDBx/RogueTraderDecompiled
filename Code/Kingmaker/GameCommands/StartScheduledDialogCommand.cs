using Kingmaker.Controllers.Dialog;

namespace Kingmaker.GameCommands;

public class StartScheduledDialogCommand : GameCommand
{
	private readonly DialogData m_DialogData;

	public StartScheduledDialogCommand(DialogData dialogData)
	{
		m_DialogData = dialogData;
	}

	protected override void ExecuteInternal()
	{
		((IDialogControllerStartScheduledDialogImmediately)Game.Instance.DialogController).StartScheduledDialogImmediately(m_DialogData);
	}
}

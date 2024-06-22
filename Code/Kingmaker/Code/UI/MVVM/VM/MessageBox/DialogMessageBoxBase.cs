namespace Kingmaker.Code.UI.MVVM.VM.MessageBox;

public abstract class DialogMessageBoxBase
{
	public enum BoxType
	{
		Message,
		Dialog,
		TextField,
		ProgressBar
	}

	public enum BoxButton
	{
		Close,
		Yes,
		No
	}
}

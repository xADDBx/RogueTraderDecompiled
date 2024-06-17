using Kingmaker.DialogSystem.Blueprints;

namespace Kingmaker.Blueprints.Root;

public static class DialogExtensions
{
	public static bool IsSystem(this BlueprintAnswer answer)
	{
		DialogRoot dialog = Game.Instance.BlueprintRoot.Dialog;
		if (!answer.Equals(dialog.ContinueAnswer) && !answer.Equals(dialog.ExitAnswer) && !answer.Equals(dialog.InterchapterContinueAnswer))
		{
			return answer.Equals(dialog.InterchapterExitAnswer);
		}
		return true;
	}

	public static bool IsContinue(this BlueprintAnswer answer)
	{
		DialogRoot dialog = Game.Instance.BlueprintRoot.Dialog;
		if (!answer.Equals(dialog.ContinueAnswer))
		{
			return answer.Equals(dialog.InterchapterContinueAnswer);
		}
		return true;
	}

	public static bool IsExit(this BlueprintAnswer answer)
	{
		DialogRoot dialog = Game.Instance.BlueprintRoot.Dialog;
		if (!answer.Equals(dialog.ExitAnswer))
		{
			return answer.Equals(dialog.InterchapterExitAnswer);
		}
		return true;
	}
}

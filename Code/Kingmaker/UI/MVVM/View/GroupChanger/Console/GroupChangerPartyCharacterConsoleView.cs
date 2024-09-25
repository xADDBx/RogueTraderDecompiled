namespace Kingmaker.UI.MVVM.View.GroupChanger.Console;

public class GroupChangerPartyCharacterConsoleView : GroupChangerCharacterBaseView
{
	protected override void SetState(bool isInParty, bool isLock)
	{
		base.gameObject.SetActive(isInParty || isLock);
	}
}

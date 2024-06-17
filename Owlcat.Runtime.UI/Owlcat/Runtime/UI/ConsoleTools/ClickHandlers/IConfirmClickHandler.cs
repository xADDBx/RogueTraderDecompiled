namespace Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

public interface IConfirmClickHandler : IConsoleEntity
{
	bool CanConfirmClick();

	void OnConfirmClick();

	string GetConfirmClickHint();
}

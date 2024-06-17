namespace Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

public interface ILongConfirmClickHandler : IConsoleEntity
{
	bool CanLongConfirmClick();

	void OnLongConfirmClick();

	string GetLongConfirmClickHint();
}

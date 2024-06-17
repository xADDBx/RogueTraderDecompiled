namespace Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

public interface ILongDeclineClickHandler : IConsoleEntity
{
	bool CanLongDeclineClick();

	void OnLongDeclineClick();

	string GetLongDeclineClickHint();
}

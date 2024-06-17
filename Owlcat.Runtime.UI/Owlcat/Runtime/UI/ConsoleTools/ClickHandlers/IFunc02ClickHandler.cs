namespace Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

public interface IFunc02ClickHandler : IConsoleEntity
{
	bool CanFunc02Click();

	void OnFunc02Click();

	string GetFunc02ClickHint();
}

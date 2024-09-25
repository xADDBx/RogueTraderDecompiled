namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public interface IConsoleNavigationEntity : IConsoleEntity
{
	void SetFocus(bool value);

	bool IsValid();
}

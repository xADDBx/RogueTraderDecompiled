namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public class EmptyConsoleNavigationEntity : IConsoleNavigationEntity, IConsoleEntity
{
	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return false;
	}
}

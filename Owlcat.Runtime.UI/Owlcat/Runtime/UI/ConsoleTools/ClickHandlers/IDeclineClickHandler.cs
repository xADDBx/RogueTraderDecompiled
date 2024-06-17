namespace Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

public interface IDeclineClickHandler : IConsoleEntity
{
	bool CanDeclineClick();

	void OnDeclineClick();

	string GetDeclineClickHint();
}

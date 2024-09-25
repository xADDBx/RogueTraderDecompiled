namespace Kingmaker.EntitySystem.Persistence;

public interface ILoadingScreen
{
	void ShowLoadingScreen();

	void HideLoadingScreen();

	LoadingScreenState GetLoadingScreenState();
}

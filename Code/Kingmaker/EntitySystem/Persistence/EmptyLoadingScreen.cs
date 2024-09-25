namespace Kingmaker.EntitySystem.Persistence;

public class EmptyLoadingScreen : ILoadingScreen
{
	public static EmptyLoadingScreen Instance = new EmptyLoadingScreen();

	private LoadingScreenState m_State;

	private EmptyLoadingScreen()
	{
	}

	public void ShowLoadingScreen()
	{
		m_State = LoadingScreenState.Shown;
	}

	public void HideLoadingScreen()
	{
		m_State = LoadingScreenState.Hidden;
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return m_State;
	}
}

using Kingmaker.EntitySystem.Persistence.Scenes;

namespace Kingmaker.IngameConsole;

public static class IngameConsoleInitializer
{
	public static void Init()
	{
		BundledSceneLoader.LoadSceneAsync("IngameConsole");
	}
}

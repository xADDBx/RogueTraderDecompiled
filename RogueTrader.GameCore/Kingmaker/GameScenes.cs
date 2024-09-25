using Kingmaker.Blueprints.Area;

namespace Kingmaker;

public static class GameScenes
{
	public const string Start = "Start";

	public const string BaseMechanics = "BaseMechanics";

	public const string EntityBounds = "EntityBounds";

	public const string SurfaceUI = "UI_Surface_Scene";

	public const string CommonUI = "UI_Common_Scene";

	public const string SpaceUI = "UI_Space_Scene";

	public const string LoadingScreen = "LoadingScreen";

	public const string Arbiter = "Arbiter";

	public const string IngameConsoleScene = "IngameConsole";

	public static readonly SceneReference SurfaceUIRef = new SceneReference("UI_Surface_Scene");

	public static readonly SceneReference SpaceUIRef = new SceneReference("UI_Space_Scene");

	public static readonly string[] ScenesToIncludeInBundles = new string[7] { MainMenu, "UI_Surface_Scene", "UI_Common_Scene", "UI_Space_Scene", "Arbiter", "IngameConsole", "LoadingScreen" };

	public static string MainMenu => "MainMenu";
}

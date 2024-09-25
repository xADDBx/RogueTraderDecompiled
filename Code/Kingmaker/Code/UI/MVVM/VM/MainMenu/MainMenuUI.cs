namespace Kingmaker.Code.UI.MVVM.VM.MainMenu;

public static class MainMenuUI
{
	public static MainMenuVM Instance;

	public static bool IsActive => Instance != null;
}

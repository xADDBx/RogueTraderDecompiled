namespace Kingmaker.Code.UI.MVVM.VM.MainMenu;

public interface IUIMainMenu
{
	void ShowNewGameSetup();

	void LoadLastGame();

	void OpenSettings();

	void ShowNetLobby();

	void ShowDlcManager();

	void Exit();

	void ShowLicense();

	void ShowCredits();

	void ShowFeedback();
}

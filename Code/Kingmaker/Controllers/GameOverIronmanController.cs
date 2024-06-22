using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Settings;

namespace Kingmaker.Controllers;

public class GameOverIronmanController : IControllerEnable, IController
{
	public void OnEnable()
	{
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			PFLog.System.Log("Deleting ironman save: " + Game.Instance.SaveManager.GetIronmanSave().FolderName);
			SaveInfo ironmanSave = Game.Instance.SaveManager.GetIronmanSave();
			Game.Instance.SaveManager.DowngradeSaveFromIronMan(ironmanSave);
		}
		LoadingProcess.Instance.ResetManualLoadingScreen();
	}
}

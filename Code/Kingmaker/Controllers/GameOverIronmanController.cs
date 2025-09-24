using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Controllers;

public class GameOverIronmanController : IControllerEnable, IController
{
	public void OnEnable()
	{
		LoadingProcess.Instance.ResetManualLoadingScreen();
	}
}

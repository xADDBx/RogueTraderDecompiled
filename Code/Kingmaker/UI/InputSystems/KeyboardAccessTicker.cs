using Kingmaker.Controllers.Interfaces;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.UI.InputSystems;

public class KeyboardAccessTicker : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.BeginOfFrame;
	}

	public void Tick()
	{
		Services.GetInstance<KeyboardAccess>().Tick();
	}
}

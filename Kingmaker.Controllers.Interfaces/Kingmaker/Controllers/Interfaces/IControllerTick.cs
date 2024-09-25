namespace Kingmaker.Controllers.Interfaces;

public interface IControllerTick : IController
{
	TickType GetTickType();

	void Tick();
}

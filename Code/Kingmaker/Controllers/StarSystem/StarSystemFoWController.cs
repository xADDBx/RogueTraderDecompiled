using Kingmaker.Controllers.Interfaces;
using Kingmaker.View;

namespace Kingmaker.Controllers.StarSystem;

public class StarSystemFoWController : IControllerEnable, IController, IControllerDisable
{
	public void OnEnable()
	{
		Game.Instance.Player.PlayerShip.View.SureFogOfWarRevealer();
	}

	public void OnDisable()
	{
		FogOfWarRevealerSettings fogOfWarRevealer = Game.Instance.Player.PlayerShip.View.FogOfWarRevealer;
		if ((bool)fogOfWarRevealer)
		{
			fogOfWarRevealer.DefaultRadius = true;
		}
	}
}

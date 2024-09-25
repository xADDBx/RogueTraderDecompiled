using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers.Projectiles;

public class ProjectileHitController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles)
		{
			if (projectile.IsHit && !projectile.IsHitHandled)
			{
				projectile.HandleHit();
			}
		}
	}
}

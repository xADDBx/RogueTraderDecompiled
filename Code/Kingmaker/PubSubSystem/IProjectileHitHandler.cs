using Kingmaker.Controllers.Projectiles;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IProjectileHitHandler : ISubscriber
{
	void HandleProjectileHit(Projectile projectile);
}

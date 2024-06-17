using Kingmaker.Controllers.Projectiles;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IProjectileLaunchedHandler : ISubscriber
{
	void HandleProjectileLaunched(Projectile projectile);
}

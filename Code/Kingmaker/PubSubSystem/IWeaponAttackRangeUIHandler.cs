using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IWeaponAttackRangeUIHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleShowAttackRange(bool state);
}

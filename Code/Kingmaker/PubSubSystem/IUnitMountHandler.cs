using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitMountHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitMount(BaseUnitEntity mount);

	void HandleUnitDismount([CanBeNull] BaseUnitEntity mount);
}

using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;

namespace Kingmaker.PubSubSystem;

public interface IUnitFeaturesHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleFeatureAdded(FeatureCountableFlag feature);

	void HandleFeatureRemoved(FeatureCountableFlag feature);
}
public interface IUnitFeaturesHandler<TTag> : IUnitFeaturesHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitFeaturesHandler, TTag>
{
}

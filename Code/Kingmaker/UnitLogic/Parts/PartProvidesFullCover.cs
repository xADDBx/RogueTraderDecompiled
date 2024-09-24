using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Covers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartProvidesFullCover : UnitPart, IUnitFeaturesHandler<EntitySubscriber>, IUnitFeaturesHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitFeaturesHandler, EntitySubscriber>, IDynamicCoverProvider, IHashable
{
	public NodeList Nodes => CustomGridNodeController.GetUnitNodes(base.Owner);

	public LosCalculations.CoverType CoverType => LosCalculations.CoverType.Full;

	public void HandleFeatureAdded(FeatureCountableFlag feature)
	{
		if (base.Owner.Features.ProvidesFullCover.Type == feature.Type)
		{
			Game.Instance.ForcedCoversController.RegisterCoverProvider(this);
		}
	}

	public void HandleFeatureRemoved(FeatureCountableFlag feature)
	{
		if (base.Owner.Features.ProvidesFullCover.Type == feature.Type)
		{
			Game.Instance.ForcedCoversController.UnregisterCoverProvider(this);
		}
	}

	protected override void OnDetach()
	{
		base.OnDetach();
		Game.Instance.ForcedCoversController.UnregisterCoverProvider(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

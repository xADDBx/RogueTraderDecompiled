using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
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
	private readonly HashSet<BaseUnitEntity> exclusiveUsers = new HashSet<BaseUnitEntity>();

	public NodeList Nodes
	{
		get
		{
			if (!base.Owner.IsInGame)
			{
				return NodeList.Empty;
			}
			return CustomGridNodeController.GetUnitNodes(base.Owner);
		}
	}

	public LosCalculations.CoverType CoverType
	{
		get
		{
			if (!base.Owner.Features.ProvidesFullCover)
			{
				if (!base.Owner.Features.ProvidesHalfCover)
				{
					return LosCalculations.CoverType.None;
				}
				return LosCalculations.CoverType.Half;
			}
			return LosCalculations.CoverType.Full;
		}
	}

	public void AddExclusiveUser(BaseUnitEntity user)
	{
		exclusiveUsers.Add(user);
	}

	public void RemoveExclusiveUser(BaseUnitEntity user)
	{
		exclusiveUsers.Remove(user);
	}

	public void HandleFeatureAdded(FeatureCountableFlag feature)
	{
		if (!Game.Instance.ForcedCoversController.ContainsCoverProvider(this) && (base.Owner.Features.ProvidesFullCover.Type == feature.Type || base.Owner.Features.ProvidesHalfCover.Type == feature.Type))
		{
			Game.Instance.ForcedCoversController.RegisterCoverProvider(this);
		}
	}

	public void HandleFeatureRemoved(FeatureCountableFlag feature)
	{
		if (Game.Instance.ForcedCoversController.ContainsCoverProvider(this) && (base.Owner.Features.ProvidesFullCover.Type == feature.Type || base.Owner.Features.ProvidesHalfCover.Type == feature.Type))
		{
			Game.Instance.ForcedCoversController.UnregisterCoverProvider(this);
		}
	}

	protected override void OnDetach()
	{
		base.OnDetach();
		Game.Instance.ForcedCoversController.UnregisterCoverProvider(this);
	}

	public bool CanUseCover(BaseUnitEntity entity)
	{
		if (exclusiveUsers.Count != 0)
		{
			return exclusiveUsers.Contains(entity);
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

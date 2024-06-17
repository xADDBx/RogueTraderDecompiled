using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.PathRenderer;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("cd154b06267b1c74380412abac2289e9")]
public class WarhammerStarshipVantagePoints : UnitBuffComponentDelegate, IStarshipVantagePointsHandler<EntitySubscriber>, IStarshipVantagePointsHandler, ISubscriber<IStarshipEntity>, ISubscriber, IEventTag<IStarshipVantagePointsHandler, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[Range(1f, 100f)]
	private int PercentAmongReachableNodes;

	[SerializeField]
	private ActionList ActionsOnEnterVantagePoint;

	[SerializeField]
	private ActionList ActionsOnLeaveVantagePoint;

	private PartVantagePoints VantagePointsPart => base.Owner.GetOrCreate<PartVantagePoints>();

	protected override void OnActivate()
	{
		if (!(base.Owner is StarshipEntity starshipEntity))
		{
			PFLog.Default.Error("Failed to show Vantage points. Owner expected to be a ship");
			return;
		}
		VantagePointsPart.DetectVantagePoints(PercentAmongReachableNodes);
		ShipPathManager.Instance.UpdateVantagePointsMarkers(starshipEntity, starshipEntity.Navigation.ReachableTiles);
	}

	protected override void OnDeactivate()
	{
		if (VantagePointsPart.IsInVantagePoint)
		{
			RunActionsOnLeaveVantagePoint();
		}
		VantagePointsPart.ClearVantagePoints();
	}

	public void HandleEnteredVantagePoint()
	{
		RunActionsOnEnterVantagePoint();
	}

	public void HandleLeavedVantagePoint()
	{
		RunActionsOnLeaveVantagePoint();
	}

	private void RunActionsOnEnterVantagePoint()
	{
		using (base.Context.GetDataScope(base.OwnerTargetWrapper))
		{
			ActionsOnEnterVantagePoint.Run();
		}
	}

	private void RunActionsOnLeaveVantagePoint()
	{
		using (base.Context.GetDataScope(base.OwnerTargetWrapper))
		{
			ActionsOnLeaveVantagePoint.Run();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		OnDeactivate();
		OnActivate();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

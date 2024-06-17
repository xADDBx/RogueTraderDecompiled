using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class SpaceCombatPointMarkersVM : SpacePointMarkersVM, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IUnitBecameVisibleHandler, ISubscriber<IEntity>, IUnitBecameInvisibleHandler
{
	private bool m_IsTurnBasedMode;

	protected override IEnumerable<BaseUnitEntity> UnitsSelector => Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => unit.IsInCombat && (unit.Faction.IsPlayerEnemy || unit.Faction.IsPlayer) && unit.IsVisibleForPlayer && !unit.LifeState.IsDead);

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			UpdateUnits();
		}
		else
		{
			Clear();
		}
	}

	public void OnEntityBecameVisible()
	{
		UpdateUnits();
	}

	public void OnEntityBecameInvisible()
	{
		UpdateUnits();
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateUnits();
	}
}

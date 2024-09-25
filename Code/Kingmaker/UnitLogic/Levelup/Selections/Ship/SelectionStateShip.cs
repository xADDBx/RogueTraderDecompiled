using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections.Ship;

public class SelectionStateShip : SelectionState
{
	private bool m_Selected;

	public BaseUnitEntity Ship { get; private set; }

	public SelectionStateShip([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
		: base(manager, blueprint, path, pathRank)
	{
	}

	protected override bool IsMadeInternal()
	{
		return m_Selected;
	}

	protected override bool IsValidInternal()
	{
		return true;
	}

	protected override bool CanSelectAnyInternal()
	{
		return true;
	}

	public void SelectShip(BaseUnitEntity ship)
	{
		Ship = ship;
		m_Selected = true;
		NotifySelectionChanged();
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		if (Ship != null)
		{
			Game.NewGameShip = Ship;
			Game.Instance.Player.SetMainStarship(Ship);
		}
	}

	protected override void InvalidateInternal()
	{
	}
}

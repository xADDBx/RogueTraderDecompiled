using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections.Doll;

public class SelectionStateDoll : SelectionState
{
	private DollState m_State;

	private bool m_Selected;

	public SelectionStateDoll([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
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

	public void Select(DollState state)
	{
		m_State = state.Copy();
		m_Selected = true;
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		unit.ViewSettings.SetDoll(m_State.CreateData());
	}

	protected override void InvalidateInternal()
	{
	}
}

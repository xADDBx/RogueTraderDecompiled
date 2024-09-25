using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections.Portrait;

public class SelectionStatePortrait : SelectionState
{
	private BlueprintPortrait m_Portrait;

	private bool m_Selected;

	public SelectionStatePortrait([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
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

	public void SelectPortrait(BlueprintPortrait portrait)
	{
		m_Portrait = portrait;
		m_Selected = true;
		NotifySelectionChanged();
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		unit.UISettings.SetPortrait(m_Portrait);
	}

	protected override void InvalidateInternal()
	{
	}
}

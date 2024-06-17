using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;

public class SelectionStateGender : SelectionState
{
	private Gender m_Gender;

	private bool m_Selected;

	public SelectionStateGender([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
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

	public void SelectGender(Gender gender)
	{
		m_Gender = gender;
		m_Selected = true;
		NotifySelectionChanged();
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		unit.Description.SetGender(m_Gender);
	}

	protected override void InvalidateInternal()
	{
	}
}

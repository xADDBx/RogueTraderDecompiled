using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections;

public abstract class SelectionState : SelectionState.IInvalidateAccess
{
	internal interface IInvalidateAccess
	{
		void Invalidate();
	}

	[NotNull]
	public LevelUpManager Manager { get; }

	[NotNull]
	public BlueprintSelection Blueprint { get; }

	[NotNull]
	public BlueprintPath Path { get; }

	public int PathRank { get; }

	public bool IsMade => IsMadeInternal();

	public bool IsValid => IsValidInternal();

	public bool CanSelectAny => CanSelectAnyInternal();

	protected SelectionState([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
	{
		Manager = manager;
		Blueprint = blueprint;
		Path = path;
		PathRank = pathRank;
	}

	public void Apply(BaseUnitEntity unit)
	{
		bool flag = true;
		if (Blueprint is BlueprintSelectionFeature blueprintSelectionFeature)
		{
			flag = blueprintSelectionFeature.MeetPrerequisites(unit);
		}
		if (IsMade && IsValid && flag)
		{
			ApplyInternal(unit);
		}
	}

	void IInvalidateAccess.Invalidate()
	{
		InvalidateInternal();
	}

	protected void NotifySelectionChanged()
	{
		((LevelUpManager.IOnSelectionChangedAccess)Manager).OnSelectionChanged(this);
	}

	protected abstract bool IsMadeInternal();

	protected abstract bool IsValidInternal();

	protected abstract bool CanSelectAnyInternal();

	protected abstract void ApplyInternal(BaseUnitEntity unit);

	protected abstract void InvalidateInternal();
}

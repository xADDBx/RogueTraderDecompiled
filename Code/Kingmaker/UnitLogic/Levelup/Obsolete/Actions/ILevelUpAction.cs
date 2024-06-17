using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[Obsolete]
public interface ILevelUpAction
{
	LevelUpActionPriority Priority { get; }

	bool Check([NotNull] LevelUpState state, [NotNull] BaseUnitEntity unit);

	void Apply([NotNull] LevelUpState state, [NotNull] BaseUnitEntity unit);

	void PostLoad();
}

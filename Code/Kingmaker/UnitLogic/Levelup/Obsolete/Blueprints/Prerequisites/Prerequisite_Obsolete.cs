using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintCharacterClass))]
[TypeId("fc3c6748ac88410438982853af380dc1")]
[AllowedOn(typeof(BlueprintArchetype))]
public abstract class Prerequisite_Obsolete : BlueprintComponent
{
	public enum GroupType
	{
		All,
		Any,
		ForcedTrue
	}

	public GroupType Group;

	public bool CheckInProgression;

	public bool HideInUI;

	public abstract bool Check([CanBeNull] FeatureSelectionState selectionState, [NotNull] BaseUnitEntity unit, [CanBeNull] LevelUpState state);

	public virtual void Restrict([CanBeNull] FeatureSelectionState selectionState, [NotNull] BaseUnitEntity unit, [NotNull] LevelUpState state)
	{
	}

	public abstract string GetUIText(BaseUnitEntity unit);
}

using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.FactLogic;

public interface IHiddenUnitFacts
{
	[CanBeNull]
	BlueprintRace ReplaceRace { get; }

	HashSet<BlueprintFact> Facts { get; }
}

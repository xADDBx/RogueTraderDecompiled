using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Globalmap.CombatRandomEncounters;
using Owlcat.QA.Validation;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("6c0221930c164574eabd26acbc7cc934")]
public class AddTags : BlueprintComponent
{
	public UnitRole RandomEncounterRole;

	public bool IsRanged;

	public bool IsCaster;

	[ValidateNotNull]
	public UnitTag[] Tags = new UnitTag[0];
}

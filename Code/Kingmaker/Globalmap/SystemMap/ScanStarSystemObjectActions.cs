using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

[AllowedOn(typeof(BlueprintStarSystemObject))]
[TypeId("fbf24398eea844fab0cfa40166a129c2")]
public class ScanStarSystemObjectActions : BlueprintComponent
{
	[SerializeField]
	public ConditionsChecker Conditions;

	[SerializeField]
	public ActionList AdditionalActions;
}

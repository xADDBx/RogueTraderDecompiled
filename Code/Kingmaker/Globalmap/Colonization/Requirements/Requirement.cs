using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[TypeId("76a0aedc87e64d9e95518853c442388e")]
public abstract class Requirement : BlueprintComponent
{
	public bool HideInUI;

	public abstract bool Check(Colony colony = null);

	public abstract void Apply(Colony colony = null);
}

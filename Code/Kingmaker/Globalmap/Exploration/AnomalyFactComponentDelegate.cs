using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[AllowedOn(typeof(BlueprintAnomalyFact))]
public abstract class AnomalyFactComponentDelegate : EntityFactComponentDelegate<AnomalyEntityData>, IHashable
{
	protected new AnomalyFact Fact => (AnomalyFact)base.Fact;

	protected FeatureParam Param => ComponentEventContext.CurrentRuntime.Param;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

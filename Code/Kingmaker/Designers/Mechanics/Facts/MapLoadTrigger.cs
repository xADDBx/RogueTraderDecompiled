using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("7215c0c9ca244c829715f4d7ad9385ff")]
public class MapLoadTrigger : UnitFactComponentDelegate, IPartyLeaveAreaHandler, ISubscriber, IHashable
{
	public ActionList AreaUnloadActions;

	public void HandlePartyLeaveArea(BlueprintArea currentArea, BlueprintAreaEnterPoint targetArea)
	{
		if (currentArea == targetArea.Area)
		{
			return;
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(AreaUnloadActions, base.Owner.ToITargetWrapper());
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

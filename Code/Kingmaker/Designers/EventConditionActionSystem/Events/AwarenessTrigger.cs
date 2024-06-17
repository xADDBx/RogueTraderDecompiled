using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("e7e916cf1fcdef14a81da6a7fff47811")]
public class AwarenessTrigger : EntityFactComponentDelegate, IUnitSpottedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAwarenessHandler, ISubscriber<IMapObjectEntity>, IHashable
{
	[CanBeNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[CanBeNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	[NotNull]
	public ActionList OnSpotted;

	public void HandleUnitSpotted(BaseUnitEntity spottedBy)
	{
		if (Unit != null && EventInvokerExtensions.BaseUnitEntity == Unit.GetValue())
		{
			OnSpotted.Run();
		}
	}

	public void OnEntityNoticed(BaseUnitEntity character)
	{
		if (MapObject != null && EventInvokerExtensions.GetEntity<MapObjectEntity>() == MapObject.GetValue())
		{
			OnSpotted.Run();
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

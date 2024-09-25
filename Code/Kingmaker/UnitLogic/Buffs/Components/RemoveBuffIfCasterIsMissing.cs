using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("188e834654fcf9a43af76ede9ba714e7")]
public class RemoveBuffIfCasterIsMissing : UnitBuffComponentDelegate, IAreaHandler, ISubscriber, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IHashable
{
	public bool RemoveOnCasterDeath = true;

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (base.Context.MaybeCaster == null)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
		if (base.Context.MaybeCaster == null)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (RemoveOnCasterDeath && base.Context.MaybeCaster == baseUnitEntity)
		{
			base.Buff.Remove();
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

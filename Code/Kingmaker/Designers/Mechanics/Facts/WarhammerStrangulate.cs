using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("6db23c586b23455a9dadddc032b67a83")]
public class WarhammerStrangulate : UnitBuffComponentDelegate, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	protected override void OnActivate()
	{
		(base.Context?.MaybeCaster)?.GetOrCreate<WarhammerUnitPartStrangulateController>().NewBuff(base.Buff);
	}

	protected override void OnDeactivate()
	{
		(base.Context?.MaybeCaster)?.GetOptional<WarhammerUnitPartStrangulateController>()?.RemoveBuff(base.Buff);
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
		(base.Context?.MaybeCaster)?.GetOptional<WarhammerUnitPartStrangulateController>()?.RemoveBuff(base.Buff);
	}

	public void HandleUnitDeath()
	{
		(base.Context?.MaybeCaster)?.GetOptional<WarhammerUnitPartStrangulateController>()?.RemoveBuff(base.Buff);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Paths;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add calculated hit points")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d32a32fa01d208c4bbf31462ec510fb3")]
public class ToughnessLogic : UnitFactComponentDelegate, IUnitGainPathRankHandler<EntitySubscriber>, IUnitGainPathRankHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitGainPathRankHandler, EntitySubscriber>, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		UpdateModifier();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Health.HitPoints.RemoveModifiersFrom(base.Runtime);
	}

	public void HandleUnitGainPathRank(BlueprintPath path)
	{
		UpdateModifier();
	}

	private void UpdateModifier()
	{
		base.Owner.Health.HitPoints.RemoveModifiersFrom(base.Runtime);
		int value = (base.Owner.Progression.CharacterLevel + 1) / 2;
		base.Owner.Health.HitPoints.AddModifier(value, base.Runtime);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("`t|SourceUnit` - unit (or pet's master if IsPet enabled)\n`t|Descriptor` - pet if IsPet enabled")]
[TypeId("b8897f82ab7440244bbb721caf387a47")]
public class TutorialTriggerUnitDeath : TutorialTrigger, IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public bool IsPet;

	public void OnUnitDie()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit.IsPet != IsPet || (unit.Blueprint.Faction != BlueprintRoot.Instance.PlayerFaction && unit.Master?.Blueprint.Faction != BlueprintRoot.Instance.PlayerFaction))
		{
			return;
		}
		UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.ExCompanion)
		{
			return;
		}
		if (IsPet)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit.Master;
				context[TutorialContextKey.Descriptor] = unit;
			});
		}
		else
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
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

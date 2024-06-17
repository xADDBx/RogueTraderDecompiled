using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("4a6f362c4d6bff24c83ef48f157dcab7")]
public class WarhammerExtendBuffWithChild : UnitFactComponentDelegate, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_parenttBuff;

	[SerializeField]
	private BlueprintBuffReference m_childBuff;

	public BlueprintBuff ParentBuff => m_parenttBuff?.Get();

	public BlueprintBuff ChildBuff => m_childBuff?.Get();

	public void HandleBuffDidAdded(Buff addedBuff)
	{
		if (addedBuff.Blueprint == ParentBuff)
		{
			Buff buff = addedBuff.Owner.Buffs.Add(ChildBuff, base.Context);
			if (buff != null)
			{
				addedBuff.StoreFact(buff);
			}
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
		HandleBuffDidAdded(buff);
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

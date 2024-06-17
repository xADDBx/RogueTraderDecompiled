using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("781c3882fb08d5445b1588a4b0f4d9c7")]
public abstract class EtudeBracketOverrideInteraction : EtudeBracketTrigger, IEtudeBracketOverrideInteraction, IHashable
{
	[FormerlySerializedAs("Distance")]
	public int m_Distance = 2;

	public override bool RequireLinkedArea => true;

	public int Distance => m_Distance;

	public abstract AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

using System;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Alignments;

[Obsolete]
public class PartUnitAlignment : BaseUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitAlignment>, IEntityPartOwner
	{
		PartUnitAlignment Alignment { get; }
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

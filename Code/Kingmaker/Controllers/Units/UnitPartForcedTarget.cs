using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitPartForcedTarget : BaseUnitPart, IHashable
{
	private EntityRef<BaseUnitEntity> m_UnitRef;

	[CanBeNull]
	public UnitCommandHandle CmdHandle { get; set; }

	[CanBeNull]
	public BaseUnitEntity Unit
	{
		get
		{
			return m_UnitRef;
		}
		set
		{
			m_UnitRef = value;
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

using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class EntityPartStayOnPlatform : MechanicEntityPart<MechanicEntity>, IHashable
{
	private readonly List<PlatformObjectEntity> m_Platforms = new List<PlatformObjectEntity>();

	public void SetOnPlatform(PlatformObjectEntity platform)
	{
		if (m_Platforms.Contains(platform))
		{
			throw new Exception($"Trying to place the entity on a platform that already has the entity! Platform={platform} entity={base.Owner}");
		}
		m_Platforms.Add(platform);
		platform.AddEntity(base.Owner);
	}

	public void ReleaseFromPlatform(PlatformObjectEntity platform)
	{
		if (!m_Platforms.Contains(platform))
		{
			throw new Exception($"Trying to remove the entity from a platform that do not yet have this entity! Platform={platform} entity={base.Owner}");
		}
		platform.RemoveEntity(base.Owner);
		m_Platforms.Remove(platform);
		if (base.Owner is AbstractUnitEntity abstractUnitEntity)
		{
			abstractUnitEntity.View.ForcePlaceAboveGround();
		}
	}

	public bool IsOnPlatform()
	{
		return 0 < m_Platforms.Count;
	}

	public bool IsOnPlatform(PlatformObjectEntity platform)
	{
		return m_Platforms.Contains(platform);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

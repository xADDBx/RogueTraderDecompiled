using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartPartyLock : BaseUnitPart, IHashable
{
	[JsonProperty]
	private bool m_Locked;

	public bool Locked => m_Locked;

	public void Lock()
	{
		m_Locked = true;
	}

	public void Unlock()
	{
		m_Locked = false;
	}

	public static bool IsLocked(BaseUnitEntity entity)
	{
		return entity.GetOptional<PartPartyLock>()?.Locked ?? false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Locked);
		return result;
	}
}

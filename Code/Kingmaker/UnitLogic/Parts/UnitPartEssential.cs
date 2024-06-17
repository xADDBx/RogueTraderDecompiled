using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartEssential : BaseUnitPart, IHashable
{
	[JsonProperty]
	private int m_RetainCount;

	public void Retain()
	{
		m_RetainCount++;
		PFLog.History.Party.Log($"UnitPartEssential.Retain: {base.Owner.ToString()}, count {m_RetainCount}");
	}

	public void Release()
	{
		m_RetainCount--;
		PFLog.History.Party.Log($"UnitPartEssential.Release: {base.Owner.ToString()}, count {m_RetainCount}");
		if (m_RetainCount < 1)
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_RetainCount);
		return result;
	}
}

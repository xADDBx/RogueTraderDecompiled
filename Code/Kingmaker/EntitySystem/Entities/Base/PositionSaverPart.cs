using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

public class PositionSaverPart : ViewBasedPart, IHashable
{
	[JsonProperty]
	private Vector3? m_Position;

	protected override void OnPreSave()
	{
		base.OnPreSave();
		m_Position = base.Owner.Position;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (m_Position.HasValue)
		{
			base.Owner.Position = m_Position.Value;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (m_Position.HasValue)
		{
			Vector3 val2 = m_Position.Value;
			result.Append(ref val2);
		}
		return result;
	}
}

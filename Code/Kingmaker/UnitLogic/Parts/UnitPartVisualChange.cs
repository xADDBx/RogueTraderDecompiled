using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Animation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartVisualChange : UnitPart, IHashable
{
	private AnimationSet m_AnimationSetOverride;

	public AnimationSet AnimationSetOverride
	{
		get
		{
			return m_AnimationSetOverride;
		}
		set
		{
			m_AnimationSetOverride = value;
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

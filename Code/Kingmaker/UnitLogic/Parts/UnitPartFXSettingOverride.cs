using System.Collections.Generic;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFXSettingOverride : UnitPart, IHashable
{
	private Dictionary<UnitAnimationActionLink, UnitAnimationActionLink> m_ActionsOverride = new Dictionary<UnitAnimationActionLink, UnitAnimationActionLink>();

	public Dictionary<UnitAnimationActionLink, UnitAnimationActionLink> ActionsOverride => m_ActionsOverride;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("89ff8f2696e56394eb9773f845f5e286")]
public class EtudeBracketAllowAugmentsTier : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	[Tooltip("An augment tier which is going to be allowed")]
	private AugmentTier m_AugmentTierToAllow;

	private AugmentTier m_DefaultTier;

	protected override void OnEnter()
	{
		Game.Instance.Player.PartyAugmentManager.SetCurrentAvailableTier(m_AugmentTierToAllow);
	}

	protected override void OnResume()
	{
		Game.Instance.Player.PartyAugmentManager.SetCurrentAvailableTier(m_AugmentTierToAllow);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartyAugmentManager : EntityPart<Player>, IHashable
{
	private AugmentTier m_CurrentAvailableTier;

	public AugmentTier CurrentAvailableTier => m_CurrentAvailableTier;

	public bool ShouldShowAttentionMarker
	{
		get
		{
			if (StoreManager.CheckIfDlcPurchasedAndInstalled(DlcNameEnum.DLC3TheInfiniteMuseion) && CanEquipAugment(AugmentTier.Tier1))
			{
				return !base.Owner.AugmentationsInSpaceMarkerSeen;
			}
			return false;
		}
	}

	public void SetCurrentAvailableTier(AugmentTier tier)
	{
		m_CurrentAvailableTier = tier;
	}

	public bool CanEquipAugment(AugmentTier tier)
	{
		return tier <= m_CurrentAvailableTier;
	}

	public void MarkAttentionMarkerSeen()
	{
		base.Owner.AugmentationsInSpaceMarkerSeen = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

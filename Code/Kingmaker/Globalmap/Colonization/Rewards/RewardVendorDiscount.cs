using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("3944ab5e54f6449f91fab75d1909a8b5")]
public class RewardVendorDiscount : Reward
{
	[SerializeField]
	public FactionType Faction;

	[SerializeField]
	public int Discount;

	public override void ReceiveReward(Colony colony = null)
	{
		VendorLogic.AddDiscount(Faction, Discount);
	}
}

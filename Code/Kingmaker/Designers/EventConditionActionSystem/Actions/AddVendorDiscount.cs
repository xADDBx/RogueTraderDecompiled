using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Enums;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("1f6e13c5c1d94b6786ccb4aa8585cb3e")]
[PlayerUpgraderAllowed(false)]
public class AddVendorDiscount : GameAction
{
	[SerializeField]
	private FactionType m_Faction;

	[SerializeField]
	private int m_Discount;

	public override string GetCaption()
	{
		return $"Add {m_Discount} to all deals for {m_Faction}";
	}

	protected override void RunAction()
	{
		VendorLogic.AddDiscount(m_Faction, m_Discount);
	}
}

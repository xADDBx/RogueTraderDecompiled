using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Ecnchantments;

[TypeId("7d0571abd8571a74987650ccb78d5b5e")]
public abstract class BlueprintItemEnchantment : BlueprintMechanicEntityFact
{
	[SerializeField]
	private bool m_AllowNonContextActions;

	[SerializeField]
	private LocalizedString m_EnchantName;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private LocalizedString m_Prefix;

	[SerializeField]
	private LocalizedString m_Suffix;

	[SerializeField]
	private int m_EnchantmentCost;

	[SerializeField]
	private int m_IdentifyDC;

	public string Prefix => m_Prefix;

	public string Suffix => m_Suffix;

	public int EnchantmentCost => m_EnchantmentCost;

	public int IdentifyDC => m_IdentifyDC;

	public override bool AllowContextActionsOnly => !m_AllowNonContextActions;

	protected override Type GetFactType()
	{
		return typeof(ItemEnchantment);
	}
}

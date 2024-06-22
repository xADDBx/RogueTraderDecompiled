using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("98a70bd643e045cf96f35575072962f8")]
public class OpenVendorSelectingWindow : GameAction
{
	[Serializable]
	public class VendorEntry
	{
		public string Name;

		[SerializeField]
		public ConditionsChecker ShowConditions;

		[SerializeReference]
		[ValidateNotNull]
		public MechanicEntityEvaluator Vendor;
	}

	[ValidateNotNull]
	public VendorEntry[] Vendors;

	public override string GetCaption()
	{
		return "Open vendor selecting window";
	}

	protected override void RunAction()
	{
		List<MechanicEntity> vendors = null;
		if (Vendors != null)
		{
			vendors = new List<MechanicEntity>();
			VendorEntry[] vendors2 = Vendors;
			foreach (VendorEntry vendorEntry in vendors2)
			{
				ConditionsChecker showConditions = vendorEntry.ShowConditions;
				if ((showConditions != null && showConditions.HasConditions && !vendorEntry.ShowConditions.Check()) || !vendorEntry.Vendor.TryGetValue(out var value) || value == null)
				{
					continue;
				}
				vendors.Add(value);
				PartVendor optional = value.GetOptional<PartVendor>();
				if (optional == null)
				{
					continue;
				}
				foreach (ItemEntity item in optional)
				{
					item.Identify();
				}
			}
		}
		EventBus.RaiseEvent(delegate(IBeginSelectingVendorHandler h)
		{
			h.HandleBeginSelectingVendor(vendors);
		});
	}
}

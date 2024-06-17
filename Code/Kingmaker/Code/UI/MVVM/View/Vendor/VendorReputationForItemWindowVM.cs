using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorReputationForItemWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly AutoDisposingReactiveCollection<VendorReputationForItemVM> AcceptItems = new AutoDisposingReactiveCollection<VendorReputationForItemVM>();

	private VendorLogic Vendor = VendorHelper.Vendor;

	public VendorReputationForItemWindowVM(List<ItemsItemOrigin> types)
	{
		Dictionary<ItemsItemOrigin, int> dictionary = new Dictionary<ItemsItemOrigin, int>();
		foreach (ItemsItemOrigin type in types)
		{
			if (Vendor.VendorFaction.CargoTypes.Contains(type))
			{
				BlueprintCargoRoot.CargoTemplate template = Game.Instance.BlueprintRoot.SystemMechanics.CargoRoot.GetTemplate(type);
				if (template.ReputationPointsCost > 0)
				{
					dictionary.Add(type, template.ReputationPointsCost);
				}
			}
		}
		foreach (KeyValuePair<ItemsItemOrigin, int> item in dictionary.OrderBy(delegate(KeyValuePair<ItemsItemOrigin, int> item)
		{
			KeyValuePair<ItemsItemOrigin, int> keyValuePair = item;
			return keyValuePair.Value;
		}).Reverse())
		{
			AcceptItems.Add(new VendorReputationForItemVM(item.Key, item.Value));
		}
	}

	protected override void DisposeImplementation()
	{
		AcceptItems.Clear();
	}
}

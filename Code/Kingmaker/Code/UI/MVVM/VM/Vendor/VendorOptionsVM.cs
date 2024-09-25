using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorOptionsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<VendorOptionsItemVM> ItemVms = new List<VendorOptionsItemVM>();

	private Dictionary<VendorHelper.SaleOptions, bool> SettingsDictionary => Game.Instance.Player.UISettings.OptionsDictionary;

	public VendorOptionsVM()
	{
		AddOptionsDictionary(VendorHelper.SaleOptions.MasterWork, defaultValue: false);
		AddOptionsDictionary(VendorHelper.SaleOptions.NonMagical, defaultValue: false);
		AddOptionsDictionary(VendorHelper.SaleOptions.GemsAnimalParts, defaultValue: true);
		foreach (KeyValuePair<VendorHelper.SaleOptions, bool> item in SettingsDictionary)
		{
			ItemVms.Add(new VendorOptionsItemVM(item.Key));
		}
	}

	protected override void DisposeImplementation()
	{
		ItemVms.ForEach(delegate(VendorOptionsItemVM vm)
		{
			vm.Dispose();
		});
		ItemVms.Clear();
	}

	private void AddOptionsDictionary(VendorHelper.SaleOptions key, bool defaultValue)
	{
		if (!SettingsDictionary.ContainsKey(key))
		{
			SettingsDictionary.Add(key, defaultValue);
		}
	}
}

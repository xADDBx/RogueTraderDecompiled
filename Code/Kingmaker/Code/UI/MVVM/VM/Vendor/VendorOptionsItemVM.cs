using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorOptionsItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly VendorHelper.SaleOptions m_Key;

	public readonly ReactiveProperty<string> Title = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> State = new ReactiveProperty<bool>(initialValue: false);

	private Dictionary<VendorHelper.SaleOptions, bool> SettingsDictionary => Game.Instance.Player.UISettings.OptionsDictionary;

	public VendorOptionsItemVM(VendorHelper.SaleOptions options)
	{
		m_Key = options;
		ReactiveProperty<string> title = Title;
		LocalizedString localizedString = UIUtility.GetGlossaryEntry(m_Key.ToString())?.Title;
		title.Value = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		State.Value = SettingsDictionary[m_Key];
	}

	protected override void DisposeImplementation()
	{
	}

	public void SwitchOption()
	{
		SettingsDictionary[m_Key] = !SettingsDictionary[m_Key];
		State.Value = SettingsDictionary[m_Key];
	}
}

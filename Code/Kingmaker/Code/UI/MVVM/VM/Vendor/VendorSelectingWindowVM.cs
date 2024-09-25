using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorSelectingWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveCollection<CharInfoFactionReputationItemVM> FactionItems = new ReactiveCollection<CharInfoFactionReputationItemVM>();

	public VendorSelectingWindowVM([CanBeNull] List<MechanicEntity> vendors)
	{
		foreach (FactionType value in Enum.GetValues(typeof(FactionType)))
		{
			if (value != 0)
			{
				FactionItems.Add(new CharInfoFactionReputationItemVM(value, canTrade: true, vendors));
			}
		}
	}

	protected override void DisposeImplementation()
	{
		FactionItems.ForEach(delegate(CharInfoFactionReputationItemVM vm)
		{
			vm.Dispose();
		});
		FactionItems.Clear();
	}
}

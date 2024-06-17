using System;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoFactionsReputationVM : CharInfoComponentVM
{
	public readonly ReactiveCollection<IViewModel> ScreenItems = new ReactiveCollection<IViewModel>();

	public CharInfoFactionsReputationVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		foreach (FactionType value in Enum.GetValues(typeof(FactionType)))
		{
			if (value != 0)
			{
				ScreenItems.Add(new CharInfoFactionReputationItemVM(value));
			}
		}
		ScreenItems.Add(new ProfitFactorVM());
	}

	protected override void DisposeImplementation()
	{
		ScreenItems.ForEach(delegate(IViewModel vm)
		{
			vm.Dispose();
		});
		ScreenItems.Clear();
	}
}

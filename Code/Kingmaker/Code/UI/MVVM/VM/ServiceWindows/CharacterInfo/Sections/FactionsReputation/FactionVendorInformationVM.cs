using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class FactionVendorInformationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public string Location;

	public string Name;

	public MechanicEntity Vendor;

	public FactionVendorInformationVM(string location, string name, MechanicEntity vendor)
	{
		Location = location;
		Name = name;
		Vendor = vendor;
	}

	public FactionVendorInformationVM(string location, string name)
	{
		Location = location;
		Name = name;
	}

	public void StartTrade()
	{
		bool value = (Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode).Value;
		if (!value || UINetUtility.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.StartTrading(Vendor, value);
		}
	}

	protected override void DisposeImplementation()
	{
	}
}

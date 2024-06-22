using Kingmaker.Controllers;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorReputationLevelVM : VirtualListElementVMBase
{
	public int ReputationLevel;

	public bool Locked;

	public int ReputationPoints;

	public int NextLevelReputationPoints;

	public int CurrentReputationPoints;

	public int Delta;

	public int Difference;

	public ReactiveCommand OnHighlight = new ReactiveCommand();

	public VendorReputationLevelVM(int level, bool locked)
	{
		ReputationLevel = level;
		Locked = locked;
		if (!Locked)
		{
			Delta = 1;
			Difference = 1;
			return;
		}
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(Game.Instance.Vendor.VendorFaction.FactionType);
		if ((ReputationHelper.GetNextLvl(Game.Instance.Vendor.VendorFaction.FactionType) ?? level) == level)
		{
			ReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, currentReputationLevel);
			NextLevelReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, level);
		}
		else
		{
			ReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, level);
			NextLevelReputationPoints = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, level + 1);
		}
		CurrentReputationPoints = ReputationHelper.GetCurrentReputationPoints(Game.Instance.Vendor.VendorFaction.FactionType);
		Delta = NextLevelReputationPoints - ReputationPoints;
		Difference = CurrentReputationPoints - ReputationPoints;
	}

	protected override void DisposeImplementation()
	{
	}
}

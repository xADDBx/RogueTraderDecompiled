using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.UI.Models.LevelUp;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.MainMenu;

public class MainMenuChargenUnits : BaseDisposable
{
	public static MainMenuChargenUnits Instance;

	public readonly ReactiveProperty<ChargenUnit> CurrentPregenUnit = new ReactiveProperty<ChargenUnit>();

	public BlueprintDlcRewardCampaign DlcReward;

	public BlueprintPortrait CustomCharacterPortrait => BlueprintRoot.Instance.CharGenRoot.CustomPortrait;

	public MainMenuChargenUnits()
	{
		Instance = this;
		BlueprintRoot.Instance.CharGenRoot.EnsureNewGamePregens(null);
		BlueprintRoot.Instance.CharGenRoot.EnsureShipPregens(null);
	}

	protected override void DisposeImplementation()
	{
		Instance = null;
		BlueprintRoot.Instance.CharGenRoot.DisposeUnitsForChargen();
	}
}

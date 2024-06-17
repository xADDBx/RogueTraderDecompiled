using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UI.MVVM.VM.Colonization.Events;

public class RewardAvailableColonyEventVM : ColonyEventVM
{
	public RewardAvailableColonyEventVM()
		: base(new RewardAvailableDataProvider(), isColonyManagement: true)
	{
	}

	protected override void HandleColonyEventImpl()
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.ColonyProjectsRewards.ClaimDescription);
		});
	}
}

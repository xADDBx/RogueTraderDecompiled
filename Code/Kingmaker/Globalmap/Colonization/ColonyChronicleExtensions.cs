using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;

namespace Kingmaker.Globalmap.Colonization;

public static class ColonyChronicleExtensions
{
	public static void TryStartChronicle(Colony colony)
	{
		if (colony != null)
		{
			if (!colony.HasStartedChronicles)
			{
				TryShowColonyRewards(colony);
				return;
			}
			UISounds.Instance.Sounds.SpaceColonization.ColonyEvent.Play();
			colony.StartChronicle(colony.StartedChronicles[0]);
		}
	}

	private static void TryShowColonyRewards(Colony colony)
	{
		if (colony != null)
		{
			EventBus.RaiseEvent(delegate(IColonyRewardsUIHandler h)
			{
				h.HandleColonyRewardsShow(colony);
			});
		}
	}
}

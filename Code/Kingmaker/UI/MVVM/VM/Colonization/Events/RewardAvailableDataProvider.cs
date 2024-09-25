using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Models.Tooltip.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Events;

public class RewardAvailableDataProvider : IUIDataProvider
{
	public string Name => UIStrings.Instance.ColonyEventsTexts.RewardAvailableStatus.Text;

	public string Description => UIStrings.Instance.ColonyEventsTexts.NeedsVisitWarningMessage.Text;

	public Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Reward;

	public string NameForAcronym => null;
}

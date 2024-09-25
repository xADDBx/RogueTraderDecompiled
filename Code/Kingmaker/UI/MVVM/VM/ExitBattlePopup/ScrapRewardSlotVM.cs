using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ExitBattlePopup;

public class ScrapRewardSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<int> Amount = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ScrapRewardSlotVM()
	{
		Tooltip.Value = new TooltipTemplateGlossary("ScrapSpace", isHistory: true);
	}

	protected override void DisposeImplementation()
	{
	}

	public void IncreaseAmount(int additionalValue)
	{
		Amount.Value += additionalValue;
	}
}

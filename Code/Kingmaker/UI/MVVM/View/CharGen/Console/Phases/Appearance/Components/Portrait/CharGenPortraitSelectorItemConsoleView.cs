using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Appearance.Components.Portrait;

public class CharGenPortraitSelectorItemConsoleView : CharGenPortraitSelectorItemView, ILongConfirmClickHandler, IConsoleEntity
{
	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
			{
				h.HandleHoverStart(base.ViewModel.PortraitData);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
			{
				h.HandleHoverStop();
			});
		}
	}

	public bool CanLongConfirmClick()
	{
		return m_Button.IsValid();
	}

	public void OnLongConfirmClick()
	{
	}

	public string GetLongConfirmClickHint()
	{
		return UIStrings.Instance.CharGen.ChangePortrait;
	}
}

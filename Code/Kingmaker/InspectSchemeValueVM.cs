using System;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker;

public class InspectSchemeValueVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private ArmourAndShieldValueType ArmourAndShieldType;

	public string Value;

	public TooltipBaseTemplate Tooltip;

	public InspectSchemeValueVM(ArmourAndShieldValueType armourAndShieldType, string value, TooltipBaseTemplate tooltip)
	{
		ArmourAndShieldType = armourAndShieldType;
		Value = value;
		Tooltip = tooltip;
	}

	protected override void DisposeImplementation()
	{
	}
}

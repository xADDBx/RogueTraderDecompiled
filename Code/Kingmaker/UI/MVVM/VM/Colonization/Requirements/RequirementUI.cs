using Kingmaker.Code.UI.Models.Tooltip;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.Models.Tooltip.Base;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public class RequirementUI : IUIDataProvider, IUICountModifier
{
	protected Requirement Requirement;

	public virtual string Name => "Not implemented";

	public virtual string Description => "Not implemented";

	public virtual Sprite Icon => null;

	public virtual string NameForAcronym => "Not implemented";

	public virtual int Count => 0;

	public virtual string CountText => "Not implemented";

	public virtual Color? IconColor => null;

	public virtual TooltipBaseTemplate GetTooltip()
	{
		return null;
	}

	public RequirementUI(Requirement requirement)
	{
		Requirement = requirement;
	}
}
public class RequirementUI<T> : RequirementUI where T : Requirement
{
	protected new T Requirement => (T)base.Requirement;

	public RequirementUI(T requirement)
		: base(requirement)
	{
	}
}

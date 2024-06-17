using System.Collections.Generic;

namespace Owlcat.Runtime.UI.Tooltips;

public interface IHasTooltipTemplates
{
	List<TooltipBaseTemplate> TooltipTemplates();
}

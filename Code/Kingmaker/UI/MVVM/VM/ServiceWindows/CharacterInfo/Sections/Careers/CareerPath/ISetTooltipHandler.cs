using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;

public interface ISetTooltipHandler : ISubscriber
{
	void SetTooltip(TooltipBaseTemplate template);
}

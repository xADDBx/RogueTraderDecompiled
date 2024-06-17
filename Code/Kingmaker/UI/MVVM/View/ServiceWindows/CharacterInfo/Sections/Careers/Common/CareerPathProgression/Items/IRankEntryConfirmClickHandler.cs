using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public interface IRankEntryConfirmClickHandler : ISubscriber
{
	void OnRankEntryConfirmClick();
}

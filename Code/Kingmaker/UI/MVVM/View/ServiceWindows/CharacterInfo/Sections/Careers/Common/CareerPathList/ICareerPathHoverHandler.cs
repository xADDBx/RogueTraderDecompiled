using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public interface ICareerPathHoverHandler : ISubscriber
{
	void HandleHoverStart(BlueprintCareerPath careerPath);

	void HandleHoverStop();
}

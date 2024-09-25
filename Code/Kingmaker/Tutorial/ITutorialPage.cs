using Kingmaker.Localization;
using Kingmaker.ResourceLinks;

namespace Kingmaker.Tutorial;

public interface ITutorialPage
{
	SpriteLink Picture { get; }

	VideoLink Video { get; }

	LocalizedString TitleText { get; }

	LocalizedString DescriptionText { get; }

	LocalizedString TriggerText { get; }

	LocalizedString SolutionFoundText { get; }

	LocalizedString SolutionNotFoundText { get; }
}

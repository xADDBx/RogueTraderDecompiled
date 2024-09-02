using Kingmaker.Localization;

namespace Kingmaker.DialogSystem.Interfaces;

public interface ILocalizedStringHolder
{
	LocalizedString LocalizedStringText { get; }
}

namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface ILocalizationHandler : ISubscriber
{
	void HandleLanguageChanged();
}

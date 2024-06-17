namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookSubscribersList
{
	void AddSubscriber(object subscriber);

	void RemoveSubscriber(object subscriber);

	void OnEventAboutToTrigger(IRulebookEvent evt);

	void OnEventDidTrigger(IRulebookEvent evt);

	bool Empty();
}

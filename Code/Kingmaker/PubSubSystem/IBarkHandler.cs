using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IBarkHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleOnShowBark(string text);

	void HandleOnShowBarkWithName(string text, string name, Color nameColor);

	void HandleOnShowLinkedBark(string text, string encyclopediaLink);

	void HandleOnHideBark();
}

using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBookEventUIHandler : ISubscriber
{
	void HandleChooseCharacter(BlueprintAnswer answer);

	void HandleChooseCharacterEnd();
}

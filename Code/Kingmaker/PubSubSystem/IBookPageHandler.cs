using System.Collections.Generic;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBookPageHandler : ISubscriber
{
	void HandleOnBookPageShow(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers);
}

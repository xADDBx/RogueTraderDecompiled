using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INewGameChangeDlcHandler : ISubscriber
{
	void HandleNewGameChangeDlc(BlueprintCampaign campaign, BlueprintDlc blueprintDlc);
}

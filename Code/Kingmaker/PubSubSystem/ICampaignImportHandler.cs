using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICampaignImportHandler : ISubscriber
{
	void HandleSaveImport(BlueprintCampaign campaign, List<SaveInfo> saves);
}

using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.SystemMapNotification;

public class MiningNotificationVM : SystemMapNotificationBaseVM, IMiningUIHandler, ISubscriber
{
	public readonly ReactiveProperty<string> Message = new ReactiveProperty<string>(string.Empty);

	public MiningNotificationVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	void IMiningUIHandler.HandleStartMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource)
	{
		Message.Value = UIStrings.Instance.ExplorationTexts.StartMiningNotificationText.Text;
		ShowNotificationCommand.Execute();
	}

	void IMiningUIHandler.HandleStopMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource)
	{
		Message.Value = UIStrings.Instance.ExplorationTexts.StopMiningNotificationText.Text;
		ShowNotificationCommand.Execute();
	}
}

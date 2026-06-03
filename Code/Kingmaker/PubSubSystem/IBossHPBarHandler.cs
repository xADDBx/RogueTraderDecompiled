using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBossHPBarHandler : ISubscriber
{
	void ShowBossHPBar(ShowBossHPBarUIStruct bossHPBarUIStruct);

	void HideBossHPBar(string id);
}

using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;

namespace Kingmaker.PubSubSystem;

public interface ILevelUpManagerUIHandler : ISubscriber
{
	void HandleCreateLevelUpManager(LevelUpManager manager);

	void HandleDestroyLevelUpManager();

	void HandleUISelectCareerPath();

	void HandleUICommitChanges();

	void HandleUISelectionChanged();
}

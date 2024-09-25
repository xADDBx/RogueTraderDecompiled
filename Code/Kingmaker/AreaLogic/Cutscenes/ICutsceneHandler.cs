using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.AreaLogic.Cutscenes;

public interface ICutsceneHandler : ISubscriber<CutscenePlayerData>, ISubscriber
{
	void HandleCutsceneStarted(bool queued);

	void HandleCutsceneRestarted();

	void HandleCutscenePaused(CutscenePauseReason reason);

	void HandleCutsceneResumed();

	void HandleCutsceneStopped();
}

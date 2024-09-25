using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.AreaLogic.Cutscenes;

public interface ICutscenePlayerView : IEntityViewBase
{
	Cutscene Cutscene { get; set; }
}

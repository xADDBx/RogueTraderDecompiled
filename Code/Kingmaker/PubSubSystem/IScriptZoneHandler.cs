using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.SriptZones;

namespace Kingmaker.PubSubSystem;

public interface IScriptZoneHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void OnUnitEnteredScriptZone(ScriptZone zone);

	void OnUnitExitedScriptZone(ScriptZone zone);
}

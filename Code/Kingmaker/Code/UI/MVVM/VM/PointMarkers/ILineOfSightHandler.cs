using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.PointMarkers;

public interface ILineOfSightHandler : ISubscriber
{
	void OnLineOfSightCreated(LineOfSightVM los);

	void OnLineOfSightDestroyed(LineOfSightVM los);
}

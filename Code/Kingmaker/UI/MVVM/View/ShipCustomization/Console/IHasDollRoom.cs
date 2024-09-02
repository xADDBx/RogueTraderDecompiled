using Kingmaker.UI.DollRoom;
using Kingmaker.UI.Workarounds;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public interface IHasDollRoom
{
	DollRoomTargetController Controller { get; }

	void SetCanvasScaler(CanvasScalerWorkaround canvasScaler);
}

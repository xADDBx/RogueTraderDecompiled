using Kingmaker.Globalmap.Colonization;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public interface IColonyUIComponent
{
	void SetColony(Colony colony, bool isColonyManagement);
}

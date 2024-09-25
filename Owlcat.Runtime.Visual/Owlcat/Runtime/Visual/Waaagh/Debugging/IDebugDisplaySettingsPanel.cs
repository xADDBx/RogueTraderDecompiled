using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public interface IDebugDisplaySettingsPanel
{
	string PanelName { get; }

	DebugUI.Widget[] Widgets { get; }
}

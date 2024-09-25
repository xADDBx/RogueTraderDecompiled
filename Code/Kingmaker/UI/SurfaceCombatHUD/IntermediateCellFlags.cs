using System;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Flags]
public enum IntermediateCellFlags : byte
{
	ConnectionS = 1,
	ConnectionE = 2,
	ConnectionN = 4,
	ConnectionW = 8
}

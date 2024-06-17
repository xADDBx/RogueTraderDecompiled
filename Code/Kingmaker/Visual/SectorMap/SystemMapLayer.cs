using System;

namespace Kingmaker.Visual.SectorMap;

[Flags]
public enum SystemMapLayer
{
	Systems = 1,
	Routes = 2,
	Rumors = 4,
	All = 7
}

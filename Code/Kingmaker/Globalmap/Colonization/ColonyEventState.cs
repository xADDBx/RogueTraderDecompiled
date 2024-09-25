using System;

namespace Kingmaker.Globalmap.Colonization;

[Flags]
public enum ColonyEventState
{
	None = 0,
	Scheduled = 1,
	Started = 2,
	Finished = 4
}

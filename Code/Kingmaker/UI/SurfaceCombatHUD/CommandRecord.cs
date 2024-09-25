using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct CommandRecord
{
	public CommandCode code;

	public int dataIndex;
}

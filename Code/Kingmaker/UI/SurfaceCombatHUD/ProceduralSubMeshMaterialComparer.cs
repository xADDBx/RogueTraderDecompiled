using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[BurstCompile]
internal struct ProceduralSubMeshMaterialComparer : IComparer<ProcesuralSubMesh>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Compare(ProcesuralSubMesh x, ProcesuralSubMesh y)
	{
		return x.materialId.CompareTo(y.materialId);
	}
}

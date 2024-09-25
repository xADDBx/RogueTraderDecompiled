using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct MaterialAreaDescriptorBuffer
{
	public struct Enumerator
	{
		private NativeHashMap<byte, MaterialAreaDescriptor>.Enumerator m_Enumerator;

		public MaterialAreaDescriptor Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.Current.Value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator(NativeHashMap<byte, MaterialAreaDescriptor>.Enumerator enumerator)
		{
			m_Enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return m_Enumerator.MoveNext();
		}
	}

	private NativeHashMap<byte, MaterialAreaDescriptor> m_MaterialIdToMaterialAreaDescriptorMap;

	public MaterialAreaDescriptorBuffer(NativeHashMap<byte, MaterialAreaDescriptor> materialIdToMaterialAreaDescriptorMap)
	{
		m_MaterialIdToMaterialAreaDescriptorMap = materialIdToMaterialAreaDescriptorMap;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(MaterialAreaDescriptor descriptor)
	{
		if (m_MaterialIdToMaterialAreaDescriptorMap.TryGetValue(descriptor.materialId, out var item))
		{
			descriptor.coordsMin = math.min(descriptor.coordsMin, item.coordsMin);
			descriptor.coordsMax = math.max(descriptor.coordsMax, item.coordsMax);
		}
		m_MaterialIdToMaterialAreaDescriptorMap[descriptor.materialId] = descriptor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		m_MaterialIdToMaterialAreaDescriptorMap.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_MaterialIdToMaterialAreaDescriptorMap.GetEnumerator());
	}
}

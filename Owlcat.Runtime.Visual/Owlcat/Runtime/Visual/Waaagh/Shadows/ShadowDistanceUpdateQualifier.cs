using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal readonly struct ShadowDistanceUpdateQualifier
{
	private readonly ShadowUpdateMode m_Distance0UpdateMode;

	private readonly ShadowUpdateMode m_Distance1UpdateMode;

	private readonly ShadowUpdateMode m_Distance2UpdateMode;

	private readonly ShadowUpdateMode m_Distance3UpdateMode;

	private readonly float m_Cascade2Splits;

	private readonly float2 m_Cascade3Splits;

	private readonly float3 m_Cascade4Splits;

	private readonly int m_DistancesCount;

	private readonly float m_MaxShadowDistance;

	private readonly int m_CurrentFrameId;

	public ShadowDistanceUpdateQualifier(RenderingData renderingData, int currentFrameId)
	{
		m_Distance0UpdateMode = renderingData.ShadowData.ShadowUpdateDistances.Distance0UpdateMode;
		m_Distance1UpdateMode = renderingData.ShadowData.ShadowUpdateDistances.Distance1UpdateMode;
		m_Distance2UpdateMode = renderingData.ShadowData.ShadowUpdateDistances.Distance2UpdateMode;
		m_Distance3UpdateMode = renderingData.ShadowData.ShadowUpdateDistances.Distance3UpdateMode;
		m_Cascade2Splits = renderingData.ShadowData.ShadowUpdateDistances.Cascade2Splits;
		m_Cascade3Splits = renderingData.ShadowData.ShadowUpdateDistances.Cascade3Splits;
		m_Cascade4Splits = renderingData.ShadowData.ShadowUpdateDistances.Cascade4Splits;
		m_DistancesCount = renderingData.ShadowData.ShadowUpdateDistances.Count;
		m_MaxShadowDistance = renderingData.CameraData.MaxShadowDistance;
		m_CurrentFrameId = currentFrameId;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ShouldUpdate(float distanceToCamera, int lastRenderFrameId)
	{
		int shadowUpdateInterval = GetShadowUpdateInterval(distanceToCamera);
		return m_CurrentFrameId - lastRenderFrameId >= shadowUpdateInterval;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetShadowUpdateInterval(float fromCameraToLightDistance)
	{
		if (m_DistancesCount > 1)
		{
			float num = fromCameraToLightDistance / m_MaxShadowDistance;
			switch (m_DistancesCount)
			{
			case 2:
				if (num < m_Cascade2Splits)
				{
					return (int)m_Distance0UpdateMode;
				}
				return (int)m_Distance1UpdateMode;
			case 3:
				if (num < m_Cascade3Splits.x)
				{
					return (int)m_Distance0UpdateMode;
				}
				if (num < m_Cascade3Splits.y)
				{
					return (int)m_Distance1UpdateMode;
				}
				return (int)m_Distance2UpdateMode;
			case 4:
				if (num < m_Cascade4Splits.x)
				{
					return (int)m_Distance0UpdateMode;
				}
				if (num < m_Cascade4Splits.y)
				{
					return (int)m_Distance1UpdateMode;
				}
				if (num < m_Cascade4Splits.z)
				{
					return (int)m_Distance2UpdateMode;
				}
				return (int)m_Distance3UpdateMode;
			default:
				return (int)m_Distance0UpdateMode;
			}
		}
		return (int)m_Distance0UpdateMode;
	}
}

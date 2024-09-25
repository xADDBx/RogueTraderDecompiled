using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

public class FluidArea : MonoBehaviour
{
	private static FluidArea s_Active;

	[SerializeField]
	private Bounds m_Bounds = new Bounds(default(Vector3), new Vector3(30f, 30f, 30f));

	[SerializeField]
	public AmbientWindSettings m_AmbientWindSettings = AmbientWindSettings.DefaultSettings;

	[SerializeField]
	public FluidFogSettings m_FluidFogSettings = FluidFogSettings.DefaultSettings;

	[SerializeField]
	[Space]
	private bool m_SetActiveOnEnable;

	private FluidAreaGpuBuffer m_GpuBuffer;

	public static FluidArea Active
	{
		get
		{
			return s_Active;
		}
		set
		{
			s_Active = value;
		}
	}

	public Bounds Bounds
	{
		get
		{
			return m_Bounds;
		}
		set
		{
			m_Bounds = value;
		}
	}

	public AmbientWindSettings AmbientWindSettings => m_AmbientWindSettings;

	public FluidFogSettings FluidFogSettings => m_FluidFogSettings;

	public FluidAreaGpuBuffer GpuBuffer
	{
		get
		{
			if (m_GpuBuffer == null)
			{
				m_GpuBuffer = new FluidAreaGpuBuffer(this);
				m_GpuBuffer.Reset();
			}
			return m_GpuBuffer;
		}
	}

	private void OnEnable()
	{
		if (m_SetActiveOnEnable)
		{
			Active = this;
		}
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
		if (m_GpuBuffer != null)
		{
			m_GpuBuffer.Dispose();
			m_GpuBuffer = null;
		}
	}

	public Bounds GetWorldBounds()
	{
		Bounds bounds = m_Bounds;
		bounds.center += base.transform.position;
		return bounds;
	}

	public Matrix4x4 CalculateProjMatrix(bool convertToGpu = true)
	{
		Matrix4x4 matrix4x = Matrix4x4.Ortho(0f - m_Bounds.extents.x, m_Bounds.extents.x, 0f - m_Bounds.extents.z, m_Bounds.extents.z, 0.1f, m_Bounds.size.y);
		if (convertToGpu)
		{
			matrix4x = GL.GetGPUProjectionMatrix(matrix4x, renderIntoTexture: true);
		}
		return matrix4x;
	}

	public Matrix4x4 CalculateViewMatrix()
	{
		Bounds worldBounds = GetWorldBounds();
		Matrix4x4 cameraWorld = Matrix4x4.TRS(worldBounds.center + Vector3.up * worldBounds.extents.y, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
		return WorldToCameraMatrix(in cameraWorld);
	}

	public static Matrix4x4 WorldToCameraMatrix(in Matrix4x4 cameraWorld)
	{
		Matrix4x4 inverse = cameraWorld.inverse;
		inverse.m20 *= -1f;
		inverse.m21 *= -1f;
		inverse.m22 *= -1f;
		inverse.m23 *= -1f;
		return inverse;
	}

	internal Vector4 CalculateMaskST()
	{
		Bounds worldBounds = GetWorldBounds();
		return new Vector4(1f / worldBounds.size.x, 1f / worldBounds.size.z, (worldBounds.extents.x - worldBounds.center.x) / m_Bounds.size.x, (worldBounds.extents.z - worldBounds.center.z) / m_Bounds.size.z);
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.yellow;
		Bounds worldBounds = GetWorldBounds();
		Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
		Gizmos.color = color;
	}
}

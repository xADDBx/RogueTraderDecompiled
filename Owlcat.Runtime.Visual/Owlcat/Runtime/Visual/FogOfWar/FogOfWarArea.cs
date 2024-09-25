using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.FogOfWar;

public class FogOfWarArea : MonoBehaviour
{
	public delegate void ActivateEventHandler(FogOfWarArea area);

	private static FogOfWarArea s_Active;

	private static HashSet<FogOfWarArea> s_All = new HashSet<FogOfWarArea>();

	public static ActivateEventHandler AreaActivated;

	private RTHandle m_FogOfWarMapRT;

	[SerializeField]
	private Bounds m_Bounds = new Bounds(default(Vector3), new Vector3(30f, 30f, 30f));

	[SerializeField]
	private float m_ShadowFalloff = 0.15f;

	[SerializeField]
	private bool m_IsBlurEnabled = true;

	[SerializeField]
	private BorderSettings m_BorderSettings = new BorderSettings();

	[SerializeField]
	private Texture2D m_StaticMask;

	[SerializeField]
	private bool m_RevealOnStart;

	[SerializeField]
	private bool m_SetActiveOnEnable;

	[SerializeField]
	private bool m_ApplyShaderManually;

	public static FogOfWarArea Active
	{
		get
		{
			return s_Active;
		}
		set
		{
			if (!value && (object)value != null)
			{
				Debug.LogError("FogOfWarArea.set_Active: new value is destroyed object");
				value = null;
			}
			s_Active = value;
			if ((bool)s_Active)
			{
				NotifyAreaActivated(s_Active);
			}
		}
	}

	public static HashSet<FogOfWarArea> All => s_All;

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

	public float ShadowFalloff
	{
		get
		{
			return m_ShadowFalloff;
		}
		set
		{
			m_ShadowFalloff = value;
		}
	}

	public BorderSettings BorderSettings
	{
		get
		{
			return m_BorderSettings;
		}
		set
		{
			m_BorderSettings = value;
		}
	}

	public bool RevealOnStart
	{
		get
		{
			return m_RevealOnStart;
		}
		set
		{
			m_RevealOnStart = value;
		}
	}

	public bool ApplyShaderManually
	{
		get
		{
			return m_ApplyShaderManually;
		}
		set
		{
			m_ApplyShaderManually = value;
		}
	}

	public RTHandle FogOfWarMapRT
	{
		get
		{
			if (m_FogOfWarMapRT == null)
			{
				FogOfWarSettings instance = FogOfWarSettings.Instance;
				if (instance != null)
				{
					Vector3 size = m_Bounds.size;
					int width = Mathf.Min((int)(instance.TextureDensity * size.x), 2048);
					int height = Mathf.Min((int)(instance.TextureDensity * size.z), 2048);
					m_FogOfWarMapRT = RTHandles.Alloc(width, height, 1, DepthBits.None, GraphicsFormat.R8G8B8A8_UNorm, FilterMode.Bilinear, TextureWrapMode.Clamp, TextureDimension.Tex2D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, RenderTextureMemoryless.None, VRTextureUsage.None, "FogOfWarMapRT_" + base.name);
					RenderTexture active = RenderTexture.active;
					RenderTexture.active = m_FogOfWarMapRT;
					GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
					RenderTexture.active = active;
				}
			}
			return m_FogOfWarMapRT;
		}
	}

	public bool IsBlurEnabled
	{
		get
		{
			return m_IsBlurEnabled;
		}
		set
		{
			m_IsBlurEnabled = value;
		}
	}

	public Texture2D StaticMask
	{
		get
		{
			return m_StaticMask;
		}
		set
		{
			m_StaticMask = value;
		}
	}

	public Task<byte[]> RequestData()
	{
		GraphicsFormat dstFormat = GraphicsFormat.R8G8B8_SRGB;
		_ = ((RenderTexture)FogOfWarMapRT).width;
		_ = ((RenderTexture)FogOfWarMapRT).height;
		TaskCompletionSource<byte[]> task = new TaskCompletionSource<byte[]>();
		AsyncGPUReadback.Request(FogOfWarMapRT, 0, dstFormat, delegate(AsyncGPUReadbackRequest r)
		{
			if (r.hasError)
			{
				task.SetException(new Exception("AsyncGPURequest failed to read data"));
			}
			else
			{
				NativeArray<byte> data = r.GetData<byte>();
				byte[] result = data.ToArray();
				data.Dispose();
				task.SetResult(result);
			}
		});
		return task.Task;
	}

	private static void NotifyAreaActivated(FogOfWarArea area)
	{
		AreaActivated?.Invoke(area);
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

	private void OnDestroy()
	{
		if (m_FogOfWarMapRT != null)
		{
			RTHandles.Release(m_FogOfWarMapRT);
			m_FogOfWarMapRT = null;
		}
	}

	private void OnEnable()
	{
		if (m_SetActiveOnEnable)
		{
			Active = this;
		}
		s_All.Add(this);
	}

	private void OnDisable()
	{
		s_All.Remove(this);
	}

	public Bounds GetWorldBounds()
	{
		Bounds bounds = m_Bounds;
		bounds.center += base.transform.position;
		return bounds;
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.magenta;
		Bounds worldBounds = GetWorldBounds();
		Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
		Gizmos.color = color;
	}

	internal Vector4 CalculateMaskST()
	{
		Bounds worldBounds = GetWorldBounds();
		return new Vector4(1f / worldBounds.size.x, 1f / worldBounds.size.z, (worldBounds.extents.x - worldBounds.center.x) / m_Bounds.size.x, (worldBounds.extents.z - worldBounds.center.z) / m_Bounds.size.z);
	}

	public void RestoreFogOfWarMask(Texture2D mask)
	{
		RTHandle fogOfWarMapRT = FogOfWarMapRT;
		RenderTexture.active = fogOfWarMapRT;
		Graphics.Blit(mask, fogOfWarMapRT);
	}

	public void RestoreFogOfWarMask(byte[] colorsData)
	{
		RenderTexture renderTexture = FogOfWarMapRT;
		Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, mipChain: false);
		try
		{
			texture2D.SetPixelData(colorsData, 0);
			texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			RestoreFogOfWarMask(texture2D);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		UnityEngine.Object.DestroyImmediate(texture2D);
	}
}

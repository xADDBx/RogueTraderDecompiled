using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct CameraData
{
	private Matrix4x4 m_ViewMatrix;

	private Matrix4x4 m_ProjectionMatrix;

	private Matrix4x4 m_JitterMatrix;

	public Camera Camera;

	public CameraRenderType RenderType;

	public RenderTexture TargetTexture;

	public RenderTexture TargetDepthTexture;

	public WaaaghCameraBuffer CameraBuffer;

	public CameraType CameraType;

	public bool IsLightingEnabled;

	public bool IsShadowsEnabled;

	public RenderTextureDescriptor CameraTargetDescriptor;

	public bool IsHdrEnabled;

	public HDRColorBufferPrecision HDRColorBufferPrecision;

	public float MaxShadowDistance;

	public bool ClearDepth;

	public bool PostProcessEnabled;

	public bool RequiresDepthTexture;

	public bool RequiresOpaqueTexture;

	public SortingCriteria DefaultOpaqueSortFlags;

	public bool IsSSREnabled;

	public bool IsSSREnabledInStack;

	public bool IsStochasticSSR;

	public bool IsFogEnabled;

	public bool IsNeedDepthPyramid;

	public bool IsIndirectRenderingEnabled;

	public bool IsSceneViewInPrefabEditMode;

	public bool IsStopNaNEnabled;

	public bool IsDitheringEnabled;

	public AntialiasingMode Antialiasing;

	public AntialiasingQuality AntialiasingQuality;

	public float TemporalAntialiasingSharpness;

	internal float FinalTargetAspectRatio;

	internal Rect FinalTargetViewport;

	internal CameraRenderTargetType CameraRenderTargetBufferType;

	internal CameraResolveTargetType CameraResolveTargetBufferType;

	internal bool CameraResolveRequired;

	internal float RenderScale;

	internal ImageScalingMode ScalingMode;

	internal ImageUpscalingFilter UpscalingFilter;

	internal float FsrSharpness;

	public ScriptableRenderer Renderer;

	public Vector3 WorldSpaceCameraPos;

	internal Vector2Int NonScaledCameraTargetViewportSize => new Vector2Int((int)FinalTargetViewport.width, (int)FinalTargetViewport.height);

	internal Vector2Int ScaledCameraTargetViewportSize
	{
		get
		{
			if (ScalingMode != 0)
			{
				return new Vector2Int((int)(FinalTargetViewport.width * RenderScale), (int)(FinalTargetViewport.height * RenderScale));
			}
			return new Vector2Int((int)FinalTargetViewport.width, (int)FinalTargetViewport.height);
		}
	}

	public bool IsSceneViewCamera => CameraType == CameraType.SceneView;

	public bool IsPreviewCamera => CameraType == CameraType.Preview;

	internal void SetViewProjectionAndJitterMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 jitterMatrix)
	{
		m_ViewMatrix = viewMatrix;
		m_ProjectionMatrix = projectionMatrix;
		m_JitterMatrix = jitterMatrix;
	}

	public Matrix4x4 GetViewMatrix(int viewIndex = 0)
	{
		return m_ViewMatrix;
	}

	public Matrix4x4 GetProjectionMatrix(int viewIndex = 0)
	{
		return m_JitterMatrix * m_ProjectionMatrix;
	}

	public Matrix4x4 GetProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return m_ProjectionMatrix;
	}

	public Matrix4x4 GetGPUProjectionMatrix(int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrix(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	public Matrix4x4 GetGPUProjectionMatrixNoJitter(int viewIndex = 0)
	{
		return GL.GetGPUProjectionMatrix(GetProjectionMatrixNoJitter(viewIndex), IsCameraProjectionMatrixFlipped());
	}

	public bool IsCameraProjectionMatrixFlipped()
	{
		if (ScriptableRenderer.Current != null)
		{
			bool flag = true;
			return SystemInfo.graphicsUVStartsAtTop && flag;
		}
		return true;
	}
}

using System;
using Kingmaker.Utility.CommandLineArgs;
using UnityEngine;

namespace Kingmaker.Visual;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RenderingManager : MonoBehaviour
{
	[Serializable]
	public struct RimLightingSettings
	{
		public Color RimGlobalColor;

		[Range(0f, 10f)]
		public float RimGlobalIntensity;

		public static RimLightingSettings DefaultSettings
		{
			get
			{
				RimLightingSettings result = default(RimLightingSettings);
				result.RimGlobalColor = Color.white;
				result.RimGlobalIntensity = 1f;
				return result;
			}
		}
	}

	public GameObject BackgroundCamera;

	[HideInInspector]
	public GameObject Background;

	private static bool m_Initialized;

	private Camera m_MainCamera;

	public static Action SettingsApplied;

	public RimLightingSettings RimLighting = RimLightingSettings.DefaultSettings;

	public bool HDR = true;

	public static RenderingManager Instance { get; private set; }

	public Camera MainCamera
	{
		get
		{
			if (m_MainCamera == null)
			{
				m_MainCamera = GetComponent<Camera>();
			}
			return m_MainCamera;
		}
		set
		{
			m_MainCamera = value;
		}
	}

	private void Start()
	{
		Instance = this;
		if ((bool)BackgroundCamera)
		{
			BackgroundCamera.SetActive(value: false);
			Game.GetCamera().clearFlags = CameraClearFlags.Color;
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnEnable()
	{
		MainCamera.depthTextureMode = DepthTextureMode.Depth;
		ApplySettings();
	}

	private void OnPreRender()
	{
		MainCamera.depthTextureMode = DepthTextureMode.Depth;
		UpdateRimLighting();
		UpdateShaderConstants();
	}

	private void UpdateShaderConstants()
	{
		Matrix4x4 matrix4x = Matrix4x4.Inverse(m_MainCamera.projectionMatrix);
		Shader.SetGlobalFloat("_TanHalfHorizontalFov", matrix4x[0, 0]);
		Shader.SetGlobalFloat("_TanHalfVerticalFov", matrix4x[1, 1]);
		Shader.SetGlobalVector("_CamBasisUp", m_MainCamera.transform.up);
		Shader.SetGlobalVector("_CamBasisSide", m_MainCamera.transform.right);
		Shader.SetGlobalVector("_CamBasisFront", m_MainCamera.transform.forward);
	}

	private void UpdateRimLighting()
	{
		Shader.SetGlobalColor("_RimGlobalColor", RimLighting.RimGlobalColor * RimLighting.RimGlobalIntensity);
	}

	public void ApplySettings()
	{
		if (!CommandLineArguments.Parse().Contains("default-graphics"))
		{
			MainCamera.allowHDR = HDR;
			SettingsApplied?.Invoke();
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void InitOnLoad()
	{
		Init();
	}

	private static void Init()
	{
		Shader.EnableKeyword("GLOBAL_REFLECTIONS_ON");
		Shader.EnableKeyword("GLOBAL_SHADOWS_ON");
		if (!m_Initialized)
		{
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(StaticPreRender));
			m_Initialized = true;
		}
	}

	private static void StaticPreRender(Camera cam)
	{
		if (Instance != null && Instance.MainCamera != null)
		{
			Instance.MainCamera.nearClipPlane = 1f;
		}
		SetPositionReconstructionData(cam);
		SetScreenSpaceReflectionsData(cam);
	}

	private static void SetScreenSpaceReflectionsData(Camera cam)
	{
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
		float num = cam.pixelWidth;
		float num2 = cam.pixelHeight;
		Vector4 value = new Vector4(-2f / (num * projectionMatrix[0]), -2f / (num2 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]);
		float num3 = num / 2f;
		float num4 = num2 / 2f;
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.SetRow(0, new Vector4(num3, 0f, 0f, num3));
		matrix4x.SetRow(1, new Vector4(0f, num4, 0f, num4));
		matrix4x.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
		matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		Matrix4x4 value2 = matrix4x * projectionMatrix;
		Vector3 vector = (float.IsPositiveInfinity(cam.farClipPlane) ? new Vector3(cam.nearClipPlane, -1f, 1f) : new Vector3(cam.nearClipPlane * cam.farClipPlane, cam.nearClipPlane - cam.farClipPlane, cam.farClipPlane));
		Shader.SetGlobalMatrix("_WorldToCameraMatrix", cam.worldToCameraMatrix);
		Shader.SetGlobalVector("_ProjInfo", value);
		Shader.SetGlobalMatrix("_ProjectToPixelMatrix", value2);
		Shader.SetGlobalVector("_ScreenSize", new Vector2(num, num2));
		Shader.SetGlobalVector("_InvScreenSize", new Vector2(1f / num, 1f / num2));
		Shader.SetGlobalVector("_CameraClipInfo", vector);
	}

	private static void SetPositionReconstructionData(Camera cam)
	{
		Matrix4x4 matrix4x = Matrix4x4.Inverse(cam.projectionMatrix);
		Shader.SetGlobalFloat("_TanHalfHorizontalFov", matrix4x[0, 0]);
		Shader.SetGlobalFloat("_TanHalfVerticalFov", matrix4x[1, 1]);
		Shader.SetGlobalVector("_CamBasisUp", cam.transform.up);
		Shader.SetGlobalVector("_CamBasisSide", cam.transform.right);
		Shader.SetGlobalVector("_CamBasisFront", cam.transform.forward);
	}

	private void LateUpdate()
	{
		if ((bool)BackgroundCamera && (bool)m_MainCamera)
		{
			if (!Background)
			{
				BackgroundCamera.SetActive(value: false);
				m_MainCamera.clearFlags = CameraClearFlags.Color;
			}
			else
			{
				BackgroundCamera.SetActive(value: true);
				m_MainCamera.clearFlags = CameraClearFlags.Depth;
			}
		}
	}
}

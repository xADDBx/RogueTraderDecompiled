using Kingmaker.View;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Code.Globalmap;

public class WarpMoveEffect : MonoBehaviour
{
	[Header("WarpEffect")]
	[Tooltip("This value will update when you start new travel")]
	public Color WarpEffectColorAndTransparent = new Color(255f, 0f, 149f, 80f);

	public Vector2 WarpEffectSpeed = new Vector2(-0.1f, -0.01f);

	[Range(1f, 10f)]
	[Tooltip("This value will update when you finish travel")]
	public float WarpEffectFadeOutSpeed = 3f;

	[Header("WarpPortal")]
	[Tooltip("Warp  portal prefab")]
	public GameObject WarpPortalIn;

	public GameObject WarpPortalOut;

	[Header("Camera shake")]
	[Range(0f, 0.05f)]
	[Tooltip("This value will update when you start new travel")]
	public float ShakeAmplitude = 0.02f;

	[Tooltip("This value will update when you start new travel")]
	[Range(0f, 0.05f)]
	public float ShakeSpeed = 0.005f;

	[Header("Effect alarm light")]
	[Tooltip("Light prefab")]
	public GameObject LightPrefab1;

	[Range(0f, 20f)]
	[Tooltip("Light rotate speed update in runtime")]
	public float RotateSpeed1 = 0.5f;

	private float m_RotateSpeed1;

	[Space(10f)]
	[Tooltip("Light prefab")]
	public GameObject LightPrefab2;

	[Range(0f, 20f)]
	[Tooltip("Light rotate speed update in runtime")]
	public float RotateSpeed2 = 1f;

	private float m_RotateSpeed2;

	[Range(0f, 20f)]
	[Tooltip("Divide light rotate speed in RandomEncauters and dialogs")]
	public float RotateSpeedDialogDivider = 2f;

	private Material m_EffectMaterial;

	private float m_FadeValue = 1f;

	private ParticleSystem.MainModule m_EffectParticleMain;

	private GameObject m_Light1;

	private GameObject m_Light2;

	private GameObject m_InstancedWarpPortalIn;

	private bool m_EffectIsStartDestroy;

	private bool m_LightsIsInstanced;

	private static readonly int WarpFadeShaderProperty = Shader.PropertyToID("_Fade");

	private static readonly int WarpSpeedShaderProperty = Shader.PropertyToID("_ScrollBaseMap");

	private void OnDestroy()
	{
		Object.Destroy(m_Light1);
		Object.Destroy(m_Light2);
		CameraRig.Instance.StopShake();
	}

	public void Start()
	{
		Renderer component = base.gameObject.GetComponent<Renderer>();
		m_EffectMaterial = component.material;
		ParticleSystem component2 = base.gameObject.GetComponent<ParticleSystem>();
		m_EffectParticleMain = component2.main;
		m_EffectParticleMain.startColor = WarpEffectColorAndTransparent;
		m_EffectMaterial.SetVector(WarpSpeedShaderProperty, WarpEffectSpeed);
	}

	private void Update()
	{
		RotateLight();
		DestroyEffect();
	}

	public void WarpTravelBeforeStart()
	{
		InstanceWarpPortal(enterToWarp: true);
		Debug.Log("WarpTravelBeforeStart");
	}

	public void WarpTravelStarted()
	{
		InstanceLights();
		CameraRig.Instance.StartShake(ShakeAmplitude, ShakeSpeed);
		Debug.Log("WarpTravelStarted");
	}

	public void WarpTravelStopped()
	{
		InstanceWarpPortal(enterToWarp: false);
		CameraRig.Instance.StopShake();
		m_EffectIsStartDestroy = true;
		Debug.Log("WarpTravelStopped");
	}

	public void WarpTravelPaused()
	{
		m_RotateSpeed1 = RotateSpeed1;
		m_RotateSpeed2 = RotateSpeed2;
		RotateSpeed1 /= RotateSpeedDialogDivider;
		RotateSpeed2 /= RotateSpeedDialogDivider;
	}

	public void WarpTravelResumed()
	{
		RotateSpeed1 = m_RotateSpeed1;
		RotateSpeed2 = m_RotateSpeed2;
	}

	private void InstanceWarpPortal(bool enterToWarp)
	{
		GameObject gameObject = Game.Instance.SectorMapController.VisualParameters.PlayerShip.gameObject;
		if (!gameObject)
		{
			PFLog.TechArt.Error("Cant find playerShip GameObject");
		}
		if (enterToWarp)
		{
			m_InstancedWarpPortalIn = FxHelper.SpawnFxOnGameObject(WarpPortalIn, gameObject);
			return;
		}
		if ((bool)m_InstancedWarpPortalIn)
		{
			FxHelper.Destroy(m_InstancedWarpPortalIn);
		}
		FxHelper.SpawnFxOnGameObject(WarpPortalOut, gameObject);
	}

	private void InstanceLights()
	{
		m_Light1 = Object.Instantiate(LightPrefab1, CameraRig.Instance.CameraAttachPoint.transform);
		m_Light2 = Object.Instantiate(LightPrefab2, CameraRig.Instance.CameraAttachPoint.transform);
		m_LightsIsInstanced = true;
	}

	private void RotateLight()
	{
		if (m_LightsIsInstanced)
		{
			m_Light1.transform.Rotate(0f, RotateSpeed1, 0f, Space.World);
			m_Light2.transform.Rotate(0f, RotateSpeed2, 0f, Space.World);
		}
	}

	private void DestroyEffect()
	{
		if (m_EffectIsStartDestroy)
		{
			m_EffectMaterial.SetFloat(WarpFadeShaderProperty, m_FadeValue);
			m_FadeValue -= WarpEffectFadeOutSpeed * Time.deltaTime;
			if (m_FadeValue <= 0f)
			{
				Object.Destroy(m_Light1);
				Object.Destroy(m_Light2);
				Object.Destroy(base.gameObject);
			}
		}
	}
}

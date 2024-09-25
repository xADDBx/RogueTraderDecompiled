using System.Collections;
using Cinemachine;
using Kingmaker.Controllers.Clicks;
using Kingmaker.GameModes;
using Kingmaker.UI.Selection;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.View;

public class CameraZoom : MonoBehaviour
{
	[SerializeField]
	private Camera m_Camera;

	public bool ZoomLock;

	public bool RecordLock;

	private float m_PlayerScrollPosition;

	private float m_GamepadScrollPosition;

	private float m_GameScrollPosition;

	private float m_ScrollPosition;

	private float m_SmoothScrollPosition;

	private Coroutine m_ZoomRoutine;

	private CinemachineVirtualCamera m_VirtualCamera;

	private float m_MainCameraCurrentFow;

	private float m_MainCameraCurrentOffsetZ;

	public float ZoomLength { get; set; }

	public float Smoothness { get; set; }

	public float CurrentNormalizePosition
	{
		get
		{
			return m_SmoothScrollPosition / ZoomLength;
		}
		set
		{
			m_ScrollPosition = value * ZoomLength;
		}
	}

	public float CutSceneNormalizePosition
	{
		get
		{
			return m_GameScrollPosition / ZoomLength;
		}
		set
		{
			m_GameScrollPosition = value * ZoomLength;
		}
	}

	public float FovMin { get; set; }

	public float FovMax { get; set; }

	public float FovDefault { get; set; }

	public bool EnablePhysicalZoom { get; set; }

	public float PhysicalZoomMin { get; set; }

	public float PhysicalZoomMax { get; set; }

	public float GamepadScrollPosition
	{
		set
		{
			m_GamepadScrollPosition = value;
		}
	}

	public float PlayerScrollPosition
	{
		set
		{
			m_PlayerScrollPosition = value;
		}
	}

	public bool IsScrollBusy => KingmakerInputModule.ScrollIsBusy;

	public bool IsOutOfScreen => false;

	public float FovDefaultNormalized => Mathf.InverseLerp(FovMax, FovMin, FovDefault);

	public void TickZoom()
	{
		if (m_ZoomRoutine != null || ZoomLock || RecordLock)
		{
			return;
		}
		if (Game.Instance.CurrentMode == GameModeType.SpaceCombat && !m_VirtualCamera)
		{
			GameObject gameObject = GameObject.Find("VcMain");
			m_VirtualCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
		}
		if (!IsScrollBusy && Game.Instance.IsControllerMouse && !IsOutOfScreen && !PointerController.InGui)
		{
			m_PlayerScrollPosition += Input.GetAxis("Mouse ScrollWheel");
		}
		m_ScrollPosition = m_PlayerScrollPosition + m_GamepadScrollPosition;
		m_GamepadScrollPosition = 0f;
		m_ScrollPosition = Mathf.Clamp(m_ScrollPosition, 0f, ZoomLength);
		m_SmoothScrollPosition = Mathf.Lerp(m_SmoothScrollPosition, m_ScrollPosition, Time.unscaledDeltaTime * Smoothness);
		m_MainCameraCurrentFow = Mathf.Lerp(FovMax, FovMin, CurrentNormalizePosition);
		if (Game.Instance.CurrentMode == GameModeType.SpaceCombat && (bool)m_VirtualCamera)
		{
			m_VirtualCamera.m_Lens.FieldOfView = m_MainCameraCurrentFow;
			if (EnablePhysicalZoom)
			{
				m_MainCameraCurrentOffsetZ = Mathf.Lerp(PhysicalZoomMin, PhysicalZoomMax, CurrentNormalizePosition);
				m_VirtualCamera.transform.localPosition = new Vector3(m_VirtualCamera.transform.localPosition.x, m_VirtualCamera.transform.localPosition.y, m_MainCameraCurrentOffsetZ);
			}
		}
		else
		{
			m_Camera.fieldOfView = m_MainCameraCurrentFow;
			if (EnablePhysicalZoom)
			{
				m_MainCameraCurrentOffsetZ = Mathf.Lerp(PhysicalZoomMin, PhysicalZoomMax, CurrentNormalizePosition);
				m_Camera.transform.localPosition = new Vector3(m_Camera.transform.localPosition.x, m_Camera.transform.localPosition.y, m_MainCameraCurrentOffsetZ);
			}
		}
		m_PlayerScrollPosition = m_ScrollPosition;
	}

	public void ZoomToImmediate(float position)
	{
		if (m_ZoomRoutine != null)
		{
			StopCoroutine(m_ZoomRoutine);
			m_ZoomRoutine = null;
		}
		m_PlayerScrollPosition = (m_ScrollPosition = (m_SmoothScrollPosition = position));
		m_Camera.fieldOfView = Mathf.Lerp(FovMax, FovMin, CurrentNormalizePosition);
	}

	public Coroutine ZoomToTimed(float toValue, float maxTime = 0f, float speed = 0f, AnimationCurve curve = null)
	{
		float targetTime;
		return ZoomToTimed(toValue, out targetTime, maxTime, speed, curve);
	}

	public Coroutine ZoomToTimed(float toValue, out float targetTime, float maxTime = 0f, float speed = 0f, AnimationCurve curve = null)
	{
		if (m_ZoomRoutine != null)
		{
			StopCoroutine(m_ZoomRoutine);
			m_ZoomRoutine = null;
		}
		float currentNormalizePosition = CurrentNormalizePosition;
		float num = Mathf.Abs(toValue - currentNormalizePosition);
		float num2 = ((0f < maxTime) ? (num / maxTime) : speed);
		float num3 = ((0f < num2) ? (num / num2) : 0f);
		if (curve == null)
		{
			curve = AnimationCurveUtility.LinearAnimationCurve;
		}
		targetTime = num3;
		if (num <= 0f)
		{
			return null;
		}
		m_ZoomRoutine = StartCoroutine(ZoomToRoutine(currentNormalizePosition, toValue, num3, curve));
		return m_ZoomRoutine;
	}

	private IEnumerator ZoomToRoutine(float fromValue, float toValue, float time, AnimationCurve curve)
	{
		float start = Time.time;
		float end = start + time;
		while (Time.time < end)
		{
			yield return null;
			float t = curve.Evaluate((Time.time - start) / time);
			m_ScrollPosition = (m_SmoothScrollPosition = Mathf.LerpUnclamped(fromValue, toValue, t));
			m_Camera.fieldOfView = Mathf.Lerp(FovMax, FovMin, CurrentNormalizePosition);
		}
		m_PlayerScrollPosition = (m_ScrollPosition = (m_SmoothScrollPosition = toValue));
		m_Camera.fieldOfView = Mathf.Lerp(FovMax, FovMin, CurrentNormalizePosition);
		yield return null;
		m_ZoomRoutine = null;
	}
}

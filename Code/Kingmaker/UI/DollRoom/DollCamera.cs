using System.Collections;
using Kingmaker.Settings.Graphics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.DollRoom;

[RequireComponent(typeof(Camera))]
public class DollCamera : MonoBehaviour
{
	[SerializeField]
	protected float m_Elasticity = 0.1f;

	[SerializeField]
	protected float m_SensitivityZoom;

	[SerializeField]
	protected float m_AutoZoomSpeed = 0.2f;

	[SerializeField]
	protected float m_SmoothZoom = 1f;

	[SerializeField]
	protected float m_MinZoom;

	[SerializeField]
	protected float m_MaxZoom;

	private float m_CurrentZoom;

	private float m_CurrentSmoothZoom;

	private Vector3 m_StartPosition;

	private Quaternion m_StartRotation;

	[SerializeField]
	private DollRoomCameraZoomPreset m_DefaultZoomPreset;

	[SerializeField]
	private DollRoomCameraZoomPreset m_CharacterZoomPreset;

	[SerializeField]
	private Transform m_DefaultTargetTransform;

	private Transform m_CharacterTargetTransform;

	private Camera m_Camera;

	private bool m_CameraEnabled;

	protected float m_DeltaZoom;

	protected float m_Velocity;

	protected bool m_Zoom;

	protected bool m_IsInit;

	public static DollCamera Current { get; protected set; }

	private DollRoomCameraZoomPreset ZoomPreset
	{
		get
		{
			if (m_CharacterZoomPreset == null)
			{
				return m_DefaultZoomPreset;
			}
			return m_CharacterZoomPreset;
		}
	}

	private Transform TargetTransform
	{
		get
		{
			if (!(m_CharacterTargetTransform != null))
			{
				return m_DefaultTargetTransform;
			}
			return m_CharacterTargetTransform;
		}
	}

	public float ZoomLenght => Mathf.Abs(m_MinZoom - m_MaxZoom);

	public float ZoomNormalized
	{
		get
		{
			return (m_CurrentZoom - m_MinZoom) / ZoomLenght;
		}
		set
		{
			m_CurrentZoom = Mathf.Lerp(m_MinZoom, m_MaxZoom, Mathf.Clamp01(value));
		}
	}

	private bool CanZoom
	{
		get
		{
			if (m_CharacterZoomPreset != null && m_CharacterTargetTransform != null)
			{
				return m_CharacterZoomPreset.CanZoom;
			}
			return false;
		}
	}

	protected void Start()
	{
		EnsureCamera();
		UpdateCameraState();
		m_StartPosition = base.transform.position;
		m_StartRotation = base.transform.rotation;
		m_CurrentSmoothZoom = m_CurrentZoom;
		m_IsInit = true;
	}

	private void UpdateCameraState()
	{
		if (m_Camera.enabled != m_CameraEnabled)
		{
			m_Camera.enabled = m_CameraEnabled;
		}
	}

	protected void OnEnable()
	{
		SetAsCurrent();
		this.EnsureComponent<CameraAntialiasingHelper>();
	}

	protected void Update()
	{
		DirtyZoom();
	}

	protected void DirtyZoom()
	{
		if (!CanZoom)
		{
			m_CurrentZoom = 0f;
		}
		else
		{
			m_CurrentZoom += m_DeltaZoom;
		}
		if (m_Zoom)
		{
			m_CurrentZoom = Mathf.Clamp(m_CurrentZoom, m_MinZoom - m_Elasticity, m_MaxZoom + m_Elasticity * 3f);
		}
		else
		{
			if (m_CurrentZoom < m_MinZoom)
			{
				m_CurrentZoom = Mathf.Lerp(m_CurrentZoom, m_MinZoom, 20f * Time.unscaledDeltaTime);
			}
			if (m_CurrentZoom > m_MaxZoom)
			{
				m_CurrentZoom = Mathf.Lerp(m_CurrentZoom, m_MaxZoom, 20f * Time.unscaledDeltaTime);
			}
		}
		if (m_SmoothZoom != 0f && base.isActiveAndEnabled)
		{
			m_CurrentSmoothZoom = Mathf.Lerp(m_CurrentSmoothZoom, m_CurrentZoom, Time.unscaledDeltaTime * m_SmoothZoom);
		}
		else
		{
			m_CurrentSmoothZoom = m_CurrentZoom;
		}
		Vector3 axis = GetAxis();
		axis = new Vector3(0f, axis.y, axis.z);
		base.transform.position = m_StartPosition + axis * m_CurrentSmoothZoom;
		if (m_CurrentZoom <= m_MinZoom || m_CurrentZoom >= m_MaxZoom)
		{
			m_Zoom = false;
			m_DeltaZoom = 0f;
		}
	}

	protected Vector3 GetAxis()
	{
		return Vector3.Normalize(m_StartPosition - TargetTransform.position - ZoomPreset.OffsetFromHead);
	}

	public void SetAsCurrent()
	{
		Current = this;
	}

	public void LookAt(Transform targetTransform = null, DollRoomCameraZoomPreset zoomPreset = null)
	{
		m_CharacterTargetTransform = targetTransform;
		m_CharacterZoomPreset = zoomPreset;
	}

	public void BeginZoom()
	{
		m_Zoom = true;
	}

	public void EndZoom()
	{
		m_Zoom = false;
		m_DeltaZoom = 0f;
	}

	public void Zoom(float delta)
	{
		m_DeltaZoom = (0f - delta) * m_SensitivityZoom;
		StartSmoothZoom();
	}

	private void StartSmoothZoom()
	{
		if (!m_Zoom)
		{
			StartCoroutine(OnEndZoom());
		}
		m_Zoom = true;
	}

	public void ZoomMin()
	{
		m_DeltaZoom = 0f - m_AutoZoomSpeed;
		m_Zoom = true;
	}

	public void ZoomMax()
	{
		m_DeltaZoom = m_AutoZoomSpeed;
		m_Zoom = true;
	}

	public void ResetZoom()
	{
		m_CharacterTargetTransform = null;
		m_CharacterZoomPreset = null;
		m_CurrentZoom = 0f;
		m_DeltaZoom = 0f;
		if (m_IsInit)
		{
			DirtyZoom();
		}
	}

	protected IEnumerator OnEndZoom()
	{
		yield return null;
		m_Zoom = false;
		m_DeltaZoom = 0f;
	}

	public void SetMinZoom(float val)
	{
		m_MinZoom = val;
	}

	internal void EnableCamera()
	{
		EnsureCamera();
		m_CameraEnabled = true;
		UpdateCameraState();
	}

	internal void DisableCamera()
	{
		EnsureCamera();
		m_CameraEnabled = false;
		UpdateCameraState();
	}

	private void EnsureCamera()
	{
		if (m_Camera == null)
		{
			m_Camera = base.gameObject.EnsureComponent<Camera>();
			m_Camera.depth = UICamera.Claim().depth - 1f;
		}
	}

	public void SetTargetTexture(RenderTexture targetTexture)
	{
		EnsureCamera();
		m_Camera.targetTexture = targetTexture;
	}
}

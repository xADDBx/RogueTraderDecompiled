using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class OrbitCamera : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	public Transform target;

	public Vector3 targetOffset;

	public float distance = 5f;

	public float maxDistance = 20f;

	public float minDistance = 0.6f;

	public float xSpeed = 200f;

	public float ySpeed = 200f;

	public int yMinLimit = -80;

	public int yMaxLimit = 80;

	public int zoomRate = 40;

	public float panSpeed = 0.3f;

	public float zoomDampening = 5f;

	private float xDeg;

	private float yDeg;

	private float currentDistance;

	private float desiredDistance;

	private Quaternion currentRotation;

	private Quaternion desiredRotation;

	private Quaternion rotation;

	private Vector3 position;

	public GraphicRaycaster m_Raycaster;

	public PointerEventData m_PointerEventData;

	public EventSystem m_EventSystem;

	private string enterObjName;

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
	}

	public void Init()
	{
		if (!target)
		{
			GameObject gameObject = new GameObject("Cam Target");
			gameObject.transform.position = base.transform.position + base.transform.forward * distance;
			target = gameObject.transform;
		}
		distance = Vector3.Distance(base.transform.position, target.position);
		currentDistance = distance;
		desiredDistance = distance;
		position = base.transform.position;
		rotation = base.transform.rotation;
		currentRotation = base.transform.rotation;
		desiredRotation = base.transform.rotation;
		xDeg = Vector3.Angle(Vector3.right, base.transform.right);
		yDeg = Vector3.Angle(Vector3.up, base.transform.up);
	}

	private void LateUpdate()
	{
		if (!Input.GetMouseButton(0))
		{
			if (Input.GetMouseButton(1))
			{
				xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
				yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
				desiredRotation = Quaternion.Euler(yDeg, xDeg, 0f);
				currentRotation = base.transform.rotation;
				rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
				base.transform.rotation = rotation;
			}
			else if (Input.GetMouseButton(2))
			{
				float num = 1f;
				if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
				{
					num = 0.1f;
				}
				target.rotation = base.transform.rotation;
				target.Translate(Vector3.right * (0f - Input.GetAxis("Mouse X")) * panSpeed * num);
				target.Translate(base.transform.up * (0f - Input.GetAxis("Mouse Y")) * panSpeed * num, Space.World);
			}
		}
		bool flag = true;
		if (Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f)
		{
			PointerEventData pointerEventData = new PointerEventData(m_EventSystem);
			pointerEventData.position = Input.mousePosition;
			List<RaycastResult> list = new List<RaycastResult>();
			m_EventSystem.RaycastAll(pointerEventData, list);
			if (list.Count > 0)
			{
				foreach (RaycastResult item in list)
				{
					if (item.gameObject.layer == LayerMask.NameToLayer("UI"))
					{
						flag = false;
						break;
					}
				}
			}
		}
		if (flag)
		{
			desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * (float)zoomRate * Mathf.Abs(desiredDistance);
			desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
		}
		currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
		position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
		base.transform.position = position;
		m_EventSystem.UpdateModules();
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		enterObjName = eventData.pointerCurrentRaycast.gameObject.name;
		Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
	}
}

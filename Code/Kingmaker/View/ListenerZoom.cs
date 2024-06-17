using UnityEngine;

namespace Kingmaker.View;

public class ListenerZoom : MonoBehaviour
{
	[SerializeField]
	protected CameraZoom m_Input;

	[SerializeField]
	protected Vector3 m_DirectionZoom = Vector3.up;

	[SerializeField]
	protected float m_Min;

	[SerializeField]
	protected float m_Max = 1f;

	protected Vector3 m_Orgin;

	protected void Start()
	{
		m_Orgin = base.transform.localPosition;
	}

	protected void Update()
	{
		if ((bool)m_Input)
		{
			base.transform.localPosition = m_Orgin + m_DirectionZoom * Mathf.Lerp(m_Min, m_Max, m_Input.CurrentNormalizePosition);
		}
	}
}

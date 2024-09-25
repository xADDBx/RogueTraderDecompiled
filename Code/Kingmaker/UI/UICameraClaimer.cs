using UnityEngine;

namespace Kingmaker.UI;

[RequireComponent(typeof(Canvas))]
public class UICameraClaimer : MonoBehaviour
{
	private Canvas m_Canvas;

	private void Awake()
	{
		m_Canvas = GetComponent<Canvas>();
	}

	private void OnEnable()
	{
		m_Canvas.worldCamera = UICamera.Claim();
	}

	private void OnDisable()
	{
		m_Canvas.worldCamera = null;
	}
}

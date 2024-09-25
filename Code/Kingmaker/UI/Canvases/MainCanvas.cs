using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.UI.Canvases;

public class MainCanvas : MonoBehaviour
{
	private RectTransform m_RectTransform;

	public static MainCanvas Instance { get; private set; }

	public RectTransform RectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

	[UsedImplicitly]
	private void OnEnable()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Instance = null;
	}
}

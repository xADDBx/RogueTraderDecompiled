using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

[RequireComponent(typeof(Scrollbar))]
public class ScrollSizeReset : MonoBehaviour
{
	private Scrollbar m_Scrollbar;

	private Scrollbar Scrollbar => m_Scrollbar = (m_Scrollbar ? m_Scrollbar : GetComponent<Scrollbar>());

	[UsedImplicitly]
	private void Awake()
	{
		Scrollbar.size = 0f;
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		Scrollbar.size = 0f;
	}
}

using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.DLC;

public class DlcVisualSwitch : MonoBehaviour
{
	[SerializeField]
	private BlueprintDlcReference m_Dlc;

	[SerializeField]
	private GameObject m_defaultVisual;

	[SerializeField]
	private GameObject m_dlcVisual;

	public bool IsDLCActive => m_Dlc.Get().IsAvailable;

	private void Start()
	{
		if (IsDLCActive)
		{
			m_defaultVisual.SetActive(value: false);
			m_dlcVisual.SetActive(value: true);
		}
		else
		{
			m_dlcVisual.SetActive(value: false);
			m_defaultVisual.SetActive(value: true);
		}
	}
}

using Kingmaker.Blueprints.Root;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipCrewPanelBarBlock : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private Image m_Bar;

	private const float kCritRatio = 0.5f;

	public void SetLabel(string label)
	{
		m_Label.text = label;
	}

	public void SetTextValue(string value)
	{
		m_Value.text = value;
	}

	public void SetRatioValue(float ratio)
	{
		if (m_Bar != null)
		{
			m_Bar.fillAmount = ratio;
			m_Bar.color = ((ratio > 0.5f) ? UIConfig.Instance.SpaceCombat.CrewPanelBarColorNormal : UIConfig.Instance.SpaceCombat.CrewPanelBarColorCritical);
		}
	}
}

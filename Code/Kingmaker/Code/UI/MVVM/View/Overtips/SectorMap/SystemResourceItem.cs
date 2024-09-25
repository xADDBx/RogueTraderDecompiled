using Kingmaker.Globalmap.Blueprints.Colonization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap;

public class SystemResourceItem : MonoBehaviour
{
	[SerializeField]
	private Image m_ItemIcon;

	[SerializeField]
	private Sprite m_UnknownItem;

	[SerializeField]
	private TextMeshProUGUI m_ItemLabel;

	public void InitializePlanetResource(BlueprintResource itemBp, int itemAmount)
	{
		m_ItemIcon.sprite = ((itemBp.Icon != null) ? itemBp.Icon : m_UnknownItem);
		m_ItemLabel.text = itemAmount.ToString();
	}

	public void Initialize(BlueprintResource itemBp, int itemValue)
	{
		m_ItemIcon.sprite = ((itemBp.Icon != null) ? itemBp.Icon : m_UnknownItem);
		m_ItemLabel.text = itemValue.ToString();
	}
}

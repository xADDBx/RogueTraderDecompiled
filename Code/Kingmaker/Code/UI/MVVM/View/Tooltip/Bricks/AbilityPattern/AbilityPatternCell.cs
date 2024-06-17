using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks.AbilityPattern;

[RequireComponent(typeof(RectTransform))]
public class AbilityPatternCell : MonoBehaviour
{
	[SerializeField]
	private GameObject m_CommonView;

	[SerializeField]
	private GameObject m_StartView;

	[SerializeField]
	private GameObject m_CasterView;

	public void Initialize(AbilityPatternCellType cellType)
	{
		m_StartView.SetActive(cellType == AbilityPatternCellType.Start);
		m_CommonView.SetActive(cellType == AbilityPatternCellType.Common);
		m_CasterView.SetActive(cellType == AbilityPatternCellType.Caster);
	}
}

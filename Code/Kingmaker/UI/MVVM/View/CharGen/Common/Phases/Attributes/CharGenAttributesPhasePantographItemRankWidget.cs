using Owlcat.Runtime.UI.Controls.Selectable;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;

public class CharGenAttributesPhasePantographItemRankWidget : MonoBehaviour
{
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	public void SetState(bool state)
	{
		m_Selectable.SetActiveLayer(state ? "Active" : "Inactive");
	}
}

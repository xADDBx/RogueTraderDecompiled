using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorPagedView : TextureSelectorCommonView
{
	[SerializeField]
	private List<GameObject> m_NoSelectionsGroup;

	[SerializeField]
	private List<GameObject> m_HasSelectionsGroup;

	[SerializeField]
	private TextMeshProUGUI m_NoSelectionsWarning;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.SetActive(value: true);
		m_NoSelectionsWarning.text = base.ViewModel.NoItemsDesc.Value;
	}

	protected override void OnAvailableStateChange(bool state)
	{
		m_NoSelectionsGroup.ForEach(delegate(GameObject go)
		{
			go.SetActive(!state);
		});
		m_HasSelectionsGroup.ForEach(delegate(GameObject go)
		{
			go.SetActive(state);
		});
	}

	public override bool IsValid()
	{
		return true;
	}
}

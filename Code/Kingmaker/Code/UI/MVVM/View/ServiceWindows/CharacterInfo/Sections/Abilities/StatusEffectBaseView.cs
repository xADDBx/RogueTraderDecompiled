using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public abstract class StatusEffectBaseView : CharInfoFeatureSimpleBaseView
{
	[FormerlySerializedAs("m_Description")]
	[SerializeField]
	protected TextMeshProUGUI m_Duration;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupDescription();
		TextHelper.AppendTexts(m_Duration);
	}

	private void SetupDescription()
	{
		if (!(m_Duration == null))
		{
			m_Duration.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.TimeLeft));
			m_Duration.text = base.ViewModel.TimeLeft;
		}
	}
}

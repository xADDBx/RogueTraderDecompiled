using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;

public class CharGenCharInfoSkillView : CharInfoSkillPCView
{
	[SerializeField]
	private GameObject m_RecommendedMark;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.HighlightedBySource.Subscribe(delegate(bool value)
		{
			string activeLayer = (value ? "Highlighted" : "Normal");
			m_Selectable.SetActiveLayer(activeLayer);
		}));
		if ((bool)m_RecommendedMark)
		{
			AddDisposable(base.ViewModel.IsRecommended.Subscribe(delegate(bool value)
			{
				m_RecommendedMark.SetActive(value);
			}));
		}
	}

	protected override void SetValues(int statValue, int previewValue, int bonus)
	{
		base.SetValues(previewValue, previewValue, bonus);
	}
}

using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.EntitySystem.Stats.Base;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;

public class CharInfoSkillsBlockCommonView : CharInfoComponentWithLevelUpView<CharInfoSkillsBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CharacterSkillsLabel;

	[Header("Containers")]
	[SerializeField]
	private Transform m_SkillContainer;

	protected List<CharInfoSkillPCView> SkillEntries;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.OnStatsUpdated.Subscribe(delegate
		{
			BindEntries();
		}));
		if ((bool)m_CharacterSkillsLabel)
		{
			m_CharacterSkillsLabel.text = UIStrings.Instance.CharacterSheet.Skills;
		}
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		CreateEntries();
		BindEntries();
	}

	private void BindEntries()
	{
		List<CharInfoStatVM> sortedSkills = GetSortedSkills();
		for (int i = 0; i < sortedSkills.Count; i++)
		{
			SkillEntries[i].Bind(sortedSkills[i]);
		}
	}

	private void CreateEntries()
	{
		if (SkillEntries == null || !SkillEntries.Any())
		{
			SkillEntries = new List<CharInfoSkillPCView>();
			CharInfoSkillPCView[] componentsInChildren = m_SkillContainer.GetComponentsInChildren<CharInfoSkillPCView>();
			foreach (CharInfoSkillPCView charInfoSkillPCView in componentsInChildren)
			{
				charInfoSkillPCView.Initialize();
				SkillEntries.Add(charInfoSkillPCView);
			}
		}
	}

	private List<CharInfoStatVM> GetSortedSkills()
	{
		List<StatType> skills = CharInfoSkillsBlockVM.SkillsOrdered;
		return (from s in base.ViewModel.Stats
			orderby (!s.SourceStatType.HasValue) ? int.MaxValue : skills.IndexOf(s.SourceStatType.Value), s.Name.Value
			select s).ToList();
	}
}

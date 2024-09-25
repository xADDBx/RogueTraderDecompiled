using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoAbilityScoresBlockBaseView : CharInfoComponentWithLevelUpView<CharInfoAbilityScoresBlockVM>
{
	[SerializeField]
	protected List<CharInfoAbilityScorePCView> m_StatEntries;

	[SerializeField]
	private Transform m_StatsContainer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.OnStatsUpdated.Subscribe(delegate
		{
			BindEntries();
		}));
		CreateEntries();
		BindEntries();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		CreateEntries();
		BindEntries();
		foreach (CharInfoAbilityScorePCView statEntry in m_StatEntries)
		{
			statEntry.gameObject.SetActive(statEntry.IsBinded);
		}
	}

	private void BindEntries()
	{
		for (int i = 0; i < base.ViewModel.Stats.Count; i++)
		{
			m_StatEntries[i].Bind(base.ViewModel.Stats[i]);
		}
	}

	private void CreateEntries()
	{
		if (m_StatEntries == null || !m_StatEntries.Any())
		{
			m_StatEntries = new List<CharInfoAbilityScorePCView>();
			CharInfoAbilityScorePCView[] componentsInChildren = m_StatsContainer.GetComponentsInChildren<CharInfoAbilityScorePCView>();
			foreach (CharInfoAbilityScorePCView item in componentsInChildren)
			{
				m_StatEntries.Add(item);
			}
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation(int columnsCount = 2)
	{
		m_NavigationBehaviour.Clear();
		if (columnsCount > 1)
		{
			int num = Mathf.CeilToInt(1f * (float)m_StatEntries.Count / (float)columnsCount);
			for (int i = 0; i < columnsCount; i++)
			{
				m_NavigationBehaviour.AddColumn(m_StatEntries.Skip(i * num).Take(num).ToArray());
			}
		}
		else
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_StatEntries);
		}
		return m_NavigationBehaviour;
	}
}

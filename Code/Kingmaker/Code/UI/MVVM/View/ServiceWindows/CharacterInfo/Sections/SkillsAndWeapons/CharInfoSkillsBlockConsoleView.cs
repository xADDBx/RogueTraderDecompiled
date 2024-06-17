using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;

public class CharInfoSkillsBlockConsoleView : CharInfoSkillsBlockCommonView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	public void CreateNavigation(int columnsCount)
	{
		m_NavigationBehaviour.Clear();
		if (columnsCount > 1)
		{
			int num = Mathf.CeilToInt(1f * (float)SkillEntries.Count / (float)columnsCount);
			for (int i = 0; i < columnsCount; i++)
			{
				m_NavigationBehaviour.AddColumn(SkillEntries.Skip(i * num).Take(num).ToArray());
			}
		}
		else
		{
			m_NavigationBehaviour.SetEntitiesVertical(SkillEntries);
		}
	}

	public IConsoleEntity GetConsoleEntity(int columnsCount = 2)
	{
		CreateNavigation(columnsCount);
		return m_NavigationBehaviour;
	}
}

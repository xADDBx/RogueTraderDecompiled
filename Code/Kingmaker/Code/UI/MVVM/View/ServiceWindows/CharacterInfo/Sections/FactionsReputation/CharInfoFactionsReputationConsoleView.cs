using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoFactionsReputationConsoleView : CharInfoFactionsReputationPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[SerializeField]
	private int m_RowsCount = 2;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly List<IHasInputHandler> m_EntitiesWithInput = new List<IHasInputHandler>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		UpdateNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_EntitiesWithInput.Clear();
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		List<IConsoleNavigationEntity> list = m_WidgetList.Entries.Select((IWidgetView e) => e as IConsoleNavigationEntity).ToList();
		int num = Mathf.CeilToInt(1f * (float)list.Count / (float)m_RowsCount);
		for (int i = 0; i < m_RowsCount; i++)
		{
			int count = Mathf.Min(list.Count - i * num, num);
			List<IConsoleNavigationEntity> range = list.GetRange(i * num, count);
			m_NavigationBehaviour.AddRow(range);
			foreach (IConsoleNavigationEntity item2 in range)
			{
				if (item2 is IHasInputHandler item)
				{
					m_EntitiesWithInput.Add(item);
				}
			}
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
		foreach (IHasInputHandler item in m_EntitiesWithInput)
		{
			item.AddInput(inputLayer, hintsWidget);
		}
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}

using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Stats;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyStatsBaseView : ViewBase<ColonyStatsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListStats;

	public void Initialize()
	{
		m_Title.text = UIStrings.Instance.ColonizationTexts.ColonyStatsTitle;
	}

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.UpdateStatsCommand.Subscribe(DrawEntities));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		DrawEntitiesImpl();
	}

	protected virtual void DrawEntitiesImpl()
	{
	}

	public IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetListStats.GetNavigationEntities();
	}
}

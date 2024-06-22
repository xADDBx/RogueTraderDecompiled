using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Console;
using Kingmaker.UI.MVVM.View.DlcManager.Dlcs.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;

public class DlcManagerTabModsModSelectorConsoleView : DlcManagerTabModsModSelectorBaseView
{
	[Header("PC Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerModEntityConsoleView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (m_WidgetList.Entries == null || !m_WidgetList.Entries.Any())
		{
			return;
		}
		foreach (DlcManagerModEntityConsoleView item in m_WidgetList.Entries.OfType<DlcManagerModEntityConsoleView>())
		{
			item.CreateInputImpl(inputLayer, hintsWidget);
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null || !m_WidgetList.Entries.Any())
		{
			return list;
		}
		list.AddRange(m_WidgetList.Entries.OfType<DlcManagerModEntityConsoleView>());
		return list;
	}
}

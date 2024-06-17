using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyEventsBaseView : ViewBase<ColonyEventsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListEvents;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	public void Initialize()
	{
		m_HeaderLabel.text = UIStrings.Instance.ColonyEventsTexts.EventsHeader.Text;
	}

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.UpdateEventsCommand.Subscribe(DrawEntities));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetListEvents.GetNavigationEntities();
	}

	public void ScrollList(ColonyEventBaseView entity)
	{
		m_ScrollRect.EnsureVisibleVertical(entity.transform as RectTransform, 50f, smoothly: false, needPinch: false);
	}

	private void DrawEntities()
	{
		DrawEntitiesImpl();
	}

	protected virtual void DrawEntitiesImpl()
	{
	}
}

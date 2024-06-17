using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.PC;

public class ContextMenuView : ViewBase<ContextMenuVM>
{
	[SerializeField]
	private ContextMenuEntitiesView m_Config;

	[SerializeField]
	private RectTransform m_Container;

	protected readonly List<MonoBehaviour> m_Entities = new List<MonoBehaviour>();

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			InitializeImplementation();
			m_IsInit = true;
		}
	}

	protected virtual void InitializeImplementation()
	{
	}

	protected override void BindViewImplementation()
	{
		foreach (ContextMenuEntityVM entity in base.ViewModel.Entities)
		{
			MonoBehaviour entityView = ContextMenuEngine.GetEntityView(m_Config, entity);
			entityView.transform.SetParent(m_Container, worldPositionStays: false);
			m_Entities.Add(entityView);
		}
		AddDisposable(DelayedInvoker.InvokeInTime(delegate
		{
			base.gameObject.SetActive(value: true);
			UIUtility.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.Owner);
		}, 0.1f));
	}

	protected override void DestroyViewImplementation()
	{
		m_Entities.ForEach(WidgetFactory.DisposeWidget);
		m_Entities.Clear();
		base.gameObject.SetActive(value: false);
	}
}

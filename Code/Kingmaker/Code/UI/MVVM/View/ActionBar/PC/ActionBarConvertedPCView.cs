using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class ActionBarConvertedPCView : ViewBase<ActionBarConvertedVM>
{
	[SerializeField]
	private ActionBarBaseSlotView m_SlotView;

	[SerializeField]
	private RectTransform m_Container;

	private VisibilityController m_Visibility;

	private bool m_IsInit;

	private readonly List<ActionBarBaseSlotView> m_Slots = new List<ActionBarBaseSlotView>();

	private void Awake()
	{
		m_Visibility = VisibilityController.Control(base.gameObject);
		m_Visibility.SetVisible(visible: false);
	}

	protected override void BindViewImplementation()
	{
		foreach (ActionBarSlotVM slot in base.ViewModel.Slots)
		{
			ActionBarBaseSlotView widget = WidgetFactory.GetWidget(m_SlotView);
			widget.Initialize();
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Bind(slot);
			m_Slots.Add(widget);
		}
		m_Visibility.SetVisible(visible: true);
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_Slots.ForEach(WidgetFactory.DisposeWidget);
		m_Slots.Clear();
		m_Visibility.SetVisible(visible: false);
	}
}

using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarPartConsumablesPCView : ViewBase<SurfaceActionBarPartConsumablesVM>
{
	[SerializeField]
	private OwlcatButton m_InventoryButton;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private SurfaceActionBarSlotConsumablePCView m_SlotPCView;

	[SerializeField]
	private RectTransform m_LeftSideTooltipPlace;

	private List<Vector2> m_LeftSideTooltipPivots { get; } = new List<Vector2>
	{
		new Vector2(1f, 0f),
		new Vector2(0.9f, 0f),
		new Vector2(0.8f, 0f),
		new Vector2(0.7f, 0f),
		new Vector2(0.6f, 0f)
	};


	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		UISounds.Instance.SetClickAndHoverSound(m_InventoryButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_InventoryButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
			{
				h.HandleOpenInventory();
			});
		}));
		AddDisposable(m_InventoryButton.SetHint(UIStrings.Instance.MainMenu.Inventory, "OpenInventory"));
		AddDisposable(base.ViewModel.UnitChanged.Subscribe(delegate
		{
			DrawEntries();
		}));
		DrawEntries();
	}

	private void DrawEntries()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.Slots.ToArray(), m_SlotPCView));
		SetActionBarSlotsTooltipCustomPosition();
		SetBindings();
	}

	private void SetActionBarSlotsTooltipCustomPosition()
	{
		foreach (IWidgetView visibleEntry in m_WidgetList.VisibleEntries)
		{
			if (visibleEntry is SurfaceActionBarSlotConsumablePCView surfaceActionBarSlotConsumablePCView)
			{
				surfaceActionBarSlotConsumablePCView.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
			}
		}
	}

	private void SetBindings()
	{
		for (int i = 0; i < m_WidgetList.Entries?.Count; i++)
		{
			(m_WidgetList.Entries[i] as SurfaceActionBarSlotConsumablePCView).Or(null)?.SetKeyBinding(i);
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_WidgetList.VisibleEntries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_WidgetList.Clear();
	}
}

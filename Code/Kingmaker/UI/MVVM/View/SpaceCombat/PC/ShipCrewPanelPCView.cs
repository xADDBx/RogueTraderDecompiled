using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Other;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipCrewPanelPCView : ViewBase<ShipCrewPanelVM>
{
	[Header("Buffs")]
	[SerializeField]
	private BuffPCView m_BuffView;

	[SerializeField]
	private ScrollRectExtended m_BuffsScrollRect;

	[SerializeField]
	private OwlcatButton m_ScrollLeftButton;

	[SerializeField]
	private OwlcatButton m_ScrollRightButton;

	[Header("Ship Doll")]
	[SerializeField]
	private ShipCrewDollPCView m_ShipCrewDollView;

	[Header("Stats")]
	[SerializeField]
	private ShipCrewPanelBarBlock m_CrewBlock;

	[SerializeField]
	private ShipCrewPanelBarBlock m_MoraleBlock;

	[SerializeField]
	private ShipCrewPanelBarBlock m_MilitaryRatingBlock;

	private readonly List<BuffPCView> m_BuffList = new List<BuffPCView>();

	public void Initialize()
	{
		m_ShipCrewDollView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_ShipCrewDollView.Bind(base.ViewModel.ShipCrewDollVM);
		m_CrewBlock.SetLabel(UIStrings.Instance.SpaceCombatTexts.Crew);
		m_MoraleBlock.SetLabel(UIStrings.Instance.SpaceCombatTexts.Morale);
		m_MilitaryRatingBlock.SetLabel(UIStrings.Instance.SpaceCombatTexts.MilitaryRating);
		AddDisposable(base.ViewModel.ShipCrewValue.Subscribe(m_CrewBlock.SetTextValue));
		AddDisposable(base.ViewModel.ShipCrewRatio.Subscribe(m_CrewBlock.SetRatioValue));
		AddDisposable(base.ViewModel.ShipMoraleValue.Subscribe(m_MoraleBlock.SetTextValue));
		AddDisposable(base.ViewModel.ShipMoraleRatio.Subscribe(m_MoraleBlock.SetRatioValue));
		AddDisposable(base.ViewModel.ShipMilitaryRating.Subscribe(m_MilitaryRatingBlock.SetTextValue));
		AddDisposable(base.ViewModel.ShipBuffs.ObserveAdd().Subscribe(delegate
		{
			DrawBuffs();
		}));
		AddDisposable(base.ViewModel.ShipBuffs.ObserveRemove().Subscribe(delegate
		{
			DrawBuffs();
		}));
		AddDisposable(base.ViewModel.ShipBuffs.ObserveReset().Subscribe(delegate
		{
			ClearBuffs();
		}));
		DrawBuffs();
		AddDisposable(m_ScrollLeftButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			m_BuffsScrollRect.ScrollToLeft();
		}));
		AddDisposable(m_ScrollRightButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			m_BuffsScrollRect.ScrollToRight();
		}));
		m_BuffsScrollRect.onValueChanged.AddListener(OnScrollChanged);
		OnScrollChanged(Vector2.zero);
	}

	protected override void DestroyViewImplementation()
	{
		m_BuffsScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
		ClearBuffs();
	}

	private void DrawBuffs()
	{
		ClearBuffs();
		foreach (BuffVM shipBuff in base.ViewModel.ShipBuffs)
		{
			BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
			widget.Bind(shipBuff);
			widget.transform.SetParent(m_BuffsScrollRect.content, worldPositionStays: false);
			m_BuffList.Add(widget);
		}
	}

	private void ClearBuffs()
	{
		m_BuffList.ForEach(WidgetFactory.DisposeWidget);
		m_BuffList.Clear();
	}

	private void OnScrollChanged(Vector2 value)
	{
		m_ScrollLeftButton.gameObject.SetActive(m_BuffsScrollRect.LeftEdgeNeeded);
		m_ScrollRightButton.gameObject.SetActive(m_BuffsScrollRect.RightEdgeNeeded);
	}
}

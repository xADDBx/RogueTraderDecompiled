using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Transition.Common;

public class TransitionBaseView : ViewBase<TransitionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MapName;

	[SerializeField]
	private List<TransitionMapPart> m_Parts = new List<TransitionMapPart>();

	[SerializeField]
	private PantographView m_PantographView;

	[SerializeField]
	private float m_DefaultPantographMaxY = -100f;

	[SerializeField]
	private TransitionLegendButtonView m_TransitionLegendButtonViewPrefab;

	protected TransitionMapPart CurrentPart;

	private readonly List<Action> m_HoverActions = new List<Action>();

	private readonly List<Action> m_UnHoverActions = new List<Action>();

	private List<TransitionLegendButtonView> m_TransitionLegendButtonView;

	private void ShowBeam()
	{
		CurrentPart.LightBeamCanvas.DOFade(1f, 0.25f).SetUpdate(isIndependentUpdate: true);
	}

	private void MoveBeam(bool state, PointOnMap pointOnMap)
	{
		int index = CurrentPart.PointsOnMap.IndexOf(pointOnMap);
		base.ViewModel.ObjectsList.ForEach(delegate(TransitionLegendButtonVM o)
		{
			o.IsHover.Value = o == base.ViewModel.ObjectsList[index];
		});
		if (!state)
		{
			HideBeam();
		}
		else if (pointOnMap.LightBeamPointSettings != null)
		{
			ShowBeam();
			CurrentPart.LightBeam.transform.DOLocalMove(new Vector3(pointOnMap.LightBeamPointSettings.LocalPosition.x, pointOnMap.LightBeamPointSettings.LocalPosition.y), 0.25f).SetUpdate(isIndependentUpdate: true);
			CurrentPart.LightBeam.transform.DORotateQuaternion(Quaternion.Euler(pointOnMap.LightBeamPointSettings.Rotation), 0.25f).SetUpdate(isIndependentUpdate: true);
			CurrentPart.LightBeam.anchoredPosition = pointOnMap.LightBeamPointSettings.LocalPosition;
			CurrentPart.LightBeam.localScale = pointOnMap.LightBeamPointSettings.LocalScale;
			pointOnMap.PointButton.SetFocus(value: true);
			CurrentPart.PointsOnMap.Where((PointOnMap pp) => pp != pointOnMap).ToList().ForEach(delegate(PointOnMap pp)
			{
				pp.PointButton.SetFocus(value: false);
			});
		}
	}

	private void HideBeam()
	{
		CurrentPart.LightBeamCanvas.DOFade(0f, 0.25f).SetUpdate(isIndependentUpdate: true);
	}

	public void SetBeamOnCurrentLocation()
	{
		TransitionLegendButtonVM transitionLegendButtonVM = base.ViewModel.ObjectsList.FirstOrDefault((TransitionLegendButtonVM o) => o.IsVisible && o.TransitionEntryVM.IsCurrentlyLocation);
		if (transitionLegendButtonVM != null)
		{
			transitionLegendButtonVM.IsHover.Value = true;
			return;
		}
		base.ViewModel.ObjectsList.ForEach(delegate(TransitionLegendButtonVM o)
		{
			o.IsHover.Value = false;
		});
		EventBus.RaiseEvent(delegate(IPantographHandler h)
		{
			h.Unbind();
		});
		HideBeam();
		CurrentPart.PointsOnMap.ForEach(delegate(PointOnMap p)
		{
			p.PointButton.SetFocus(value: false);
		});
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_PantographView.Show());
		m_Parts.ForEach(delegate(TransitionMapPart p)
		{
			p.Initialize();
		});
		CurrentPart = m_Parts.First((TransitionMapPart p) => p.Map == base.ViewModel.Map);
		CurrentPart.MapObject.SetActive(value: true);
		m_PantographView.SetCustomMaxY(CurrentPart.CustomPantographMaxY ? CurrentPart.CustomPantographMaxYValue : m_DefaultPantographMaxY);
		if (m_MapName != null)
		{
			m_MapName.text = base.ViewModel.Name;
		}
		foreach (TransitionEntryVM entryVm in base.ViewModel.EntryVms)
		{
			TransitionEntryBaseView transitionEntryBaseView = CurrentPart.Entries.FirstOrDefault((TransitionEntryBaseView v) => v.EntranceEntry == entryVm.Entry);
			if (!(transitionEntryBaseView == null))
			{
				transitionEntryBaseView.Bind(entryVm);
			}
		}
		SetVisible(state: true);
		foreach (PointOnMap p in CurrentPart.PointsOnMap)
		{
			m_HoverActions.Add(delegate
			{
				MoveBeam(state: true, p);
			});
			m_UnHoverActions.Add(SetBeamOnCurrentLocation);
		}
		base.ViewModel.AddObjectsInfo(m_HoverActions, m_UnHoverActions);
		DrawObjects();
		CurrentPart.PointsOnMap.Where((PointOnMap o) => o.PointButton.Interactable).ToList().ForEach(delegate(PointOnMap p)
		{
			AddDisposable(p.PointButton.OnHoverAsObservable().Subscribe(delegate(bool value)
			{
				if (value)
				{
					MoveBeam(value, p);
				}
				else
				{
					SetBeamOnCurrentLocation();
				}
			}));
		});
		SetBeamOnCurrentLocation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.TransitionMap);
		});
	}

	private void DrawObjects()
	{
		TransitionLegendButtonVM[] vmCollection = base.ViewModel.ObjectsList.ToArray();
		CurrentPart.WidgetList.DrawEntries(vmCollection, m_TransitionLegendButtonViewPrefab);
	}

	protected override void DestroyViewImplementation()
	{
		CurrentPart.WidgetList.Clear();
		m_HoverActions.Clear();
		m_UnHoverActions.Clear();
		SetVisible(state: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.TransitionMap);
		});
	}

	private void SetVisible(bool state)
	{
		base.gameObject.SetActive(state);
		UISounds.Instance.Play(state ? UISounds.Instance.Sounds.Dialogue.BookOpen : UISounds.Instance.Sounds.Dialogue.BookClose);
	}
}

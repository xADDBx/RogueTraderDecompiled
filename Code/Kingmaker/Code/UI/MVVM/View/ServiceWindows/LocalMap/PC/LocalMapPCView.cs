using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.UI.Controls.Button;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.PC;

public class LocalMapPCView : LocalMapBaseView, IPointerClickHandler, IEventSystemHandler, IScrollHandler, IDragHandler
{
	[Header("Right Buttons PC")]
	[SerializeField]
	private OwlcatButton m_ZoomPlusButton;

	[SerializeField]
	private OwlcatButton m_CenterOnRogueTraderButton;

	[SerializeField]
	private OwlcatButton m_ZoomMinusButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ZoomPlusButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			SetMapScale(1f);
		}));
		AddDisposable(m_ZoomMinusButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			SetMapScale(-1f);
		}));
		AddDisposable(m_CenterOnRogueTraderButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			FindRogueTrader(smooth: true);
		}));
		AddDisposable(m_CenterOnRogueTraderButton.SetHint(UIStrings.Instance.LocalMapTexts.CenterOnRogueTrader));
		AddDisposable(m_ZoomPlusButton.SetHint(UIStrings.Instance.LocalMapTexts.ZoomMapPlus));
		AddDisposable(m_ZoomMinusButton.SetHint(UIStrings.Instance.LocalMapTexts.ZoomMapMinus));
	}

	protected override void InteractableRightButtons()
	{
		base.InteractableRightButtons();
		Vector3 localScale = m_Image.rectTransform.localScale;
		OwlcatButton centerOnRogueTraderButton = m_CenterOnRogueTraderButton;
		OwlcatButton zoomMinusButton = m_ZoomMinusButton;
		bool flag2 = (MinZoom.Value = localScale.x > m_ZoomMinSize && localScale.y > m_ZoomMinSize);
		bool interactable = (zoomMinusButton.Interactable = flag2);
		centerOnRogueTraderButton.Interactable = interactable;
		OwlcatButton zoomPlusButton = m_ZoomPlusButton;
		interactable = (MaxZoom.Value = localScale.x < m_ZoomMaxSize && localScale.y < m_ZoomMaxSize);
		zoomPlusButton.Interactable = interactable;
	}

	public void OnDrag(PointerEventData eventData)
	{
		try
		{
			switch (eventData.button)
			{
			case PointerEventData.InputButton.Left:
			{
				Entity entity = eventData.pointerPress.GetComponent<LocalMapMarkerPCView>()?.GetEntity();
				base.ViewModel.OnClick(GetViewportPos(eventData), state: true, entity);
				break;
			}
			case PointerEventData.InputButton.Middle:
				UpdateMapPosition(eventData.delta);
				break;
			}
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"NullReferenceException {arg}");
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Middle)
		{
			Entity entity = eventData.pointerPress.GetComponent<LocalMapMarkerPCView>()?.GetEntity();
			base.ViewModel.OnClick(GetViewportPos(eventData), eventData.button == PointerEventData.InputButton.Left, entity);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		SetMapScale((eventData.scrollDelta.y > 0f) ? 1 : (-1));
	}
}

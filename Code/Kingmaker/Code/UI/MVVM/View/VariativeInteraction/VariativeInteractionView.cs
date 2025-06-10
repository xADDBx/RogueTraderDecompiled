using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Kingmaker.UI.Sound;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction;

public class VariativeInteractionView<TInteractionVariant> : ViewBase<VariativeInteractionVM> where TInteractionVariant : InteractionVariantView
{
	[SerializeField]
	protected WidgetList WidgetList;

	[SerializeField]
	protected TInteractionVariant m_InteractionVariantViewPrefab;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		OnUpdateHandler();
		AddDisposable(WidgetList.DrawEntries(base.ViewModel.Variants.ToArray(), m_InteractionVariantViewPrefab));
		SetButtonsPosition(WidgetList.VisibleEntries.Select((IWidgetView v) => v.MonoBehaviour.transform as RectTransform).ToList());
		RectTransform obj = (RectTransform)base.transform;
		Rect rect = ((RectTransform)m_InteractionVariantViewPrefab.transform).rect;
		obj.sizeDelta = new Vector2((rect.width + 5f) * (float)base.ViewModel.Variants.Count, rect.height);
		obj.anchoredPosition = new Vector2(0f, base.ViewModel.VerticalCorrection + 5f - rect.height);
	}

	protected override void DestroyViewImplementation()
	{
		WidgetList.HideAll();
		base.gameObject.SetActive(value: false);
	}

	private void OnUpdateHandler()
	{
		UpdatePosition();
		if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
		{
			base.ViewModel.Close();
		}
	}

	private void SetButtonsPosition(List<RectTransform> buttons)
	{
		buttons.ForEach(delegate(RectTransform b)
		{
			b.anchoredPosition = Vector2.zero;
		});
		for (int i = 0; i < buttons.Count(); i++)
		{
			float num = buttons[i].rect.width + 5f;
			float x = (float)(-i) * num - num / 2f + (float)buttons.Count * num / 2f;
			ShortcutExtensions46.DOAnchorPos(endValue: new Vector2(x, 0f), target: buttons[i], duration: 0.2f).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void UpdatePosition()
	{
		if (!(CameraRig.Instance.Camera == null))
		{
			Vector2 vector = CameraRig.Instance.WorldToViewport(base.ViewModel.ObjectWorldPosition);
			RectTransform obj = (RectTransform)base.transform;
			Vector2 anchorMax = (obj.anchorMin = vector);
			obj.anchorMax = anchorMax;
		}
	}
}

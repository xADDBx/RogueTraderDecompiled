using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.OvertipParts;

public class MapObjectOvertipNameBlockView : ViewBase<OvertipMapObjectVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_NameText;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private List<RectTransform> m_ContainersList;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Name.Subscribe(SetName));
		AddDisposable(base.ViewModel.MapObjectIsHighlited.CombineLatest(base.ViewModel.ForceHotKeyPressed, base.ViewModel.IsMouseOverUI, (bool hover, bool force, bool mouseOver) => hover || force || mouseOver).Subscribe(delegate(bool value)
		{
			SetVisible(value && !string.IsNullOrEmpty(base.ViewModel.Name.Value));
		}));
	}

	private void SetName(string value)
	{
		float x = UIUtility.CalculateBarkWidth(value, m_NameText.fontSize);
		m_NameText.text = value;
		RectTransform rectTransform = (RectTransform)m_NameText.transform;
		rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
		foreach (RectTransform containers in m_ContainersList)
		{
			containers.sizeDelta = new Vector2(x, containers.sizeDelta.y);
		}
	}

	private void SetVisible(bool state)
	{
		m_FadeAnimator.PlayAnimation(state);
	}

	protected override void DestroyViewImplementation()
	{
	}
}

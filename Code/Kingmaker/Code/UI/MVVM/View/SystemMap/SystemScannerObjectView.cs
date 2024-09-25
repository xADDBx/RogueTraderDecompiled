using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class SystemScannerObjectView : ViewBase<SystemScannerObjectVM>, IWidgetView
{
	[SerializeField]
	private Image m_ObjectIcon;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	private RectTransform m_ObjectTransform;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_ObjectTransform = (RectTransform)base.transform;
		m_ObjectIcon.sprite = base.ViewModel.Icon;
		RectTransform objectTransform = m_ObjectTransform;
		Vector2 anchorMin = (m_ObjectTransform.anchorMax = Vector2.zero);
		objectTransform.anchorMin = anchorMin;
		SetObjectPositions(base.ViewModel.Position);
	}

	private void SetObjectPositions(Vector3 pos)
	{
		Rect rect = ((RectTransform)m_ObjectTransform.parent).rect;
		m_ObjectTransform.anchoredPosition = new Vector2(pos.x * rect.width, pos.y * rect.height);
		m_CanvasGroup.alpha = 0f;
		m_CanvasGroup.DOFade(1f, 0.5f);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((SystemScannerObjectVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SystemScannerObjectVM;
	}
}

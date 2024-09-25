using System;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.PagesMenu;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.PagesMenu;

public class CharInfoPagesMenuEntityPCView : SelectionGroupEntityView<CharInfoPagesMenuEntityVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private Transform m_Lens;

	private float m_LensDistanceThreshold;

	private float m_LensAnimationDuration;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Label.text = base.ViewModel.Label;
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
		base.DestroyViewImplementation();
	}

	public void SetupLens(Transform lens, float lensDistanceThreshold, float lensAnimationDuration)
	{
		m_Lens = lens;
		m_LensDistanceThreshold = lensDistanceThreshold;
		m_LensAnimationDuration = lensAnimationDuration;
		DelayedInvoker.InvokeInFrames(InitializeLensPosition, 1);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoPagesMenuEntityVM);
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.RefreshView, delegate
		{
			UpdateLensPosition();
		}));
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoPagesMenuEntityVM;
	}

	private void InitializeLensPosition()
	{
		if (base.ViewModel != null && base.ViewModel.IsSelected.Value && !(m_Lens == null))
		{
			DOTween.Kill(m_Lens.transform);
			m_Lens.transform.localPosition = base.gameObject.transform.localPosition;
		}
	}

	private void UpdateLensPosition()
	{
		if (base.ViewModel.IsSelected.Value && !(m_Lens == null) && !(Math.Abs(m_Lens.transform.localPosition.x - base.gameObject.transform.localPosition.x) < m_LensDistanceThreshold))
		{
			DOTween.Kill(m_Lens.transform);
			UIUtility.MoveXLensPosition(m_Lens.transform, base.gameObject.transform.localPosition.x, m_LensAnimationDuration);
		}
	}
}

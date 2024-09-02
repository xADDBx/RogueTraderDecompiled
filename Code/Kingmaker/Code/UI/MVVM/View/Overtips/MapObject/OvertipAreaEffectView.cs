using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public class OvertipAreaEffectView : BaseOvertipView<OvertipAreaEffectVM>
{
	[Header("Common Block")]
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	[SerializeField]
	private float m_FarDistance = 120f;

	[SerializeField]
	private float m_StandardOvertipPositionYCorrection = 100f;

	[SerializeField]
	private OwlcatButton m_InfoButton;

	private readonly ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	protected override bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.IsVisibleForPlayer.Value)
			{
				return !base.ViewModel.AreaEffectEntity.Suppressed;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.name = base.ViewModel.AreaEffectEntity.View.GameObjectName + "_AreaEffectOvertip";
		AddDisposable(m_InfoButton.SetTooltip(new TooltipTemplateBuff(base.ViewModel.Buff)));
		AddDisposable(base.ViewModel.CameraDistance.Subscribe(delegate
		{
			UpdateVisibility();
		}));
		AddDisposable(m_Visibility.Subscribe(DoVisibility));
	}

	private void DoVisibility(UnitOvertipVisibility unitOvertipVisibility)
	{
		UnitOvertipVisibilitySettings? unitOvertipVisibilitySettings = m_UnitOvertipVisibilitySettings.FirstOrDefault((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == unitOvertipVisibility);
		float alpha = unitOvertipVisibilitySettings.Value.Alpha;
		float scale = unitOvertipVisibilitySettings.Value.Scale;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_InnerCanvasGroup.DOFade(alpha, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = m_RectTransform.DOScale(scale, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		if (unitOvertipVisibility == UnitOvertipVisibility.NotFull || unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized)
		{
			base.transform.SetAsLastSibling();
		}
		PositionCorrectionFromView = new Vector2(0f, m_StandardOvertipPositionYCorrection);
	}

	private void UpdateVisibility()
	{
		if (!CheckVisibility || base.ViewModel.IsCutscene || base.ViewModel.IsInDialog)
		{
			m_Visibility.Value = UnitOvertipVisibility.Invisible;
			return;
		}
		bool flag = base.ViewModel.CameraDistance.Value.sqrMagnitude < m_FarDistance;
		m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		base.DestroyViewImplementation();
	}
}

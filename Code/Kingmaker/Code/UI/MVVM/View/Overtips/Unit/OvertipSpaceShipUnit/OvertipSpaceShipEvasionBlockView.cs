using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.OvertipSpaceShipUnit;

public class OvertipSpaceShipEvasionBlockView : ViewBase<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Image m_HintPlace;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.EvasionChance.CombineLatest(base.ViewModel.IsVisibleTrigger, (float chance, bool visible) => new { chance, visible }).Subscribe(value =>
		{
			m_CanvasGroup.alpha = Mathf.Clamp01(value.chance);
			m_CanvasGroup.blocksRaycasts = value.visible && value.chance > 0f;
		}));
		AddDisposable(m_HintPlace.SetHint(UIStrings.Instance.CombatTexts.Avoid));
	}

	protected override void DestroyViewImplementation()
	{
	}
}

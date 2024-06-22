using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.OvertipParts;

public class MapObjectOvertipSkillCheckBlockView : ViewBase<OvertipMapObjectVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_SkillCheckText;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ObjectDescription.Subscribe(delegate(string text)
		{
			m_DescriptionText.text = text;
		}));
		AddDisposable(base.ViewModel.ObjectSkillCheckText.Subscribe(delegate(string text)
		{
			m_SkillCheckText.text = text;
		}));
		AddDisposable(base.ViewModel.IsEnabled.CombineLatest(base.ViewModel.MapObjectIsHighlited, base.ViewModel.ForceHotKeyPressed, base.ViewModel.IsMouseOverUI, base.ViewModel.ObjectDescription, base.ViewModel.ObjectSkillCheckText, base.ViewModel.ForceHideInCombat, (bool enable, bool hover, bool force, bool mouseOver, string descr, string skillcheck, bool forceHideInCombat) => enable && !forceHideInCombat && (hover || force || mouseOver) && (!descr.IsNullOrEmpty() || !skillcheck.IsNullOrEmpty())).Subscribe(delegate(bool value)
		{
			m_FadeAnimator.PlayAnimation(value && Game.Instance.SelectionCharacter.SelectedUnit.Value.IsMyNetRole());
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}

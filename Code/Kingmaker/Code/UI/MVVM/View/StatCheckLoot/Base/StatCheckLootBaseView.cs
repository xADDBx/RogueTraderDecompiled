using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;

public abstract class StatCheckLootBaseView : ViewBase<StatCheckLootVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_SelectUnitHeaderLabel;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(SetVisibility));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetVisibility(bool value)
	{
		if (value)
		{
			m_FadeAnimator.AppearAnimation();
			return;
		}
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}

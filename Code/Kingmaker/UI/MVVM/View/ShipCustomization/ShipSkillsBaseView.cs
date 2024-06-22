using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipSkillsBaseView<TShipCareerPathTabs> : ViewBase<ShipSkillsVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected InfoSectionView m_InfoSection;

	[SerializeField]
	protected TShipCareerPathTabs m_ShipCareerPathSelectionTabsPCView;

	[SerializeField]
	protected CareerPathRoundProgressionCommonView m_CareerPathRoundProgression;

	[SerializeField]
	protected ShipRankExpCounterPCView m_RankExpCounterPCView;

	private readonly List<RankEntryItemCommonView> m_RankEntries = new List<RankEntryItemCommonView>();

	[SerializeField]
	protected GameObject m_LockBackground;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_CareerPathRoundProgression.Initialize();
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		m_RankExpCounterPCView.Bind(base.ViewModel.ShipProgressionVM?.ShipInfoExperienceVM);
		m_CareerPathRoundProgression.Bind(base.ViewModel.ShipProgressionVM?.CareerPathVM);
		m_InfoSection.Bind(base.ViewModel.ShipProgressionVM?.CareerPathVM?.SelectedItemInfoSectionVM);
		AddDisposable(base.ViewModel.IsLocked.Subscribe(delegate(bool val)
		{
			m_LockBackground.SetActive(val);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
		Clear();
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation();
	}

	private void HideWindow()
	{
		m_FadeAnimator.DisappearAnimation(OnDisappearEnd);
	}

	private void OnDisappearEnd()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Clear()
	{
		m_RankEntries.ForEach(WidgetFactory.DisposeWidget);
		m_RankEntries.Clear();
	}
}

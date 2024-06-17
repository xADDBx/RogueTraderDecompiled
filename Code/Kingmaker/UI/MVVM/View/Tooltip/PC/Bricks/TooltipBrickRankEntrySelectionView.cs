using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickRankEntrySelectionView : TooltipBaseBrickView<TooltipBrickRankEntrySelectionVM>
{
	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_CharInfoRankEntryView;

	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_MainButtonImage;

	[SerializeField]
	private RankEntrySelectionStateSprites[] m_StateSprites;

	[SerializeField]
	private TextMeshProUGUI m_SelectionLabel;

	[SerializeField]
	private float m_DefaultFontSizeLabel = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeLabel = 24f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.RankEntrySelectionVM.SelectedFeature.Subscribe(delegate(RankEntrySelectionFeatureVM featureVM)
		{
			m_CharInfoRankEntryView.Bind(featureVM);
			IDisposable disposable2;
			if (featureVM == null)
			{
				IDisposable disposable = m_MainButton.SetTooltip(base.ViewModel.RankEntrySelectionVM.Tooltip);
				disposable2 = disposable;
			}
			else
			{
				disposable2 = m_MainButton.SetTooltip(featureVM.Tooltip);
			}
			AddDisposable(disposable2);
			string text = ((featureVM != null) ? featureVM.DisplayName : UIUtility.GetGlossaryEntryName(base.ViewModel.RankEntrySelectionVM.GlossaryEntryKey));
			if (featureVM is RankEntrySelectionStatVM rankEntrySelectionStatVM)
			{
				text = rankEntrySelectionStatVM.StatDisplayName;
			}
			m_SelectionLabel.text = text;
		}));
		AddDisposable(base.ViewModel.RankEntrySelectionVM.EntryState.Subscribe(UpdateState));
		m_SelectionLabel.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSizeLabel : m_DefaultConsoleFontSizeLabel) * FontMultiplier;
	}

	private void UpdateState(RankEntryState entryState)
	{
		RankEntrySelectionStateSprites rankEntrySelectionStateSprites = m_StateSprites.FirstOrDefault((RankEntrySelectionStateSprites p) => p.FeatureGroup == base.ViewModel.RankEntrySelectionVM.FeatureGroup);
		if (rankEntrySelectionStateSprites == null)
		{
			UILog.Warning($"Could not found sprites for feature group: {base.ViewModel.RankEntrySelectionVM.FeatureGroup}");
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		else if (entryState == RankEntryState.NotSelectable || entryState == RankEntryState.FirstSelectable || entryState == RankEntryState.WaitPreviousToSelect || entryState == RankEntryState.Selectable)
		{
			m_MainButtonImage.sprite = rankEntrySelectionStateSprites.Icon;
			m_MainButtonImage.gameObject.SetActive(value: true);
		}
		else
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		m_MainButton.SetActiveLayer(entryState.ToString());
	}
}

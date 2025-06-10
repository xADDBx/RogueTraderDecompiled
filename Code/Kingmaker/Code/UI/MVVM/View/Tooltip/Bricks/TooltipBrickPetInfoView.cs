using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPetInfoView : TooltipBaseBrickView<TooltipBrickPetInfoVM>
{
	[SerializeField]
	private VideoPlayerHelper m_VideoPlayerHelper;

	[SerializeField]
	private TextMeshProUGUI m_MovementPoints;

	[SerializeField]
	private TextMeshProUGUI m_NarrativeDescription;

	[SerializeField]
	private TextMeshProUGUI m_CharacteristicsTitle;

	[SerializeField]
	private TextMeshProUGUI m_KeyStatsTitle;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionTitle;

	[SerializeField]
	private TooltipBrickIconAndNameView m_BrickIconAndNameViewPrefab;

	[SerializeField]
	protected WidgetList m_AbilitiesList;

	[SerializeField]
	private TooltipBrickShortLabelView m_BrickShortLabelViewPrefab;

	[SerializeField]
	protected WidgetList m_KeyStatsList;

	[SerializeField]
	protected CharInfoAbilityScoresBlockBaseView m_AbilityScorePCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CharacteristicsTitle.text = UIStrings.Instance.Pets.CharacteristicsTitle;
		m_DescriptionTitle.text = UIStrings.Instance.Pets.DescriptionTitle;
		m_KeyStatsTitle.text = UIStrings.Instance.Tooltips.Ranks;
		AddDisposable(base.ViewModel.MovementPoints.Subscribe(delegate(int v)
		{
			m_MovementPoints.text = v.ToString();
		}));
		AddDisposable(base.ViewModel.PetVideo.Subscribe(SetVideoClip));
		AddDisposable(base.ViewModel.PetDescription.Subscribe(delegate
		{
			m_NarrativeDescription.text = base.ViewModel.PetDescription.Value;
		}));
		AddDisposable(m_AbilitiesList.DrawEntries(base.ViewModel.CoreAbilities, m_BrickIconAndNameViewPrefab));
		AddDisposable(m_KeyStatsList.DrawEntries(base.ViewModel.KeyStats, m_BrickShortLabelViewPrefab));
		m_AbilityScorePCView.Initialize();
		m_AbilityScorePCView.Bind(base.ViewModel.AbilityScores);
	}

	private void SetVideoClip(VideoClip newClip)
	{
		m_VideoPlayerHelper.Initialize();
		if (!(newClip == null))
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VideoPlayerHelper.SetClip(newClip, SoundStateType.Default, prepareVideo: true, string.Empty, string.Empty);
				m_VideoPlayerHelper.Play();
			}, 2);
		}
	}
}

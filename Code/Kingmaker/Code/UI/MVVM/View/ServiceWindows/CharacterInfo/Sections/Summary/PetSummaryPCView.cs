using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;

public class PetSummaryPCView : CharInfoComponentView<PetSummaryVM>
{
	[SerializeField]
	private TooltipBrickTextView m_TooltipBrickTextView;

	[SerializeField]
	protected TooltipBrickTextConsoleView m_TooltipBrickTextConsoleView;

	[SerializeField]
	private VideoPlayerHelper m_VideoPlayerHelper;

	[SerializeField]
	private TextMeshProUGUI m_NarrativeDescription;

	[SerializeField]
	private TextMeshProUGUI m_StrategyTitle;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_VideoPlayerHelper.Initialize();
		m_TooltipBrickTextView.Or(null)?.Bind(base.ViewModel.StrategyDescription.Value);
		m_TooltipBrickTextConsoleView.Or(null)?.Bind(base.ViewModel.StrategyDescription.Value);
		AddDisposable(base.ViewModel.RefreshCommand.Subscribe(Refresh));
		AddDisposable(base.ViewModel.IsUnitPet.Subscribe(delegate
		{
			m_FadeAnimator.PlayAnimation(base.ViewModel.IsUnitPet.Value);
		}));
		AddDisposable(base.ViewModel.PetVideoClip.Subscribe(SetVideoClip));
		AddDisposable(base.ViewModel.NarrativeDescription.Subscribe(delegate(string narrative)
		{
			m_NarrativeDescription.text = narrative;
		}));
		m_StrategyTitle.text = UIStrings.Instance.Pets.StrategyTitle;
		RefreshView();
	}

	private void SetVideoClip(VideoClip newClip)
	{
		if (!(newClip == null))
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VideoPlayerHelper.SetClip(newClip, SoundStateType.Default, prepareVideo: true, string.Empty, string.Empty);
				m_VideoPlayerHelper.Play();
			}, 1);
		}
	}

	public void Refresh()
	{
		SetVideoClip(base.ViewModel.PetVideoClip.Value);
		m_StrategyTitle.text = UIStrings.Instance.Pets.StrategyTitle;
		m_NarrativeDescription.text = base.ViewModel.NarrativeDescription.Value;
		if (base.ViewModel.StrategyDescription.Value != null)
		{
			m_TooltipBrickTextView.Or(null)?.Bind(new TooltipBrickTextVM(base.ViewModel.StrategyDescription.Value.Text, TooltipTextType.Simple));
			m_TooltipBrickTextConsoleView.Or(null)?.Bind(new TooltipBrickTextVM(base.ViewModel.StrategyDescription.Value.Text, TooltipTextType.Simple));
		}
	}
}

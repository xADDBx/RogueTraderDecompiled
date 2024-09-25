using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class SystemTitleView : ViewBase<SystemTitleVM>
{
	[Header("System Name Block")]
	[SerializeField]
	private TextMeshProUGUI m_SystemTitleNameText;

	[Header("System Name Block")]
	[SerializeField]
	private TextMeshProUGUI m_TitleResearchCountText;

	[Header("Tooltip Taker")]
	[SerializeField]
	private Image m_TitleTooltipTaker;

	[SerializeField]
	private RectTransform m_TitleTooltipPlace;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShouldShow.CombineLatest(base.ViewModel.IsOnSystemMap, base.ViewModel.IsInSpaceCombat, (bool shouldShow, bool isOnSystemMap, bool isInSpaceCombat) => new { shouldShow, isOnSystemMap, isInSpaceCombat }).Subscribe(value =>
		{
			base.gameObject.SetActive(value.shouldShow && value.isOnSystemMap && !value.isInSpaceCombat);
		}));
		AddDisposable(base.ViewModel.IsOnSystemMap.Subscribe(delegate(bool value)
		{
			if (value)
			{
				SetSystemName();
				AddDisposable(m_TitleTooltipTaker.SetTooltip(new TooltipTemplateGlobalMapSystem(Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TitleTooltipPlace)));
			}
		}));
	}

	private void SetSystemName()
	{
		m_SystemTitleNameText.text = Game.Instance.CurrentlyLoadedArea.Name;
	}

	private void SetResearchCount(float value)
	{
		m_TitleResearchCountText.text = $"{value}%";
	}

	protected override void DestroyViewImplementation()
	{
	}
}

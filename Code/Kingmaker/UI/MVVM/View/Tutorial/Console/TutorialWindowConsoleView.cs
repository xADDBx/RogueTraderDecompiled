using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Tutorial.PC;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tutorial.Console;

public abstract class TutorialWindowConsoleView<TViewModel> : TutorialWindowBaseView<TViewModel> where TViewModel : TutorialWindowVM
{
	[SerializeField]
	protected ConsoleHint m_ToggleHint;

	[SerializeField]
	protected ConsoleHint m_GlossaryHint;

	[SerializeField]
	protected ConsoleHint m_CloseGlossaryHint;

	[SerializeField]
	protected ConsoleHint m_EncyclopediaHint;

	[SerializeField]
	protected OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	protected OwlcatMultiButton m_SecondGlossaryFocus;

	[SerializeField]
	private float m_TitleDefaultConsoleFontSize = 28f;

	[SerializeField]
	private float m_TriggerDefaultSize = 24f;

	[SerializeField]
	private float m_MainTextsDefaultConsoleFontSize = 24f;

	protected InputLayer GlossaryInputLayer;

	protected FloatConsoleNavigationBehaviour NavigationBehaviour;

	protected InputLayer InputLayer;

	private IConsoleEntity m_FirstEnt;

	protected string LinkKey;

	protected readonly BoolReactiveProperty IsGlossaryMode = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty HasGlossaryPoints = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty IsPossibleGoToEncyclopedia = new BoolReactiveProperty();

	private readonly List<IFloatConsoleNavigationEntity> m_Entities = new List<IFloatConsoleNavigationEntity>();

	protected abstract void OnFocusLink(string key);

	protected abstract void Focus();

	protected virtual void GoToEncyclopedia()
	{
		CloseGlossary();
		if (TooltipHelper.GetLinkTooltipTemplate(LinkKey) is TooltipTemplateGlossary tooltipTemplateGlossary)
		{
			tooltipTemplateGlossary.EncyclopediaCallback();
		}
		TooltipHelper.HideTooltip();
		base.ViewModel?.TemporarilyHide();
	}

	protected void SelectDeselectToggle(InputActionEventData eventData)
	{
		UISounds.Instance.Sounds.Tutorial.BanTutorialType.Play();
		m_DontShowToggle.Set(!m_DontShowToggle.IsOn.Value);
	}

	protected void DelayedGlossaryCalculation()
	{
		DelayedInvoker.InvokeInFrames(CalculateGlossary, 5);
	}

	private void CalculateGlossary()
	{
		m_Entities.Clear();
		NavigationBehaviour.Clear();
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		List<IFloatConsoleNavigationEntity> list2 = new List<IFloatConsoleNavigationEntity>();
		list = TMPLinkNavigationGenerator.GenerateEntityList(m_TutorialText, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusLink, TooltipHelper.GetLinkTooltipTemplate);
		if (m_SolutionText.text != null)
		{
			list2 = TMPLinkNavigationGenerator.GenerateEntityList(m_SolutionText, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusLink, TooltipHelper.GetLinkTooltipTemplate);
		}
		m_FirstEnt = list.FirstOrDefault();
		foreach (IFloatConsoleNavigationEntity item in list)
		{
			m_Entities.Add(item);
		}
		if (!list2.Empty())
		{
			foreach (IFloatConsoleNavigationEntity item2 in list2)
			{
				m_Entities.Add(item2);
			}
		}
		NavigationBehaviour.AddEntities(m_Entities);
		HasGlossaryPoints.Value = m_Entities.Any();
	}

	protected void ShowGlossary()
	{
		AddDisposable(GamePad.Instance.PushLayer(GlossaryInputLayer));
		IsGlossaryMode.Value = true;
		NavigationBehaviour.FocusOnEntityManual(m_FirstEnt);
	}

	protected void CloseGlossary()
	{
		if (GlossaryInputLayer != null)
		{
			GamePad.Instance.PopLayer(GlossaryInputLayer);
		}
		IsGlossaryMode.Value = false;
		TooltipHelper.HideTooltip();
		NavigationBehaviour?.UnFocusCurrentEntity();
	}

	public override void Show()
	{
		base.Show();
		m_DontShowToggle.Set(value: false);
	}

	protected override void SetTextsSize(float multiplier)
	{
		m_Title.fontSize = m_TitleDefaultConsoleFontSize * multiplier;
		m_TriggerText.fontSize = m_TriggerDefaultSize * multiplier;
		m_TutorialText.fontSize = m_MainTextsDefaultConsoleFontSize * multiplier;
		m_SolutionText.fontSize = m_MainTextsDefaultConsoleFontSize * multiplier;
		base.SetTextsSize(multiplier);
	}
}

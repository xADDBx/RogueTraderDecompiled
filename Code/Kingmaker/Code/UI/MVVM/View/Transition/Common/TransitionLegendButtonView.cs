using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Transition.Common;

public class TransitionLegendButtonView : ViewBase<TransitionLegendButtonVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private OwlcatButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private Image m_Attention;

	public PantographConfig PantographConfig { get; private set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(base.ViewModel.IsVisible);
		AddDisposable(base.ViewModel.Attention.Subscribe(delegate(bool value)
		{
			m_Attention.gameObject.SetActive(value);
			if (value)
			{
				m_Button.SetTooltip(base.ViewModel.TransitionEntryVM.GetTooltipTemplate(), new TooltipConfig(InfoCallPCMethod.None));
				Sprite firstObjectiveTypeSprite = GetFirstObjectiveTypeSprite();
				m_Attention.gameObject.SetActive(firstObjectiveTypeSprite != null);
				if (!(firstObjectiveTypeSprite == null))
				{
					m_Attention.sprite = firstObjectiveTypeSprite;
				}
			}
		}));
		m_Title.text = base.ViewModel.Name;
		AddDisposable(m_Button.OnHoverAsObservable().Subscribe(delegate(bool value)
		{
			if (value)
			{
				if (!base.ViewModel.IsHover.Value)
				{
					base.ViewModel.HoverAction();
					EventBus.RaiseEvent(delegate(IPantographHandler h)
					{
						h.Bind(PantographConfig);
					});
				}
			}
			else
			{
				base.ViewModel.UnHoverAction();
			}
		}));
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}));
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}));
		AddDisposable(base.ViewModel.IsHover.Subscribe(delegate(bool value)
		{
			if (value)
			{
				base.ViewModel.HoverAction();
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Bind(PantographConfig);
				});
			}
			UISounds.Instance.SetClickAndHoverSound(m_Button, base.ViewModel.IsHover.Value ? UISounds.ButtonSoundsEnum.NoSound : UISounds.ButtonSoundsEnum.NormalSound);
		}));
		SetupPantographConfig();
	}

	private Sprite GetFirstObjectiveTypeSprite(bool isPaper = true)
	{
		BlueprintQuestObjective blueprintQuestObjective = base.ViewModel.TransitionEntryVM.Entry.GetLinkedObjectives().FirstOrDefault();
		if (blueprintQuestObjective == null)
		{
			return null;
		}
		QuestType type = blueprintQuestObjective.Quest.Type;
		if (!isPaper)
		{
			return BlueprintRoot.Instance.UIConfig.UIIcons.QuestTypesIcons.GetQuestMonitorTypeIcon(type);
		}
		return BlueprintRoot.Instance.UIConfig.UIIcons.QuestTypesIcons.GetQuestPaperTypeIcon(type);
	}

	private void SetupPantographConfig()
	{
		List<Sprite> list = new List<Sprite>();
		if (base.ViewModel.Attention.Value)
		{
			list.Add(GetFirstObjectiveTypeSprite(isPaper: false));
		}
		PantographConfig = new PantographConfig(base.transform, m_Title.text, list);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((TransitionLegendButtonVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TransitionLegendButtonVM;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		base.ViewModel.IsHover.Value = value;
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public bool CheckHover()
	{
		return base.ViewModel.IsHover.Value;
	}
}

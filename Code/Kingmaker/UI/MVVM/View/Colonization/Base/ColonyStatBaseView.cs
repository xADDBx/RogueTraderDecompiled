using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Stats;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyStatBaseView : ViewBase<ColonyStatVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[Serializable]
	private struct ColonyStatTypeIcon
	{
		public ColonyStatType Type;

		public Sprite Icon;
	}

	[SerializeField]
	private Image m_StatIcon;

	[SerializeField]
	private TextMeshProUGUI m_StatName;

	[SerializeField]
	private TextMeshProUGUI m_StatValue;

	[SerializeField]
	private Color32 m_NormalColor;

	[SerializeField]
	private Color32 m_ModifiedColor;

	[SerializeField]
	private ColonyStatTypeIcon[] m_ColonyStatTypeIcons;

	[SerializeField]
	protected OwlcatMultiButton m_FocusButton;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.StatType.Subscribe(SetStatIcon));
		AddDisposable(base.ViewModel.StatName.Subscribe(delegate(string statName)
		{
			m_StatName.text = statName;
		}));
		AddDisposable(base.ViewModel.StatValue.Subscribe(delegate(int statValue)
		{
			m_StatValue.text = statValue.ToString();
		}));
		AddDisposable(base.ViewModel.IsNegativelyModified.Subscribe(delegate(bool val)
		{
			SetStatColor(m_StatValue, val);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetStatIcon(ColonyStatType colonyStatType)
	{
		ColonyStatTypeIcon[] colonyStatTypeIcons = m_ColonyStatTypeIcons;
		for (int i = 0; i < colonyStatTypeIcons.Length; i++)
		{
			ColonyStatTypeIcon colonyStatTypeIcon = colonyStatTypeIcons[i];
			if (colonyStatTypeIcon.Type == colonyStatType)
			{
				m_StatIcon.sprite = colonyStatTypeIcon.Icon;
				return;
			}
		}
		PFLog.UI.Error("ColonyStatBaseView.SetStatIcon - can't find stat icon");
	}

	private void SetStatColor(TextMeshProUGUI statText, bool isModified)
	{
		statText.color = (isModified ? m_ModifiedColor : m_NormalColor);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ColonyStatVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyStatVM;
	}

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public bool CanInteract()
	{
		return false;
	}

	public bool CanShowTooltip()
	{
		return true;
	}

	public void Interact()
	{
	}

	public void ShowTooltip()
	{
		this.ShowTooltip(base.ViewModel.Tooltip.Value);
	}
}

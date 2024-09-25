using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class AnomalyCharacterPCView : ViewBase<AnomalyCharacterVM>, IWidgetView
{
	[SerializeField]
	private Image m_CharacterPortrait;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private TextMeshProUGUI m_RequiredSkill;

	[SerializeField]
	private TextMeshProUGUI m_SkillValue;

	[SerializeField]
	private TextMeshProUGUI m_SkillValueModificator;

	[SerializeField]
	private OwlcatButton m_ChooseButton;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoFeaturePCView m_WidgetEntityView;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.NeedUpdate.Subscribe(delegate
		{
			DrawEntities();
		}));
		m_CharacterPortrait.sprite = base.ViewModel.CharacterPortrait;
		m_CharacterName.text = base.ViewModel.CharacterName;
		m_RequiredSkill.text = base.ViewModel.RequiredSkillName;
		m_SkillValue.text = base.ViewModel.SkillValue;
		m_SkillValueModificator.text = base.ViewModel.SkillValueModificator;
		AddDisposable(m_ChooseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ScanAnomaly();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		if (base.ViewModel.FeatureList != null)
		{
			m_WidgetList.DrawEntries(base.ViewModel.FeatureList.ToArray(), m_WidgetEntityView);
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as AnomalyCharacterVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is AnomalyCharacterVM;
	}
}

using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Formation.Base;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Formation;

public class FormationPCView : FormationBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private FormationCharacterPCView m_CharacterView;

	[Header("Buttons")]
	[SerializeField]
	[UsedImplicitly]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	[UsedImplicitly]
	private OwlcatButton m_ResetButton;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_FormationHintPc;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_ResetLabel;

	private readonly List<FormationCharacterPCView> m_Characters = new List<FormationCharacterPCView>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
		AddDisposable(m_ResetButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ResetCurrentFormation();
		}));
		m_FormationHintPc.text = SetFormationHintText();
		m_ResetLabel.text = UIStrings.Instance.FormationTexts.RestoreToDefault;
		foreach (FormationCharacterVM character in base.ViewModel.Characters)
		{
			FormationCharacterPCView widget = WidgetFactory.GetWidget(m_CharacterView);
			widget.transform.SetParent(m_CharacterContainer, worldPositionStays: false);
			widget.Bind(character);
			m_Characters.Add(widget);
		}
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, delegate
		{
			base.ViewModel.FormationSelector.SelectPrevValidEntity();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, base.OnSelectFormation));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Characters.ForEach(WidgetFactory.DisposeWidget);
		m_Characters.Clear();
	}

	private string SetFormationHintText()
	{
		return base.ViewModel.IsCustomFormation ? UIStrings.Instance.FormationTexts.FormationPcHint : UIStrings.Instance.FormationTexts.OptimizedFormation;
	}

	public override void OnFormationPresetIndexChanged(int formationPresetIndex)
	{
		base.OnFormationPresetIndexChanged(formationPresetIndex);
		m_ResetButton.Interactable = base.ViewModel.IsCustomFormation;
		m_FormationHintPc.text = SetFormationHintText();
	}
}

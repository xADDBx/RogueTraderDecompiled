using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Formation.Base;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.UI.InputSystems;
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
		m_FormationHintPc.text = UIStrings.Instance.FormationTexts.FormationPcHint;
		m_ResetLabel.text = UIStrings.Instance.FormationTexts.RestoreToDefault;
		foreach (FormationCharacterVM character in base.ViewModel.Characters)
		{
			FormationCharacterPCView widget = WidgetFactory.GetWidget(m_CharacterView);
			widget.transform.SetParent(m_CharacterContainer, worldPositionStays: false);
			widget.Bind(character);
			m_Characters.Add(widget);
		}
	}

	public override void OnFormationPresetIndexChanged(int formationPresetIndex)
	{
		base.OnFormationPresetIndexChanged(formationPresetIndex);
		m_ResetButton.Interactable = base.ViewModel.IsCustomFormation;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Characters.ForEach(WidgetFactory.DisposeWidget);
		m_Characters.Clear();
	}
}

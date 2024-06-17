using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Voice;

public class CharGenVoiceItemView : SelectionGroupEntityView<CharGenVoiceItemVM>, IWidgetView, IFunc01ClickHandler, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_VoiceName;

	[SerializeField]
	private List<Animation> m_AudioAnimations = new List<Animation>();

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_VoiceName.text = base.ViewModel.DisplayName;
		PlayBarkIfSelected();
	}

	protected override void OnClick()
	{
		if (base.ViewModel.IsSelected.Value)
		{
			PlayBarkIfSelected();
			return;
		}
		if (UINetUtility.IsControlMainCharacter())
		{
			base.ViewModel.SetSelectedFromView(!base.ViewModel.IsSelected.Value);
		}
		PlayBarkIfSelected();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value && !base.ViewModel.IsSelected.Value)
		{
			base.ViewModel.Barks.PlayPreview();
			if (UINetUtility.IsControlMainCharacter())
			{
				base.ViewModel.SetSelectedFromView(state: true);
			}
		}
	}

	private void PlayBarkIfSelected()
	{
		if (!base.ViewModel.IsSelected.Value)
		{
			return;
		}
		base.ViewModel.Barks.PlayPreview();
		foreach (Animation audioAnimation in m_AudioAnimations)
		{
			audioAnimation.gameObject.SetActive(value: true);
			audioAnimation.Stop();
			audioAnimation.Play();
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenVoiceItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenVoiceItemVM;
	}

	public bool CanFunc01Click()
	{
		return true;
	}

	public void OnFunc01Click()
	{
		PlayBarkIfSelected();
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.CharGen.PlayVoicePreview;
	}
}

using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.PC.InputField;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.PC;

public class NewSaveSlotPCView : SaveSlotPCView
{
	[SerializeField]
	private PCInputField m_PCInputField;

	public override void DoInitialize()
	{
		UISaveLoadTexts saveLoadTexts = UIStrings.Instance.SaveLoadTexts;
		m_PCInputField.Initialize(saveLoadTexts.ClickToEdit, saveLoadTexts.SaveDefaultName);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(base.gameObject.SetActive));
		AddDisposable(m_PCInputField.Bind(base.ViewModel.SaveName.Value, base.ViewModel.TrySetSaveName));
	}

	public override bool IsValid()
	{
		return base.ViewModel.IsAvailable.Value;
	}
}

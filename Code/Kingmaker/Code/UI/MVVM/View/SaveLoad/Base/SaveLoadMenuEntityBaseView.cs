using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveLoadMenuEntityBaseView : SelectionGroupEntityView<SaveLoadMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private static UISaveLoadTexts Texts => UIStrings.Instance.SaveLoadTexts;

	public override void DoInitialize()
	{
		base.DoInitialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		base.BindViewImplementation();
		switch (base.ViewModel.Mode)
		{
		case SaveLoadMode.Save:
			m_Label.text = Texts.SaveLabel;
			break;
		case SaveLoadMode.Load:
			m_Label.text = Texts.LoadLabel;
			break;
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
	}
}

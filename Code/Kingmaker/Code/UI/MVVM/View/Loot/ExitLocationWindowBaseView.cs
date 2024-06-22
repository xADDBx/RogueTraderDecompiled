using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public class ExitLocationWindowBaseView : ViewBase<ExitLocationWindowVM>
{
	[SerializeField]
	public TextMeshProUGUI Header;

	[SerializeField]
	public TextMeshProUGUI Description;

	[SerializeField]
	public TextMeshProUGUI AdditionalInformation;

	[SerializeField]
	public TextMeshProUGUI AcceptText;

	[SerializeField]
	public TextMeshProUGUI DeclineText;

	public virtual void Initialize()
	{
		Hide();
	}

	protected override void BindViewImplementation()
	{
		Show();
		Header.text = base.ViewModel.Header;
		Description.text = base.ViewModel.Description;
		AdditionalInformation.text = base.ViewModel.AdditionalInformation;
		AcceptText.text = UIStrings.Instance.CommonTexts.Accept;
		DeclineText.text = UIStrings.Instance.CommonTexts.Cancel;
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}

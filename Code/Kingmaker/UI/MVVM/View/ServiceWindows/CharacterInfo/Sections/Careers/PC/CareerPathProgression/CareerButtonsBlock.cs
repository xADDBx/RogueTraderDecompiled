using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression;

public class CareerButtonsBlock : MonoBehaviour
{
	[SerializeField]
	public OwlcatButton NextButton;

	[SerializeField]
	public TextMeshProUGUI NextButtonLabel;

	[SerializeField]
	public OwlcatButton BackButton;

	[SerializeField]
	public TextMeshProUGUI BackButtonLabel;

	[SerializeField]
	public OwlcatButton FirstSelectableButton;

	public void SetActive(bool state)
	{
		base.gameObject.SetActive(state);
	}
}

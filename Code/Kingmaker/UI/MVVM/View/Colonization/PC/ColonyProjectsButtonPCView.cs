using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectsButtonPCView : ViewBase<ColonyProjectsButtonVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatButton m_Button;

	public void Initialize()
	{
		m_Label.text = UIStrings.Instance.ColonyProjectsTexts.OpenProjectsButton;
	}

	protected override void BindViewImplementation()
	{
		UISounds.Instance.SetHoverSound(m_Button, UISounds.ButtonSoundsEnum.ColonyProjectSound);
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleClick();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void HandleClick()
	{
		base.ViewModel.OpenColonyProjects();
	}
}

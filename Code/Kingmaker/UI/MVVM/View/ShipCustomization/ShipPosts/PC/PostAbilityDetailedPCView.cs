using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts.PC;

public class PostAbilityDetailedPCView : PostAbilityDetailedBaseView
{
	[SerializeField]
	private OwlcatButton m_AttuneButton;

	[SerializeField]
	private TextMeshProUGUI m_AttuneButtonText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_AttuneButton.OnSingleLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.TryAttune();
		}));
		AddDisposable(m_AttuneButton.OnSingleLeftClickNotInteractableAsObservable().Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.AttunePrerequisitesNotCompleted);
			});
		}));
	}

	protected override void SetupAttuneBlock()
	{
		base.SetupAttuneBlock();
		m_AttuneButtonText.text = UIStrings.Instance.ShipCustomization.Attune;
		m_AttuneButton.Interactable = base.ViewModel.CanAttune;
	}
}

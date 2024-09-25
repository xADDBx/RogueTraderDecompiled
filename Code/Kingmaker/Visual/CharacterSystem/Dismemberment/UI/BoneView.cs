using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class BoneView : ViewBase<BoneVM>, IWidgetView
{
	public Toggle ToggleSevering;

	public Text Text;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as BoneVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is BoneVM;
	}

	protected override void BindViewImplementation()
	{
		base.ViewModel.IsMarkedForSevering.Subscribe(delegate(bool val)
		{
			ToggleSevering.isOn = val;
		});
		ToggleSevering.onValueChanged.AddListener(base.ViewModel.OnSeveringChanged);
		Text.text = base.ViewModel.Bone.name;
	}

	protected override void DestroyViewImplementation()
	{
		ToggleSevering.onValueChanged.RemoveAllListeners();
	}
}

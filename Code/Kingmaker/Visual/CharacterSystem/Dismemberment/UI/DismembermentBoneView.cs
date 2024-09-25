using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentBoneView : ViewBase<DismembermentBoneVM>, IWidgetView
{
	public TMP_Text TextBoneName;

	public Slider SliderSliceOffset;

	public Slider SliderOrientationX;

	public Slider SliderOrientationY;

	public Slider SliderOrientationZ;

	public Button RemoveButton;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as DismembermentBoneVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DismembermentBoneVM;
	}

	protected override void BindViewImplementation()
	{
		TextBoneName.text = base.ViewModel.DismembermentBone.Transform.name;
		SliderSliceOffset.minValue = 0f;
		SliderSliceOffset.maxValue = 1f;
		SliderSliceOffset.value = base.ViewModel.DismembermentBone.SliceOffset;
		SliderSliceOffset.onValueChanged.AddListener(base.ViewModel.OnSliceOffsetChanged);
		SliderOrientationX.minValue = -30f;
		SliderOrientationX.maxValue = 30f;
		SliderOrientationX.value = base.ViewModel.DismembermentBone.SliceOrientationEuler.x;
		SliderOrientationX.onValueChanged.AddListener(base.ViewModel.OnSliceOrientationXChanged);
		SliderOrientationY.minValue = -30f;
		SliderOrientationY.maxValue = 30f;
		SliderOrientationY.value = base.ViewModel.DismembermentBone.SliceOrientationEuler.y;
		SliderOrientationY.onValueChanged.AddListener(base.ViewModel.OnSliceOrientationYChanged);
		SliderOrientationZ.minValue = -30f;
		SliderOrientationZ.maxValue = 30f;
		SliderOrientationZ.value = base.ViewModel.DismembermentBone.SliceOrientationEuler.z;
		SliderOrientationZ.onValueChanged.AddListener(base.ViewModel.OnSliceOrientationZChanged);
		RemoveButton.onClick.AddListener(base.ViewModel.OnRemove);
	}

	protected override void DestroyViewImplementation()
	{
		SliderSliceOffset.onValueChanged.RemoveAllListeners();
		SliderOrientationX.onValueChanged.RemoveAllListeners();
		SliderOrientationY.onValueChanged.RemoveAllListeners();
		SliderOrientationZ.onValueChanged.RemoveAllListeners();
		RemoveButton.onClick.RemoveAllListeners();
	}
}

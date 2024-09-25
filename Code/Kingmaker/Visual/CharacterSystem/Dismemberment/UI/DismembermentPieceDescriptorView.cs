using System;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentPieceDescriptorView : ViewBase<DismembermentPieceDescriptorVM>, IWidgetView
{
	public Slider ImpulseX;

	public Slider ImpulseY;

	public Slider ImpulseZ;

	public TMP_InputField ImpulseScaleMin;

	public TMP_InputField ImpulseScaleMax;

	public TMP_InputField IncomingImpulseScaleMin;

	public TMP_InputField IncomingImpulseScaleMax;

	public TMP_InputField ChildrenImpulseScaleMin;

	public TMP_InputField ChildrenImpulseScaleMax;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as DismembermentPieceDescriptorVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DismembermentPieceDescriptorVM;
	}

	protected override void BindViewImplementation()
	{
		DismembermentPieceDescriptorVM viewModel = base.ViewModel;
		viewModel.ImpulseChanged = (Action<Vector3>)Delegate.Combine(viewModel.ImpulseChanged, new Action<Vector3>(OnViewModelImpulseChanged));
		ImpulseX.minValue = -1f;
		ImpulseX.maxValue = 1f;
		ImpulseX.value = base.ViewModel.DismembermentPieceDescriptor.Impulse.x;
		ImpulseX.onValueChanged.AddListener(base.ViewModel.OnImpulseXChanged);
		ImpulseY.minValue = -1f;
		ImpulseY.maxValue = 1f;
		ImpulseY.value = base.ViewModel.DismembermentPieceDescriptor.Impulse.y;
		ImpulseY.onValueChanged.AddListener(base.ViewModel.OnImpulseYChanged);
		ImpulseZ.minValue = -1f;
		ImpulseZ.maxValue = 1f;
		ImpulseZ.value = base.ViewModel.DismembermentPieceDescriptor.Impulse.z;
		ImpulseZ.onValueChanged.AddListener(base.ViewModel.OnImpulseZChanged);
		ResetImpulseScaleMin(string.Empty);
		ImpulseScaleMin.onValueChanged.AddListener(base.ViewModel.OnImpulseScaleMinChanged);
		ImpulseScaleMin.onSubmit.AddListener(ResetImpulseScaleMin);
		ImpulseScaleMin.onDeselect.AddListener(ResetImpulseScaleMin);
		ResetImpulseScaleMax(string.Empty);
		ImpulseScaleMax.onValueChanged.AddListener(base.ViewModel.OnImpulseScaleMaxChanged);
		ImpulseScaleMax.onSubmit.AddListener(ResetImpulseScaleMax);
		ImpulseScaleMax.onDeselect.AddListener(ResetImpulseScaleMax);
		ResetIncomingImpulseMin(string.Empty);
		IncomingImpulseScaleMin.onValueChanged.AddListener(base.ViewModel.OnIncomingImpulseScaleMinChanged);
		IncomingImpulseScaleMin.onSubmit.AddListener(ResetIncomingImpulseMin);
		IncomingImpulseScaleMin.onDeselect.AddListener(ResetIncomingImpulseMin);
		ResetIncomingImpulseMax(string.Empty);
		IncomingImpulseScaleMax.onValueChanged.AddListener(base.ViewModel.OnIncomingImpulseScaleMaxChanged);
		IncomingImpulseScaleMax.onSubmit.AddListener(ResetIncomingImpulseMax);
		IncomingImpulseScaleMax.onDeselect.AddListener(ResetIncomingImpulseMax);
		ResetChildrenImpulseMin(string.Empty);
		ChildrenImpulseScaleMin.onValueChanged.AddListener(base.ViewModel.OnChildrenImpulseScaleMinChanged);
		ChildrenImpulseScaleMin.onSubmit.AddListener(ResetChildrenImpulseMin);
		ChildrenImpulseScaleMin.onDeselect.AddListener(ResetChildrenImpulseMin);
		ResetChildrenImpulseMax(string.Empty);
		ChildrenImpulseScaleMax.onValueChanged.AddListener(base.ViewModel.OnChildrenImpulseScaleMaxChanged);
		ChildrenImpulseScaleMax.onSubmit.AddListener(ResetChildrenImpulseMax);
		ChildrenImpulseScaleMax.onDeselect.AddListener(ResetChildrenImpulseMax);
	}

	private void ResetImpulseScaleMin(string value)
	{
		ImpulseScaleMin.SetTextWithoutNotify(base.ViewModel.DismembermentPieceDescriptor.ImpulseMultiplier.x.ToString());
	}

	private void ResetImpulseScaleMax(string value)
	{
		ImpulseScaleMax.SetTextWithoutNotify(base.ViewModel.DismembermentPieceDescriptor.ImpulseMultiplier.y.ToString());
	}

	private void ResetIncomingImpulseMin(string value)
	{
		IncomingImpulseScaleMin.SetTextWithoutNotify(base.ViewModel.DismembermentPieceDescriptor.IncomingImpulseMultiplier.x.ToString());
	}

	private void ResetIncomingImpulseMax(string value)
	{
		IncomingImpulseScaleMax.SetTextWithoutNotify(base.ViewModel.DismembermentPieceDescriptor.IncomingImpulseMultiplier.y.ToString());
	}

	private void ResetChildrenImpulseMin(string empty)
	{
		ChildrenImpulseScaleMin.SetTextWithoutNotify(base.ViewModel.DismembermentPieceDescriptor.ChildrenImpulseMultiplier.x.ToString());
	}

	private void ResetChildrenImpulseMax(string empty)
	{
		ChildrenImpulseScaleMax.SetTextWithoutNotify(base.ViewModel.DismembermentPieceDescriptor.ChildrenImpulseMultiplier.y.ToString());
	}

	private void OnViewModelImpulseChanged(Vector3 impulse)
	{
		ImpulseX.SetValueWithoutNotify(impulse.x);
		ImpulseY.SetValueWithoutNotify(impulse.y);
		ImpulseZ.SetValueWithoutNotify(impulse.z);
	}

	protected override void DestroyViewImplementation()
	{
		ImpulseX.onValueChanged.RemoveAllListeners();
		ImpulseY.onValueChanged.RemoveAllListeners();
		ImpulseZ.onValueChanged.RemoveAllListeners();
		ImpulseScaleMin.onValueChanged.RemoveAllListeners();
		ImpulseScaleMin.onDeselect.RemoveAllListeners();
		ImpulseScaleMin.onSubmit.RemoveAllListeners();
		ImpulseScaleMax.onValueChanged.RemoveAllListeners();
		ImpulseScaleMax.onDeselect.RemoveAllListeners();
		ImpulseScaleMax.onSubmit.RemoveAllListeners();
		IncomingImpulseScaleMin.onValueChanged.RemoveAllListeners();
		IncomingImpulseScaleMin.onDeselect.RemoveAllListeners();
		IncomingImpulseScaleMin.onSubmit.RemoveAllListeners();
		IncomingImpulseScaleMax.onValueChanged.RemoveAllListeners();
		IncomingImpulseScaleMax.onDeselect.RemoveAllListeners();
		IncomingImpulseScaleMax.onSubmit.RemoveAllListeners();
		ChildrenImpulseScaleMin.onValueChanged.RemoveAllListeners();
		ChildrenImpulseScaleMin.onDeselect.RemoveAllListeners();
		ChildrenImpulseScaleMin.onSubmit.RemoveAllListeners();
		ChildrenImpulseScaleMax.onValueChanged.RemoveAllListeners();
		ChildrenImpulseScaleMax.onDeselect.RemoveAllListeners();
		ChildrenImpulseScaleMax.onSubmit.RemoveAllListeners();
		DismembermentPieceDescriptorVM viewModel = base.ViewModel;
		viewModel.ImpulseChanged = (Action<Vector3>)Delegate.Remove(viewModel.ImpulseChanged, new Action<Vector3>(OnViewModelImpulseChanged));
	}
}

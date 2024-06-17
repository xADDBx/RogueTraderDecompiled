using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Colonization;

public class BCResourcePCView : ViewBase<BCResourceVM>
{
	[SerializeField]
	private TextMeshProUGUI m_BCResourceValue;

	[SerializeField]
	private Image m_TooltipArea;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.BCResource.Subscribe(delegate(float currentBC)
		{
			m_BCResourceValue.text = currentBC.ToString();
		}));
	}

	public void SetData()
	{
		base.ViewModel.BCResource.Value = 0f;
	}

	public void UpdateData()
	{
		base.ViewModel.BCResource.Value = 0f;
	}

	protected override void DestroyViewImplementation()
	{
	}
}

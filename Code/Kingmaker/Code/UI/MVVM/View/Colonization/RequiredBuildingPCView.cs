using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Colonization;

public class RequiredBuildingPCView : ViewBase<RequiredBuildingVM>
{
	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_Icon;

	public void SetData(BlueprintColonyProject project, int index)
	{
	}

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}
}

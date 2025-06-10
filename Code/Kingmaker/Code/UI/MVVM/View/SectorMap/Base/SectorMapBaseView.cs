using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.VM.SectorMap;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.Base;

public class SectorMapBaseView : CommonStaticComponentView<SectorMapVM>
{
	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}

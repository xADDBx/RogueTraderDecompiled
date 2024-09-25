using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Colonization;

public class SpaceResourcePCView : ViewBase<SpaceResourceVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SpaceResourceValue;

	[SerializeField]
	private Image m_TooltipArea;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.SpaceResource.Subscribe(delegate(string currentSpace)
		{
			m_SpaceResourceValue.text = currentSpace.ToString();
		}));
		AddDisposable(m_TooltipArea.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.ColonizationTexts.ResourceStrings[2].Name, UIStrings.Instance.ColonizationTexts.ResourceStrings[2].Description)));
	}

	public void SetData(PlanetEntity planet)
	{
		base.ViewModel.CurrentPlanet.Value = planet;
	}

	public void UpdateData(PlanetEntity planet)
	{
		base.ViewModel.CurrentPlanet.Value = planet;
	}

	public void SetData(Colony colony)
	{
		base.ViewModel.CurrentColony.Value = colony;
	}

	public void UpdateData(Colony colony)
	{
		base.ViewModel.CurrentColony.Value = colony;
	}

	protected override void DestroyViewImplementation()
	{
	}
}

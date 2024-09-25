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

public class PeopleResourcePCView : ViewBase<PeopleResourceVM>
{
	[SerializeField]
	private TextMeshProUGUI m_PeopleResourceValue;

	[SerializeField]
	private Image m_TooltipArea;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.PeopleResource.Subscribe(delegate(string currentPeople)
		{
			m_PeopleResourceValue.text = currentPeople.ToString();
		}));
		AddDisposable(m_TooltipArea.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.ColonizationTexts.ResourceStrings[0].Name, UIStrings.Instance.ColonizationTexts.ResourceStrings[0].Description)));
	}

	public void SetData(PlanetEntity planet)
	{
	}

	public void SetData(Colony colony)
	{
	}

	public void UpdateData(PlanetEntity planet)
	{
	}

	public void UpdateData(Colony colony)
	{
	}

	protected override void DestroyViewImplementation()
	{
	}
}

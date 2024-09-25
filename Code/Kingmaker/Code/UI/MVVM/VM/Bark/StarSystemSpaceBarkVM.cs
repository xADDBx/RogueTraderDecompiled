using Kingmaker.EntitySystem.Entities;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class StarSystemSpaceBarkVM : BaseSpaceBarkVM
{
	public readonly ReactiveProperty<string> EncyclopediaLink = new ReactiveProperty<string>(string.Empty);

	public StarSystemSpaceBarkVM(BaseUnitEntity baseUnitEntity, string text, string link)
		: base(baseUnitEntity, text)
	{
		EncyclopediaLink.Value = link;
	}
}

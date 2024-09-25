using Kingmaker.EntitySystem.Entities;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class SpaceBarkVM : BaseSpaceBarkVM
{
	public readonly ReactiveProperty<string> UnitName = new ReactiveProperty<string>(string.Empty);

	public SpaceBarkVM(BaseUnitEntity baseUnitEntity, string text)
		: base(baseUnitEntity, text)
	{
		UnitName.Value = baseUnitEntity.Name;
	}
}

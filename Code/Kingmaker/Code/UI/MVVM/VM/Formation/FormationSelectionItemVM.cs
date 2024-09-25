using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.Formation;

public class FormationSelectionItemVM : SelectionGroupEntityVM
{
	public readonly int FormationIndex;

	public FormationSelectionItemVM(int formationIndex)
		: base(allowSwitchOff: false)
	{
		FormationIndex = formationIndex;
	}

	protected override void DoSelectMe()
	{
	}
}

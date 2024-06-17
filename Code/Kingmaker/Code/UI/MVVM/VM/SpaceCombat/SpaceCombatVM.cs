using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.ExitBattlePopup;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Kingmaker.UI.MVVM.VM.CircleArc;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

public class SpaceCombatVM : CommonStaticComponentVM
{
	public readonly ShipWeaponsPanelVM ShipWeaponsPanelVM;

	public readonly ShipPostsPanelVM ShipPostsPanelVM;

	public readonly SpaceCombatServicePanelVM SpaceCombatServicePanelVM;

	public readonly ExitBattlePopupVM ExitBattlePopupVM;

	public readonly SpaceCombatCircleArcsVM SpaceCombatCircleArcsVM;

	public readonly StarSystemSpaceBarksHolderVM SpaceCombatBarksHolderVM;

	private readonly SpaceCombatMovementActionHolder m_SpaceCombatMovementActionHolder;

	public SpaceCombatVM()
	{
		AddDisposable(ShipWeaponsPanelVM = new ShipWeaponsPanelVM());
		AddDisposable(ShipPostsPanelVM = new ShipPostsPanelVM());
		AddDisposable(ExitBattlePopupVM = new ExitBattlePopupVM());
		AddDisposable(SpaceCombatCircleArcsVM = new SpaceCombatCircleArcsVM());
		AddDisposable(SpaceCombatBarksHolderVM = new StarSystemSpaceBarksHolderVM());
		SpaceCombatMovementActionHolder spaceCombatMovementActionHolder = new SpaceCombatMovementActionHolder();
		AddDisposable(spaceCombatMovementActionHolder);
		List<ISpaceCombatActionsHolder> list = new List<ISpaceCombatActionsHolder> { spaceCombatMovementActionHolder, ShipWeaponsPanelVM.AbilitiesGroup };
		list.AddRange(ShipWeaponsPanelVM.WeaponAbilitiesGroups.Values);
		list.AddRange(ShipPostsPanelVM.Posts.Select((ShipPostVM postVm) => postVm.AbilitiesGroup));
		AddDisposable(SpaceCombatServicePanelVM = new SpaceCombatServicePanelVM(list));
	}

	protected override void DisposeImplementation()
	{
	}
}

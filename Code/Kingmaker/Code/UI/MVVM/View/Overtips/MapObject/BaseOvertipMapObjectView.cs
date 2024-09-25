using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public abstract class BaseOvertipMapObjectView : BaseOvertipView<OvertipMapObjectVM>
{
	protected bool CheckCanBeVisible
	{
		get
		{
			if (base.ViewModel.MapObjectEntity.IsInGame && base.ViewModel.MapObjectEntity.IsRevealed && base.ViewModel.MapObjectEntity.IsAwarenessCheckPassed && !base.ViewModel.MapObjectEntity.IsInFogOfWar && base.ViewModel.MapObjectEntity.IsInCameraFrustum && base.ViewModel.MapObjectEntity.IsVisibleForPlayer && (!base.ViewModel.IsCutscene || base.ViewModel.IsBarkActive.Value) && !base.ViewModel.IsInDialog && !base.ViewModel.ForceHideInCombat.Value)
			{
				if (!base.ViewModel.IsEnabled.Value || base.ViewModel.NotAvailable)
				{
					return base.ViewModel.IsBarkActive.Value;
				}
				return true;
			}
			return false;
		}
	}
}

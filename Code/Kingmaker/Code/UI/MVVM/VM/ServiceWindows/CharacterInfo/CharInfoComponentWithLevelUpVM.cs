using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;

public class CharInfoComponentWithLevelUpVM : CharInfoComponentVM, ILevelUpManagerUIHandler, ISubscriber, INetLobbyPlayersHandler
{
	public readonly ReactiveProperty<BaseUnitEntity> PreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	protected readonly IReadOnlyReactiveProperty<LevelUpManager> LevelUpManager;

	public readonly ReactiveCommand<bool> CheckCoopControls = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	protected CharInfoComponentWithLevelUpVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit)
	{
		LevelUpManager = levelUpManager;
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(LevelUpManager?.Subscribe(UpdatePreviewUnit));
	}

	private void UpdatePreviewUnit(LevelUpManager levelUpManager)
	{
		if (levelUpManager != null && Unit.Value == levelUpManager.TargetUnit)
		{
			PreviewUnit.Value = levelUpManager.PreviewUnit;
		}
		else
		{
			PreviewUnit.Value = Unit.Value;
		}
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdatePreviewUnit(LevelUpManager?.Value);
	}

	protected override void DisposeImplementation()
	{
	}

	public new void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public new void HandleDestroyLevelUpManager()
	{
	}

	public new virtual void HandleUISelectCareerPath()
	{
		UpdatePreviewUnit(LevelUpManager?.Value);
	}

	public new virtual void HandleUICommitChanges()
	{
	}

	public new virtual void HandleUISelectionChanged()
	{
		UpdatePreviewUnit(LevelUpManager?.Value);
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		CheckCoopControls.Execute(UINetUtility.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}

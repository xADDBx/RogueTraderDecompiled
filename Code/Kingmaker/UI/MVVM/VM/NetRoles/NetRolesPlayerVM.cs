using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;
using Kingmaker.UI.MVVM.VM.NetLobby;

namespace Kingmaker.UI.MVVM.VM.NetRoles;

public class NetRolesPlayerVM : NetLobbyPlayerVM
{
	public readonly List<NetRolesPlayerCharacterVM> Players = new List<NetRolesPlayerCharacterVM>();

	private static List<BaseUnitEntity> Characters => Game.Instance.SelectionCharacter.ActualGroup.Where((BaseUnitEntity u) => !u.IsPet).ToList();

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Players.ForEach(delegate(NetRolesPlayerCharacterVM p)
		{
			p.Dispose();
		});
		Players.Clear();
	}

	public override void SetPlayer(PhotonActorNumber player, string userId, bool isActive)
	{
		base.SetPlayer(player, userId, isActive);
		Game instance = Game.Instance;
		if (instance != null)
		{
			BlueprintArea currentlyLoadedArea = instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea != null && currentlyLoadedArea.IsShipArea)
			{
				BaseUnitEntity unit = Game.Instance.Player?.MainCharacterEntity;
				Players.Add(new NetRolesPlayerCharacterVM(UnitReference.FromIAbstractUnitEntity(unit), player));
				return;
			}
		}
		for (int i = 0; i < 6; i++)
		{
			BaseUnitEntity unit2 = ((Characters.Count > i) ? Characters[i] : null);
			Players.Add(new NetRolesPlayerCharacterVM(UnitReference.FromIAbstractUnitEntity(unit2), player));
		}
	}
}

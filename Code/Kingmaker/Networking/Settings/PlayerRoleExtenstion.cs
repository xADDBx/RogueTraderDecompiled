using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Networking.Settings;

public static class PlayerRoleExtenstion
{
	public static bool Can(this PlayerRole playerRole, Entity entity)
	{
		return playerRole.Can(entity, NetworkingManager.LocalNetPlayer);
	}

	public static bool Can(this PlayerRole playerRole, Entity entity, NetPlayer player)
	{
		if (entity is StarshipEntity)
		{
			return playerRole.Can(Game.Instance.Player.MainCharacter.Id, player);
		}
		if (entity is BaseUnitEntity { IsPet: not false } baseUnitEntity)
		{
			return playerRole.Can(baseUnitEntity.Master.UniqueId, player);
		}
		return playerRole.Can(entity.UniqueId, player);
	}
}

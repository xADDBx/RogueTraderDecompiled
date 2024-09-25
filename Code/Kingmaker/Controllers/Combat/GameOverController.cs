using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.QA;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;

namespace Kingmaker.Controllers.Combat;

public class GameOverController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Megatron.IsActive)
		{
			return;
		}
		Player.GameOverReasonType? gameOverReasonType = null;
		int num;
		int num2;
		if (!(Game.Instance.CurrentMode == GameModeType.SpaceCombat) && !(Game.Instance.CurrentMode == GameModeType.StarSystem))
		{
			num = ((Game.Instance.CurrentMode == GameModeType.GlobalMap) ? 1 : 0);
			if (num == 0)
			{
				num2 = (Game.Instance.Player.PartyAndPets.All((BaseUnitEntity u) => !u.LifeState.IsConscious) ? 1 : 0);
				goto IL_008c;
			}
		}
		else
		{
			num = 1;
		}
		num2 = 0;
		goto IL_008c;
		IL_008c:
		bool flag = (byte)num2 != 0;
		bool flag2 = num != 0 && Game.Instance.Player.AllStarships.Where(delegate(BaseUnitEntity x)
		{
			PartStarshipNavigation starshipNavigationOptional = x.GetStarshipNavigationOptional();
			return starshipNavigationOptional != null && !starshipNavigationOptional.IsSoftUnit;
		}).All((BaseUnitEntity u) => !u.LifeState.IsConscious);
		if (flag || flag2)
		{
			gameOverReasonType = Player.GameOverReasonType.PartyIsDefeated;
		}
		else if (Game.Instance.Player.GameOverReason.HasValue)
		{
			gameOverReasonType = Game.Instance.Player.GameOverReason.Value;
		}
		if (gameOverReasonType.HasValue)
		{
			Game.Instance.Player.GameOver(gameOverReasonType.Value);
		}
	}
}

using Kingmaker.EntitySystem.Entities;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class ShipNameAndPortraitVM : CharInfoComponentVM
{
	public Sprite StarShipImage => Game.Instance.Player.PlayerShip.Blueprint.PlayerShipBigPicture;

	public string StarShipName => Game.Instance.Player.PlayerShip.Blueprint.PlayerShipName;

	public string StarShipDescription => Game.Instance.Player.PlayerShip.Blueprint.PlayerShipDescription;

	public ShipNameAndPortraitVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}
}

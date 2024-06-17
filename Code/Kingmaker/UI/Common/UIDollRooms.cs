using JetBrains.Annotations;
using Kingmaker.Code.UI.DollRoom;
using Kingmaker.UI.DollRoom;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Common;

public class UIDollRooms : MonoBehaviour
{
	[Header("Doll Rooms")]
	[FormerlySerializedAs("DollRoom")]
	public CharacterDollRoom CharacterDollRoom;

	public CharGenDollRoom CharGenDollRoom;

	public ShipDollRoom ShipDollRoom;

	public CharGenShipDollRoom CharGenShipDollRoom;

	public PlanetDollRoom PlanetDollRoom;

	private bool m_IsInit;

	public static UIDollRooms Instance { get; private set; }

	public RectTransform RectTransform { get; private set; }

	[UsedImplicitly]
	private void Awake()
	{
		Initialize();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void Initialize()
	{
		Instance = this;
		RectTransform = base.transform as RectTransform;
	}
}

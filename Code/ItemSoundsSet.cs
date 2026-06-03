using System;
using Kingmaker.Visual.Sound;

[Serializable]
public class ItemSoundsSet
{
	public ItemSoundType Type;

	[AkEventReference]
	public string InventoryPutSound;

	[AkEventReference]
	public string InventoryTakeSound;

	[AkEventReference]
	public string InventoryEquipSound;
}

using System;
using JetBrains.Annotations;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.CharGen;

[Serializable]
public class CustomizationOptions
{
	[NotNull]
	public EquipmentEntityLink[] Heads = new EquipmentEntityLink[0];

	[NotNull]
	[Tooltip("Must match heads 1:1")]
	public EquipmentEntityLink[] Eyebrows = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Hair = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Beards = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Horns = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Ports = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] NavigatorMutations = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] TailSkinColors = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Scars = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Tattoos = new EquipmentEntityLink[0];

	[CanBeNull]
	public EquipmentEntityLink GetTail(int skinRampIndex)
	{
		if (TailSkinColors.Length == 0)
		{
			return null;
		}
		if (skinRampIndex < 0 || skinRampIndex >= TailSkinColors.Length)
		{
			PFLog.Default.Warning("");
			skinRampIndex = 0;
		}
		return TailSkinColors[skinRampIndex];
	}
}

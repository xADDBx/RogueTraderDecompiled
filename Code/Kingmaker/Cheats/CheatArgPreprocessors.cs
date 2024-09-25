using System;
using System.Globalization;
using System.Linq;
using Core.Cheats;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Cheats;

internal static class CheatArgPreprocessors
{
	[CheatArgPreprocessor]
	internal static (bool success, string result) SelectedUnits(string value, string targetType)
	{
		if (string.Compare(value, "@selectedUnits", StringComparison.InvariantCultureIgnoreCase) != 0)
		{
			return (success: false, result: "");
		}
		if (targetType != typeof(string).FullName)
		{
			throw new Exception("Cant get selected units of type " + targetType);
		}
		return (success: true, result: string.Join(" ", Game.Instance.SelectionCharacter.SelectedUnits.Select((BaseUnitEntity x) => x.UniqueId)));
	}

	[CheatArgPreprocessor]
	internal static (bool success, string result) Mouseover(string value, string targetType)
	{
		if (string.Compare(value, "@mouseover", StringComparison.InvariantCultureIgnoreCase) != 0)
		{
			return (success: false, result: "");
		}
		if (targetType == typeof(MechanicEntity).FullName)
		{
			return (success: true, result: Utilities.GetMechanicEntityUnderMouse()?.UniqueId ?? "");
		}
		if (targetType == typeof(BaseUnitEntity).FullName)
		{
			return (success: true, result: Utilities.GetUnitUnderMouse()?.UniqueId ?? "");
		}
		throw new Exception("Cant get unit of type " + targetType + " under mouse");
	}

	[CheatArgPreprocessor]
	internal static (bool success, string result) Cursor(string value, string targetType)
	{
		if (string.Compare(value, "@cursor", StringComparison.InvariantCultureIgnoreCase) != 0)
		{
			return (success: false, result: "");
		}
		Vector3 worldPosition = Game.Instance.ClickEventsController.WorldPosition;
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return (success: true, result: worldPosition.x.ToString("G9", invariantCulture) + ";" + worldPosition.y.ToString("G9", invariantCulture) + ";" + worldPosition.z.ToString("G9", invariantCulture));
	}
}

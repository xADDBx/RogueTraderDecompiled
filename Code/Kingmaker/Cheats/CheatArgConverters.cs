using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Cheats;

public static class CheatArgConverters
{
	[CheatArgConverter]
	internal static (bool success, object result) UnitConverter(string value, Type targetType)
	{
		if (!targetType.IsAssignableFrom(typeof(BaseUnitEntity)))
		{
			return (success: false, result: null);
		}
		if (string.IsNullOrWhiteSpace(value))
		{
			return (success: true, result: null);
		}
		IEnumerable<AbstractUnitEntity> source = Game.Instance.State.AllUnits.Where((AbstractUnitEntity v) => v.UniqueId == value);
		if (source.Any())
		{
			if (!source.Skip(1).Any())
			{
				return (success: true, result: source.First());
			}
			throw new ArgumentException("Ambiguous unique id " + value);
		}
		IEnumerable<AbstractUnitEntity> source2 = Game.Instance.State.AllUnits.Where((AbstractUnitEntity v) => v.Name == value);
		if (source2.Any())
		{
			if (!source2.Skip(1).Any())
			{
				return (success: true, result: source2.First());
			}
			throw new ArgumentException("Ambiguous unit name " + value);
		}
		return (success: false, result: null);
	}

	[CheatArgConverter]
	internal static (bool success, object result) MechanicEntityConverter(string value, Type targetType)
	{
		if (!targetType.IsAssignableFrom(typeof(MechanicEntity)))
		{
			return (success: false, result: null);
		}
		if (string.IsNullOrWhiteSpace(value))
		{
			return (success: true, result: null);
		}
		IEnumerable<MechanicEntity> source = Game.Instance.State.MechanicEntities.Where((MechanicEntity v) => v.UniqueId == value);
		if (source.Any())
		{
			if (!source.Skip(1).Any())
			{
				return (success: true, result: source.First());
			}
			throw new ArgumentException("Ambiguous unique id " + value);
		}
		IEnumerable<MechanicEntity> source2 = Game.Instance.State.MechanicEntities.Where((MechanicEntity v) => v.Name == value);
		if (source2.Any())
		{
			if (!source2.Skip(1).Any())
			{
				return (success: true, result: source2.First());
			}
			throw new ArgumentException("Ambiguous unit name " + value);
		}
		return (success: false, result: null);
	}

	[CheatArgConverter]
	internal static (bool success, object result) Vector3Converter(string value, Type targetType)
	{
		if (!targetType.IsAssignableFrom(typeof(Vector3)))
		{
			return (success: false, result: null);
		}
		string[] array = value.Split(';');
		if (array.Length != 3)
		{
			throw new Exception("Cant parse " + value + " as vector, need 3 comma-separated parts");
		}
		float x = float.Parse(array[0], CultureInfo.InvariantCulture);
		float y = float.Parse(array[1], CultureInfo.InvariantCulture);
		float z = float.Parse(array[2], CultureInfo.InvariantCulture);
		return (success: true, result: new Vector3(x, y, z));
	}
}

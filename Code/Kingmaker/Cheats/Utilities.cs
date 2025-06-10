using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.BundlesLoading;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechanics;
using UnityEngine;

namespace Kingmaker.Cheats;

public class Utilities
{
	private static BlueprintList s_BlueprintList;

	private static BlueprintList GetAllBlueprints()
	{
		if (s_BlueprintList == null)
		{
			s_BlueprintList = JsonUtility.FromJson<BlueprintList>(File.ReadAllText(BundlesLoadService.BundlesPath("cheatdata.json")));
		}
		return s_BlueprintList;
	}

	public static IEnumerable<T> GetScriptableObjects<T>() where T : BlueprintScriptableObject
	{
		return GetBlueprintGuids<T>().Select(ResourcesLibrary.TryGetBlueprint<T>);
	}

	public static IEnumerable<string> GetBlueprintNames<T>() where T : BlueprintScriptableObject
	{
		_ = Application.isEditor;
		return from e in GetAllBlueprints().Entries
			where e.Type != null
			where e.Type == typeof(T) || e.Type.IsSubclassOf(typeof(T))
			select e.Name;
	}

	public static IEnumerable<string> GetBlueprintGuids<T>() where T : BlueprintScriptableObject
	{
		_ = Application.isEditor;
		return from e in GetAllBlueprints().Entries
			where e.Type != null
			where e.Type == typeof(T) || e.Type.IsSubclassOf(typeof(T))
			select e.Guid;
	}

	public static T GetBlueprintByName<T>(string name) where T : BlueprintScriptableObject
	{
		BlueprintList.Entry entry = (from e in GetAllBlueprints().Entries
			where e.Type != null
			where e.Type == typeof(T) || e.Type.IsSubclassOf(typeof(T))
			select e).FirstOrDefault((BlueprintList.Entry e) => e.Guid == name || e.Name == name);
		if (entry != null)
		{
			return ResourcesLibrary.TryGetBlueprint(entry.Guid) as T;
		}
		return null;
	}

	public static BlueprintAreaEnterPoint GetEnterPoint(BlueprintArea area)
	{
		return GetScriptableObjects<BlueprintAreaEnterPoint>().FirstOrDefault((BlueprintAreaEnterPoint scriptableObject) => scriptableObject.Area.Equals(area));
	}

	public static int? GetParamInt(string parameters, int index, string message)
	{
		string paramString = GetParamString(parameters, index, null);
		if (string.IsNullOrEmpty(paramString) && message != null)
		{
			PFLog.SmartConsole.Log(message);
		}
		try
		{
			return string.IsNullOrEmpty(paramString) ? null : new int?(Convert.ToInt32(paramString));
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static bool? GetParamBool(string parameters, int index, string message)
	{
		string paramString = GetParamString(parameters, index, null);
		if (string.IsNullOrEmpty(paramString) && message != null)
		{
			PFLog.SmartConsole.Log(message);
		}
		try
		{
			return string.IsNullOrEmpty(paramString) ? null : new bool?(Convert.ToBoolean(paramString));
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static float? GetParamFloat(string parameters, int index, string message)
	{
		string paramString = GetParamString(parameters, index, message);
		try
		{
			if (float.TryParse(paramString, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result))
			{
				return result;
			}
		}
		catch (Exception)
		{
			PFLog.SmartConsole.Log(message);
		}
		PFLog.SmartConsole.Log("Failed to parse [" + paramString + "] as float");
		return null;
	}

	public static string FormatPositionAndRotation(Transform transform)
	{
		Vector3 position = transform.position;
		Vector3 eulerAngles = transform.eulerAngles;
		return $"position=x:{position.x};y:{position.y};z:{position.z}|rotation=x:{eulerAngles.x};y:{eulerAngles.y};z:{eulerAngles.z}";
	}

	public static bool TryGetFormatedPositionAndRotation(string parameters, out Vector3 position, out Vector3 eulerAngles)
	{
		string[] array = parameters.Split("|");
		if (array.Length != 2)
		{
			position = Vector3.zero;
			eulerAngles = Vector3.zero;
			return false;
		}
		string formatedVector = array[0];
		string formatedVector2 = array[1];
		position = GetVector3FromFormatedString(formatedVector);
		eulerAngles = GetVector3FromFormatedString(formatedVector2);
		return true;
	}

	private static Vector3 GetVector3FromFormatedString(string formatedVector)
	{
		Vector3 result = default(Vector3);
		string[] array = formatedVector.Split("=")[1].Split(";");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(":");
			float num = float.Parse(array2[1]);
			switch (array2[0])
			{
			case "x":
				result.x = num;
				break;
			case "y":
				result.y = num;
				break;
			case "z":
				result.z = num;
				break;
			}
		}
		return result;
	}

	public static TEnum? GetParamEnum<TEnum>(string parameters, int index, string message) where TEnum : struct, Enum
	{
		string paramString = GetParamString(parameters, index, message);
		try
		{
			if (Enum.TryParse<TEnum>(paramString, ignoreCase: false, out var result))
			{
				return result;
			}
		}
		catch (Exception)
		{
			PFLog.SmartConsole.Log(message);
		}
		PFLog.SmartConsole.Log($"Failed to parse [{paramString}] as {typeof(TEnum)}");
		return null;
	}

	public static bool TryGetStringParam(string parameters, int index, out string str)
	{
		str = null;
		string[] arguments = GetArguments(parameters);
		if (arguments.Length > index)
		{
			str = arguments[index];
		}
		return str != null;
	}

	public static bool TryGetFloatParam(string parameters, int index, out float flt)
	{
		flt = 0f;
		if (TryGetStringParam(parameters, index, out var str))
		{
			return float.TryParse(str, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out flt);
		}
		return false;
	}

	public static bool TryGetParamBlueprint<T>(string parameters, int index, out T blueprint) where T : BlueprintScriptableObject
	{
		blueprint = null;
		if (TryGetStringParam(parameters, index, out var str))
		{
			blueprint = GetBlueprint<T>(str);
			return blueprint;
		}
		return false;
	}

	public static string GetParamString(string parameters, int index, string errorMessage)
	{
		string[] arguments = GetArguments(parameters);
		if (index < arguments.Length)
		{
			return arguments[index].Replace('\\', '/');
		}
		if (errorMessage != null)
		{
			PFLog.SmartConsole.Log(errorMessage);
		}
		return null;
	}

	public static string GetEnumValues<TEnum>()
	{
		return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Aggregate("", delegate(string current, TEnum weatherType)
		{
			TEnum val = weatherType;
			return current + ", " + val;
		});
	}

	public static string[] GetArguments(string parameters)
	{
		List<string> list = new List<string>();
		bool flag = false;
		string[] array = parameters.Split(' ');
		foreach (string text in array)
		{
			if (flag)
			{
				list[list.Count - 1] = list[list.Count - 1] + " " + text.Replace("\"", "");
			}
			else
			{
				list.Add(text.Replace("\"", ""));
				flag = text.StartsWith("\"");
			}
			if (text.EndsWith("\""))
			{
				flag = false;
			}
		}
		return list.ToArray();
	}

	internal static BaseUnitEntity GetUnitForCheat()
	{
		BaseUnitEntity baseUnitEntity = GetUnitUnderMouse();
		if (baseUnitEntity == null)
		{
			baseUnitEntity = ((Game.Instance.SelectionCharacter.SelectedUnits.Count <= 0) ? GameHelper.GetPlayerCharacter() : Game.Instance.SelectionCharacter.SelectedUnits[0]);
		}
		return baseUnitEntity;
	}

	internal static BaseUnitEntity GetUnitUnderMouse()
	{
		if (!TryGetEntityUnderMouse<UnitEntityView>(out var entity))
		{
			return null;
		}
		return entity.Data;
	}

	internal static MechanicEntity GetMechanicEntityUnderMouse()
	{
		if (!TryGetEntityUnderMouse<MechanicEntityView>(out var entity))
		{
			return null;
		}
		return entity.Data;
	}

	internal static bool TryGetEntityUnderMouse<T>(out T entity) where T : MonoBehaviour
	{
		entity = null;
		Camera camera = Game.GetCamera();
		if (camera == null)
		{
			return false;
		}
		Vector2 cursorPosition = Game.Instance.CursorController.CursorPosition;
		RaycastHit[] array = (Game.Instance.IsModeActive(GameModeType.SpaceCombat) ? Physics.RaycastAll(camera.ScreenPointToRay(cursorPosition)) : Physics.RaycastAll(camera.ScreenPointToRay(cursorPosition), camera.farClipPlane, 70014209));
		foreach (RaycastHit raycastHit in array)
		{
			GameObject gameObject = raycastHit.collider.gameObject;
			while (!gameObject.GetComponent<T>() && (bool)gameObject.transform.parent)
			{
				gameObject = gameObject.transform.parent.gameObject;
			}
			entity = gameObject.GetComponent<T>();
			if (entity != null)
			{
				break;
			}
		}
		return entity != null;
	}

	public static string GetDesigner(BlueprintArea area)
	{
		if (area == null)
		{
			PFLog.Default.Log("Cannot get area designer for null area.");
			return "";
		}
		string enumDescription = GetEnumDescription(area.Author);
		if (enumDescription == null)
		{
			PFLog.Default.Log("Cannot get area designer for area [{0}]", area.Name);
			return "";
		}
		return enumDescription;
	}

	public static string GetEnumDescription(Enum e)
	{
		if (e == null)
		{
			return null;
		}
		Type type = e.GetType();
		string name = Enum.GetName(type, e);
		if (name != null)
		{
			FieldInfo field = type.GetField(name);
			if (field != null && Attribute.GetCustomAttributes(field, typeof(DescriptionAttribute)).FirstOrDefault((Attribute a) => a is DescriptionAttribute) is DescriptionAttribute descriptionAttribute)
			{
				return descriptionAttribute.Description;
			}
		}
		return null;
	}

	public static T GetBlueprintByGuid<T>(string guid) where T : BlueprintScriptableObject
	{
		return ResourcesLibrary.TryGetBlueprint<T>(guid);
	}

	public static T GetBlueprintByPath<T>(string path) where T : BlueprintScriptableObject
	{
		return null;
	}

	public static string GetBlueprintPath(BlueprintScriptableObject blueprint)
	{
		if (blueprint == null)
		{
			return "null";
		}
		return GetBlueprintName(blueprint);
	}

	public static string GetBlueprintName(BlueprintScriptableObject blueprint)
	{
		if (blueprint == null)
		{
			return "null";
		}
		return blueprint.name;
	}

	[CheatArgConverter]
	public static (bool success, object result) BlueprintConverter(string path, Type targetType)
	{
		if (!typeof(SimpleBlueprint).IsAssignableFrom(targetType))
		{
			return (success: false, result: null);
		}
		MethodInfo obj = MethodBase.GetCurrentMethod().DeclaringType?.GetMethod("GetBlueprint", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (obj == null)
		{
			throw new Exception("Internal exception: method GetBlueprint not found");
		}
		object obj2 = obj.MakeGenericMethod(targetType).Invoke(null, new object[1] { path });
		if (obj2 == null)
		{
			throw new ArgumentException("Cant convert string " + path + " to blueprint of type " + targetType.FullName);
		}
		if (!targetType.IsInstanceOfType(obj2))
		{
			throw new ArgumentException("Blueprint " + path + " is incompatible with required type " + targetType.FullName);
		}
		return (success: true, result: obj2);
	}

	public static T GetBlueprint<T>(string value) where T : BlueprintScriptableObject
	{
		if (value == null)
		{
			PFLog.SmartConsole.Log("Can't get 'null' blueprint");
			return null;
		}
		if (Regex.IsMatch(value, "[a-z0-9]{32}"))
		{
			T blueprintByGuid = GetBlueprintByGuid<T>(value);
			if (blueprintByGuid == null)
			{
				PFLog.SmartConsole.Log("Can't get bluepring with guid '" + value + "'");
			}
			return blueprintByGuid;
		}
		T blueprintByName = GetBlueprintByName<T>(value);
		if (blueprintByName == null)
		{
			PFLog.SmartConsole.Log("Can't get blueprint with name '" + value + "'");
		}
		return blueprintByName;
	}

	public static int GetTotalChallengeRating(List<BlueprintUnit> units)
	{
		return ExperienceHelper.GetCR(GetTotalCrPoints(units));
	}

	public static int GetTotalCrPoints(IEnumerable<BlueprintUnit> units, List<string> lines = null, EncounterType? forceEncounterType = null)
	{
		int num = 0;
		foreach (BlueprintUnit unit in units)
		{
			Experience component = unit.GetComponent<Experience>();
			if (!component)
			{
				lines?.Add("No CR for: " + unit.name);
				continue;
			}
			int cRPoints = ExperienceHelper.GetCRPoints(component.CR, component.Modifier, component.Count);
			num += cRPoints;
			if (component.Encounter != 0)
			{
				lines?.Add($"{unit.name} is {component.Encounter}, CR {component.CR} CRP {cRPoints} (XP {ExperienceHelper.GetXp(component.Encounter, component.CR, component.Modifier, component.Count)})");
			}
		}
		return num;
	}

	public static void CreateGameHistoryLog()
	{
		using AreaDataStashFileAccessor areaDataStashFileAccessor = AreaDataStash.AccessFile("history");
		File.Copy(areaDataStashFileAccessor.Path, Path.Combine(ApplicationPaths.persistentDataPath, "game-history.txt"), overwrite: true);
	}

	public static string BetaStatus()
	{
		return "";
	}

	public static Vector4 QuatToV4(Quaternion quat)
	{
		return new Vector4(quat.x, quat.y, quat.z, quat.w);
	}

	public static Quaternion V4ToQuat(Vector4 v4)
	{
		return new Quaternion(v4.x, v4.y, v4.z, v4.w);
	}
}

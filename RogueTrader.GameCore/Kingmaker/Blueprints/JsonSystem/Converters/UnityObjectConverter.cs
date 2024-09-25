using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owlcat.Runtime.Core.Utility;
using RogueTrader.SharedTypes;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class UnityObjectConverter : JsonConverter
{
	private static BlueprintReferencedAssets s_AssetList;

	public static readonly List<BlueprintReferencedAssets> ModificationAssetLists = new List<BlueprintReferencedAssets>();

	public static BlueprintReferencedAssets AssetList
	{
		get
		{
			return s_AssetList;
		}
		set
		{
			s_AssetList = value;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		UnityEngine.Object @object = value as UnityEngine.Object;
		if (!@object)
		{
			writer.WriteNull();
			return;
		}
		string text = null;
		long num = 0L;
		(string, long)? tuple = ObjectExtensions.Or(AssetList, null)?.GetAssetId(@object);
		if (!tuple.HasValue)
		{
			writer.WriteNull();
			return;
		}
		string item = tuple.Value.Item1;
		long item2 = tuple.Value.Item2;
		text = item;
		num = item2;
		writer.WriteStartObject();
		writer.WritePropertyName("guid");
		writer.WriteValue(text);
		writer.WritePropertyName("fileid");
		writer.WriteValue(num);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject jObject = JObject.Load(reader);
		string text = (string?)jObject["guid"];
		long? num = (long?)jObject["fileid"];
		if (text == null || !num.HasValue)
		{
			return null;
		}
		if ((bool)AssetList)
		{
			UnityEngine.Object @object = AssetList.Get(text, num.Value);
			if (@object != null)
			{
				return @object;
			}
		}
		foreach (BlueprintReferencedAssets modificationAssetList in ModificationAssetLists)
		{
			UnityEngine.Object object2 = modificationAssetList.Get(text, num.Value);
			if (object2 != null)
			{
				return object2;
			}
		}
		return null;
	}

	public override bool CanConvert(Type objectType)
	{
		if (!objectType.IsSubclassOf(typeof(UnityEngine.Object)) || typeof(IPrototypeableObjectBase).IsAssignableFrom(objectType) || typeof(IElementConvertable).IsAssignableFrom(objectType))
		{
			return objectType == typeof(UnityEngine.Object);
		}
		return true;
	}
}

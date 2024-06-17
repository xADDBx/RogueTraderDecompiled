using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class GradientConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Gradient gradient = (Gradient)value;
		JObject jObject = new JObject();
		JArray jArray = new JArray();
		GradientColorKey[] colorKeys = gradient.colorKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			GradientColorKey gradientColorKey = colorKeys[i];
			JObject jObject2 = new JObject();
			jObject2["time"] = gradientColorKey.time;
			jObject2["r"] = gradientColorKey.color.r;
			jObject2["g"] = gradientColorKey.color.g;
			jObject2["b"] = gradientColorKey.color.b;
			jArray.Add(jObject2);
		}
		jObject["colors"] = jArray;
		jArray = new JArray();
		GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
		for (int i = 0; i < alphaKeys.Length; i++)
		{
			GradientAlphaKey gradientAlphaKey = alphaKeys[i];
			JObject jObject3 = new JObject();
			jObject3["time"] = gradientAlphaKey.time;
			jObject3["a"] = gradientAlphaKey.alpha;
			jArray.Add(jObject3);
		}
		jObject["alpha"] = jArray;
		jObject.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Gradient gradient = new Gradient();
		JArray jArray = (JArray)jObject["colors"];
		GradientColorKey[] array = new GradientColorKey[jArray.Count];
		for (int i = 0; i < array.Length; i++)
		{
			JObject jObject2 = (JObject)jArray[i];
			array[i] = new GradientColorKey(new Color((float)jObject2["r"], (float)jObject2["g"], (float)jObject2["b"]), (float)jObject2["time"]);
		}
		JArray jArray2 = (JArray)jObject["alpha"];
		GradientAlphaKey[] array2 = new GradientAlphaKey[jArray2.Count];
		for (int j = 0; j < array2.Length; j++)
		{
			JObject jObject3 = (JObject)jArray2[j];
			array2[j] = new GradientAlphaKey((float)jObject3["a"], (float)jObject3["time"]);
		}
		gradient.SetKeys(array, array2);
		return gradient;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Gradient);
	}
}

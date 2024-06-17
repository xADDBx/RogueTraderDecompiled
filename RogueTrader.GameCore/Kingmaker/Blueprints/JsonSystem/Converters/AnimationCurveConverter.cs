using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class AnimationCurveConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		AnimationCurve animationCurve = (AnimationCurve)value;
		JArray jArray = new JArray();
		Keyframe[] keys = animationCurve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			JObject jObject = new JObject();
			jObject["time"] = keyframe.time;
			jObject["value"] = keyframe.value;
			jObject["inTangent"] = keyframe.inTangent;
			jObject["inWeight"] = keyframe.inWeight;
			jObject["outTangent"] = keyframe.outTangent;
			jObject["outWeight"] = keyframe.outWeight;
			jObject["weightedMode"] = (int)keyframe.weightedMode;
			jObject["postWrapMode"] = (int)animationCurve.postWrapMode;
			jObject["preWrapMode"] = (int)animationCurve.preWrapMode;
			jArray.Add(jObject);
		}
		jArray.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JArray jArray = JArray.Load(reader);
		AnimationCurve animationCurve = new AnimationCurve();
		foreach (JObject item in jArray)
		{
			Keyframe key = new Keyframe((float)item["time"], (float)item["value"], (float)item["inTangent"], (float)item["outTangent"], (float)item["inWeight"], (float)item["outWeight"]);
			key.weightedMode = (WeightedMode)(int)item["weightedMode"];
			animationCurve.AddKey(key);
			if (item["postWrapMode"] != null)
			{
				animationCurve.postWrapMode = (WrapMode)(int)item["postWrapMode"];
			}
			if (item["preWrapMode"] != null)
			{
				animationCurve.preWrapMode = (WrapMode)(int)item["preWrapMode"];
			}
		}
		return animationCurve;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(AnimationCurve);
	}
}

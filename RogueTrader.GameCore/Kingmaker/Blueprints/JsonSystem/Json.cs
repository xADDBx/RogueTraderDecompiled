using System.Threading;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.JsonSystem;

public static class Json
{
	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new FieldsContractResolver(),
		TypeNameHandling = TypeNameHandling.Auto,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		DefaultValueHandling = DefaultValueHandling.Include,
		ReferenceLoopHandling = ReferenceLoopHandling.Error,
		Formatting = Formatting.Indented,
		SerializationBinder = new GuidClassBinder(),
		Converters = 
		{
			(JsonConverter)new Color32Converter(),
			(JsonConverter)new StringOrIntEnumConverter(),
			(JsonConverter)new SharedStringConverter(),
			(JsonConverter)new UnityObjectConverter(),
			(JsonConverter)new BlueprintReferenceConverter(),
			(JsonConverter)new AnimationCurveConverter(),
			(JsonConverter)new GradientConverter(),
			(JsonConverter)new OverridesConverter()
		}
	};

	public static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);

	private static readonly ThreadLocal<BlueprintJsonWrapper> BlueprintBeingReadInternal = new ThreadLocal<BlueprintJsonWrapper>();

	public static BlueprintJsonWrapper BlueprintBeingRead
	{
		get
		{
			return BlueprintBeingReadInternal.Value;
		}
		set
		{
			BlueprintBeingReadInternal.Value = value;
		}
	}
}

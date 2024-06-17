using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[HashNoGenerate]
public class NamedParametersContext
{
	public static class Hasher
	{
		[HasherFor(Type = typeof(NamedParametersContext))]
		public static Hash128 GetHash128(NamedParametersContext obj)
		{
			if (obj == null)
			{
				return default(Hash128);
			}
			Hash128 result = default(Hash128);
			if (RecursiveReferences.TryGetValue(obj, out var index))
			{
				result.Append(ref index);
				return result;
			}
			RecursiveReferences.Add(obj);
			int val = 0;
			foreach (KeyValuePair<string, object> param in obj.Params)
			{
				param.Deconstruct(out var key, out var value);
				string text = key;
				object par = value;
				Hash128 hash = default(Hash128);
				if (text != null)
				{
					Hash128 val2 = StringHasher.GetHash128(text);
					hash.Append(ref val2);
				}
				string text2 = Normalize(par);
				if (text2 != null)
				{
					Hash128 val3 = StringHasher.GetHash128(text2);
					hash.Append(ref val3);
				}
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
			return result;
		}
	}

	public class ContextData : ContextData<ContextData>
	{
		public NamedParametersContext Context { get; private set; }

		public ContextData Setup([NotNull] NamedParametersContext context)
		{
			Context = context;
			return this;
		}

		protected override void Reset()
		{
			Context = null;
		}
	}

	[JsonProperty]
	public readonly Dictionary<string, object> Params = new Dictionary<string, object>();

	[CanBeNull]
	public CutscenePlayerData Cutscene { get; set; }

	public IDisposable RequestContextData()
	{
		return ContextData<ContextData>.Request().Setup(this);
	}

	public bool IsTheSame(NamedParametersContext other)
	{
		foreach (KeyValuePair<string, object> param in Params)
		{
			if (!other.Params.TryGetValue(param.Key, out var value))
			{
				return false;
			}
			if (Normalize(value) != Normalize(param.Value))
			{
				return false;
			}
		}
		return true;
	}

	private static string Normalize(object par)
	{
		if (!(par is string result))
		{
			if (!(par is Entity entity))
			{
				if (!(par is IEntityRef entityRef))
				{
					if (!(par is ITypedEntityRef typedEntityRef))
					{
						if (par is SimpleBlueprint simpleBlueprint)
						{
							return simpleBlueprint.AssetGuid.ToString();
						}
						if (par == null)
						{
							return null;
						}
						return Convert.ToString(par, CultureInfo.InvariantCulture);
					}
					return typedEntityRef.GetId();
				}
				return entityRef.Id;
			}
			return entity.UniqueId;
		}
		return result;
	}
}

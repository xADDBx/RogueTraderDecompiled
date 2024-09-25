using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Serialization;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

public class EntityPartsManager : IDisposable, IHashable
{
	public class PartsGameStateAdapter : JsonConverter<List<EntityPart>>
	{
		public override bool CanRead => false;

		public override List<EntityPart> ReadJson(JsonReader reader, Type objectType, List<EntityPart> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, List<EntityPart> value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			writer.WriteStartArray();
			foreach (EntityPart item in value.Where(IsItemValid))
			{
				serializer.Serialize(writer, item);
			}
			writer.WriteEndArray();
		}

		public static Hash128 GetHash128(List<EntityPart> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (EntityPart item in obj)
			{
				if (IsItemValid(item))
				{
					Hash128 val = ClassHasher<EntityPart>.GetHash128(item);
					result.Append(ref val);
				}
			}
			return result;
		}

		private static bool IsItemValid(EntityPart obj)
		{
			if (obj != null)
			{
				return !(obj is PartUnitUISettings);
			}
			return false;
		}
	}

	private static readonly int TotalPartsCount;

	private static readonly Dictionary<Type, int> Indices;

	[ItemCanBeNull]
	private readonly EntityPart[] m_PartsCache = new EntityPart[TotalPartsCount];

	[ItemNotNull]
	[GameStateInclude]
	[HasherCustom(Type = typeof(PartsGameStateAdapter))]
	private readonly List<EntityPart> m_Parts = new List<EntityPart>();

	[JsonProperty]
	[UsedImplicitly]
	[GameStateIgnore("This is a runtime adapter for m_Parts")]
	private EntityPart[] Container
	{
		get
		{
			return m_Parts?.ToArray() ?? Array.Empty<EntityPart>();
		}
		set
		{
			m_Parts.Clear();
			m_Parts.AddRange(value);
			foreach (EntityPart entityPart in value)
			{
				int index = GetIndex(entityPart.GetType());
				m_PartsCache[index] = entityPart;
			}
		}
	}

	public Entity Owner { get; private set; }

	[CanBeNull]
	private IEntityPartsManagerDelegate Delegate => Owner as IEntityPartsManagerDelegate;

	static EntityPartsManager()
	{
		Type[] array = (from i in typeof(EntityPart).GetSubclasses()
			where i.GetConstructor(Type.EmptyTypes) != null
			select i into t
			orderby t.FullName
			select t).ToArray();
		TotalPartsCount = array.Length;
		Indices = new Dictionary<Type, int>(TotalPartsCount);
		IEnumerable<Type> enumerable = array.Where((Type i) => i.ContainsGenericParameters);
		if (enumerable.Any())
		{
			string text = string.Join(", ", enumerable);
			throw new Exception("Non abstract EntityParts with generic parameters are forbidden: " + text);
		}
		for (int j = 0; j < array.Length; j++)
		{
			Type type = array[j];
			typeof(Indexer<>).MakeGenericType(type).GetField("Index", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, j);
			Indices.Add(type, j);
		}
	}

	private static int GetIndex(Type type)
	{
		return Indices.Get(type, -1);
	}

	public EntityPartsManager(Entity owner)
	{
		Owner = owner;
	}

	[JsonConstructor]
	private EntityPartsManager(JsonConstructorMark _)
	{
	}

	[CanBeNull]
	public TPart GetOptional<TPart>() where TPart : EntityPart, new()
	{
		return Get<TPart>();
	}

	[CanBeNull]
	public TPart GetOptional<TPart>(Type type) where TPart : EntityPart
	{
		return Get<TPart>(type);
	}

	[NotNull]
	public TPart GetRequired<TPart>() where TPart : EntityPart, new()
	{
		return GetOptional<TPart>() ?? throw new Exception($"{Owner}: missing required part {typeof(TPart).Name}");
	}

	[NotNull]
	public TPart GetOrCreate<TPart>() where TPart : EntityPart, new()
	{
		return GetOptional<TPart>() ?? Add<TPart>();
	}

	public IEnumerable<TPart> GetAll<TPart>() where TPart : class
	{
		return m_Parts.OfType<TPart>();
	}

	[CanBeNull]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.NullChecks, false)]
	private TPart Get<TPart>() where TPart : EntityPart, new()
	{
		if (Owner == null)
		{
			throw new Exception("Can't access part " + typeof(TPart).Name + " owner is missing");
		}
		if (Owner.IsDisposed)
		{
			throw new Exception($"Can't access part {typeof(TPart).Name} of disposed unit {Owner}");
		}
		return (TPart)m_PartsCache[Indexer<TPart>.Index];
	}

	[CanBeNull]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.NullChecks, false)]
	private TPart Get<TPart>(Type type) where TPart : EntityPart
	{
		if (Owner == null)
		{
			throw new Exception("Can't access part " + typeof(TPart).Name + " owner is missing");
		}
		if (Owner.IsDisposed)
		{
			throw new Exception($"Can't access part {typeof(TPart).Name} of disposed unit {Owner}");
		}
		int index = GetIndex(type);
		if (index == -1)
		{
			return null;
		}
		return (TPart)m_PartsCache[index];
	}

	private TPart Add<TPart>() where TPart : EntityPart, new()
	{
		if (Get<TPart>() != null)
		{
			throw new Exception($"{Owner}: part {typeof(TPart).Name} already exists");
		}
		if (Owner.ForbidFactsAndPartsModifications && Owner.IsInitialized)
		{
			throw new Exception($"Can't add part to constant entity {Owner}");
		}
		TPart val = new TPart();
		m_Parts.Add(val);
		m_PartsCache[Indexer<TPart>.Index] = val;
		try
		{
			Delegate?.OnPartAppears(val);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
		val.Attach(Owner);
		return val;
	}

	public void Remove<TPart>() where TPart : EntityPart, new()
	{
		TPart val = Get<TPart>();
		if (val != null)
		{
			Remove(val);
		}
	}

	public void Remove([NotNull] EntityPart part)
	{
		int index = GetIndex(part.GetType());
		if (m_PartsCache[index] != part)
		{
			throw new Exception($"{Owner}: part {part.GetType().Name} isn't owned by EntityPartsManager");
		}
		part.Detach();
		m_Parts.Remove(part);
		m_PartsCache[index] = null;
		try
		{
			Delegate?.OnPartDisappears(part);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	public void RemoveAll<TPart>(Predicate<TPart> pred) where TPart : EntityPart
	{
		List<TPart> list = TempList.Get<TPart>();
		foreach (EntityPart part in m_Parts)
		{
			if (part is TPart val && pred(val))
			{
				list.Add(val);
			}
		}
		foreach (TPart item in list)
		{
			Remove(item);
		}
	}

	public void RemoveAll<TPart>() where TPart : EntityPart
	{
		RemoveAll((TPart _) => true);
	}

	public void ViewDidAttach()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.ViewDidAttach();
		}
	}

	public void ViewWillDetach()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.ViewWillDetach();
		}
	}

	public void PreSave()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.PreSave();
		}
	}

	public void PrePostLoad(Entity owner)
	{
		Owner = owner;
		foreach (EntityPart part in m_Parts)
		{
			part.PrePostLoad(owner);
			try
			{
				Delegate?.OnPartAppears(part);
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
		}
	}

	public void PostLoad()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.PostLoad();
		}
	}

	public void DidPostLoad()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.DidPostLoad();
		}
	}

	public void ApplyPostLoadFixes()
	{
		foreach (EntityPart item in m_Parts.ToTempList())
		{
			item.ApplyPostLoadFixes();
		}
	}

	public void Subscribe()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.Unsubscribe();
		}
	}

	public void OnHoldingStateChanged()
	{
		foreach (EntityPart part in m_Parts)
		{
			part.HoldingStateChanged();
		}
	}

	public void Dispose()
	{
		List<EntityPart> list = m_Parts.ToTempList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Remove(list[num]);
		}
		m_Parts.Clear();
		Array.Fill(m_PartsCache, null);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = PartsGameStateAdapter.GetHash128(m_Parts);
		result.Append(ref val);
		return result;
	}
}

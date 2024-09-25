using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility.Core;

public class CollectionClearingContractResolver : OptInContractResolver
{
	private abstract class ClearerBase
	{
		public abstract void Clear(object o);

		public void ClearListCallback(object o, StreamingContext context)
		{
			if (o != null)
			{
				Clear(o);
			}
		}
	}

	private class HashSetClearer<T> : ClearerBase
	{
		public override void Clear(object o)
		{
			(o as HashSet<T>)?.Clear();
		}
	}

	private static SerializationCallback ClearListCallback = delegate(object o, StreamingContext c)
	{
		if (o is IList list && !(list is Array) && !list.IsReadOnly)
		{
			list.Clear();
		}
	};

	protected override JsonArrayContract CreateArrayContract(Type objectType)
	{
		JsonArrayContract jsonArrayContract = base.CreateArrayContract(objectType);
		if (!objectType.IsArray)
		{
			if (typeof(IList).IsAssignableFrom(objectType))
			{
				jsonArrayContract.OnDeserializingCallbacks.Add(ClearListCallback);
			}
			if (typeof(HashSet<>) == objectType.GetGenericTypeDefinition())
			{
				ClearerBase @object = (ClearerBase)Activator.CreateInstance(typeof(HashSetClearer<>).MakeGenericType(objectType.GetGenericArguments()[0]));
				jsonArrayContract.OnDeserializingCallbacks.Add(@object.ClearListCallback);
			}
		}
		return jsonArrayContract;
	}
}

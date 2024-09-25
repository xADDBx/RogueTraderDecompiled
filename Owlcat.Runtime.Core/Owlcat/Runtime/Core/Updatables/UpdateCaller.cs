using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Owlcat.Runtime.Core.Updatables;

public static class UpdateCaller
{
	private class UpdateEntry
	{
		public IObjectRegistryBase Registry;

		public string Name;

		public bool NeedCopy;
	}

	private class LateUpdateEntry
	{
		public IObjectRegistryBase Registry;

		public string Name;

		public bool NeedCopy;
	}

	private class UpdateDriver : MonoBehaviour
	{
		private void Update()
		{
			UpdateCaller.Update();
		}

		private void LateUpdate()
		{
			UpdateCaller.LateUpdate();
		}
	}

	private static readonly List<UpdateEntry> UpdateEntries = new List<UpdateEntry>();

	private static readonly List<IUpdatable> ReusedList = new List<IUpdatable>();

	private static readonly List<LateUpdateEntry> LateUpdateEntries = new List<LateUpdateEntry>();

	private static readonly List<ILateUpdatable> ReusedListLate = new List<ILateUpdatable>();

	public static int Index { get; private set; }

	public static void Update()
	{
		for (int i = 0; i < UpdateEntries.Count; i++)
		{
			UpdateEntry updateEntry = UpdateEntries[i];
			if (updateEntry == null)
			{
				continue;
			}
			Index = 0;
			if (updateEntry.NeedCopy)
			{
				ObjectRegistryEnumerator<IUpdatable> enumerator = updateEntry.Registry.GetEnumerator<IUpdatable>();
				while (enumerator.MoveNext())
				{
					IUpdatable current = enumerator.Current;
					ReusedList.Add(current);
				}
				foreach (IUpdatable reused in ReusedList)
				{
					Call(reused);
				}
				ReusedList.Clear();
			}
			else
			{
				ObjectRegistryEnumerator<IUpdatable> enumerator3 = updateEntry.Registry.GetEnumerator<IUpdatable>();
				while (enumerator3.MoveNext())
				{
					Call(enumerator3.Current);
				}
			}
		}
		static void Call(IUpdatable obj)
		{
			try
			{
				obj?.DoUpdate();
			}
			catch (Exception ex)
			{
				if (obj is MonoBehaviour monoBehaviour && !monoBehaviour)
				{
					LogChannel.Default.Exception(ex, "In updateable: null " + obj.GetType().Name);
				}
				else
				{
					LogChannel.Default.Exception(ex, "In updateable: " + obj);
				}
			}
			Index++;
		}
	}

	public static void LateUpdate()
	{
		for (int i = 0; i < LateUpdateEntries.Count; i++)
		{
			LateUpdateEntry lateUpdateEntry = LateUpdateEntries[i];
			if (lateUpdateEntry == null)
			{
				continue;
			}
			if (lateUpdateEntry.NeedCopy)
			{
				ObjectRegistryEnumerator<ILateUpdatable> enumerator = lateUpdateEntry.Registry.GetEnumerator<ILateUpdatable>();
				while (enumerator.MoveNext())
				{
					ILateUpdatable current = enumerator.Current;
					current = enumerator.Current;
					ReusedListLate.Add(current);
				}
				foreach (ILateUpdatable item in ReusedListLate)
				{
					Call(item);
				}
				ReusedListLate.Clear();
			}
			else
			{
				ObjectRegistryEnumerator<ILateUpdatable> enumerator3 = lateUpdateEntry.Registry.GetEnumerator<ILateUpdatable>();
				while (enumerator3.MoveNext())
				{
					Call(enumerator3.Current);
				}
			}
		}
		static void Call(ILateUpdatable obj)
		{
			try
			{
				obj?.DoLateUpdate();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex, "In updateable: " + obj);
			}
		}
	}

	private static void GetUpdateablesViaReflectionFallback()
	{
		string currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.Contains("-Editor") || (!(assembly.GetName().Name == currentAssemblyName) && !assembly.GetReferencedAssemblies().Any((AssemblyName n) => n.Name == currentAssemblyName)))
			{
				continue;
			}
			foreach (Type item in (from t in assembly.GetTypes()
				where t.GetInterfaces().Contains(typeof(IUpdatable))
				select t).ToList())
			{
				UpdateEntries.Add(new UpdateEntry
				{
					Registry = Repository.Instance.GetRegistry(item),
					Name = item.Name,
					NeedCopy = item.CustomAttributes.Any((CustomAttributeData a) => a.AttributeType == typeof(UpdateCanChangeEnabledStateAttribute))
				});
			}
			foreach (Type item2 in (from t in assembly.GetTypes()
				where t.GetInterfaces().Contains(typeof(ILateUpdatable))
				select t).ToList())
			{
				LateUpdateEntries.Add(new LateUpdateEntry
				{
					Registry = Repository.Instance.GetRegistry(item2),
					Name = item2.Name,
					NeedCopy = item2.CustomAttributes.Any((CustomAttributeData a) => a.AttributeType == typeof(UpdateCanChangeEnabledStateAttribute))
				});
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	[UsedImplicitly]
	private static void OnAfterFirstSceneLoaded()
	{
		GameObject gameObject = new GameObject("[UpdateCaller]", typeof(UpdateDriver));
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		string currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.Contains("-Editor") || (!(assembly.GetName().Name == currentAssemblyName) && !assembly.GetReferencedAssemblies().Any((AssemblyName n) => n.Name == currentAssemblyName)))
			{
				continue;
			}
			Type type = assembly.GetType("Updateables");
			List<Type> list = (List<Type>)((type?.GetField("IUpdateables"))?.GetValue(null));
			if (list != null)
			{
				foreach (Type item in list)
				{
					UpdateEntries.Add(new UpdateEntry
					{
						Registry = Repository.Instance.GetRegistry(item),
						Name = item.Name,
						NeedCopy = item.CustomAttributes.Any((CustomAttributeData a) => a.AttributeType == typeof(UpdateCanChangeEnabledStateAttribute))
					});
				}
			}
			list = (List<Type>)((type?.GetField("ILateUpdateables"))?.GetValue(null));
			if (list == null)
			{
				continue;
			}
			foreach (Type item2 in list)
			{
				LateUpdateEntries.Add(new LateUpdateEntry
				{
					Registry = Repository.Instance.GetRegistry(item2),
					Name = item2.Name,
					NeedCopy = item2.CustomAttributes.Any((CustomAttributeData a) => a.AttributeType == typeof(UpdateCanChangeEnabledStateAttribute))
				});
			}
		}
		if (UpdateEntries.Count == 0)
		{
			Debug.LogError("ERROR! Could not find IUpdateables in generated code. Falling back to reflection. Please report to Maxim Savenkov!");
			GetUpdateablesViaReflectionFallback();
		}
	}
}

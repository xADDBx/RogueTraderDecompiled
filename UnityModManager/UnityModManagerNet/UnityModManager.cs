using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Code.Utility.ExtendedModInfo;
using dnlib.DotNet;
using HarmonyLib;
using Kingmaker;
using TinyJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UnityModManagerNet;

public class UnityModManager
{
	public sealed class Param
	{
		[Serializable]
		public class Mod
		{
			[XmlAttribute]
			public string Id;

			[XmlAttribute]
			public bool Enabled = true;
		}

		public static KeyBinding DefaultHotkey = new KeyBinding
		{
			keyCode = (KeyCode)291,
			modifiers = 1
		};

		public static KeyBinding EscapeHotkey = new KeyBinding
		{
			keyCode = (KeyCode)27
		};

		public KeyBinding Hotkey = new KeyBinding();

		public int CheckUpdates = 1;

		public int ShowOnStart = 0;

		public float WindowWidth;

		public float WindowHeight;

		public float UIScale = 1f;

		public string UIFont = null;

		public List<Mod> ModParams = new List<Mod>();

		private static readonly string filepath = Path.Combine(Application.persistentDataPath, "UnityModManager", "Params.xml");

		public void Save()
		{
			try
			{
				ModParams.Clear();
				foreach (ModEntry modEntry in ModEntries)
				{
					ModParams.Add(new Mod
					{
						Id = modEntry.Info.Id,
						Enabled = modEntry.Enabled
					});
				}
				using StreamWriter textWriter = new StreamWriter(filepath);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Param));
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception ex)
			{
				PFLog.UnityModManager.Error("Can't write file '" + filepath + "'.");
				Debug.LogException(ex);
			}
		}

		public static Param Load()
		{
			if (File.Exists(filepath))
			{
				try
				{
					using FileStream stream = File.OpenRead(filepath);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(Param));
					return xmlSerializer.Deserialize(stream) as Param;
				}
				catch (Exception ex)
				{
					PFLog.UnityModManager.Error("Can't read file '" + filepath + "'.");
					Debug.LogException(ex);
				}
			}
			return new Param();
		}

		internal void ReadModParams()
		{
			foreach (Mod modParam in ModParams)
			{
				ModEntry modEntry = FindMod(modParam.Id);
				if (modEntry != null)
				{
					modEntry.Enabled = modParam.Enabled;
				}
			}
		}
	}

	public class GameInfo
	{
		public string Name;

		public string ModsDirectory;

		public string ModInfo;

		public string MinimalManagerVersion;

		public GameInfo()
		{
			MinimalManagerVersion = "0.22.15";
			ModInfo = "Info.json";
			ModsDirectory = "UnityModManagerMods";
		}
	}

	public class ModEntry
	{
		public class ModLogger
		{
			protected readonly string Prefix;

			protected readonly string PrefixError;

			protected readonly string PrefixCritical;

			protected readonly string PrefixWarning;

			protected readonly string PrefixException;

			public ModLogger(string Id)
			{
				Prefix = "[" + Id + "] ";
				PrefixError = "[" + Id + "] [Error] ";
				PrefixCritical = "[" + Id + "] [Critical] ";
				PrefixWarning = "[" + Id + "] [Warning] ";
				PrefixException = "[" + Id + "] [Exception] ";
			}

			public void Log(string str)
			{
				PFLog.UnityModManager.Log(Prefix + " " + str);
			}

			public void Error(string str)
			{
				PFLog.UnityModManager.Error(PrefixError + " " + str);
			}

			public void Critical(string str)
			{
				PFLog.UnityModManager.Error(PrefixCritical + " " + str);
			}

			public void Warning(string str)
			{
				PFLog.UnityModManager.Warning(PrefixWarning + " " + str);
			}

			public void NativeLog(string str)
			{
				PFLog.UnityModManager.Log(Prefix + " " + str);
			}

			public void LogException(string key, Exception e)
			{
				PFLog.UnityModManager.Exception(e, PrefixException, new object[1] { key });
			}

			public void LogException(Exception e)
			{
				PFLog.UnityModManager.Exception(e, PrefixException);
			}
		}

		public readonly ModInfo Info;

		public readonly string Path;

		private Assembly mAssembly = null;

		public readonly Version Version = null;

		public readonly Version ManagerVersion = null;

		public readonly Version GameVersion = null;

		public Version NewestVersion;

		public readonly Dictionary<string, Version> Requirements = new Dictionary<string, Version>();

		public readonly List<string> LoadAfter = new List<string>();

		public string CustomRequirements = string.Empty;

		public readonly ModLogger Logger = null;

		public bool HasUpdate = false;

		public Func<ModEntry, bool> OnUnload = null;

		public Func<ModEntry, bool, bool> OnToggle = null;

		public Action<ModEntry> OnGUI = null;

		public Action<ModEntry> OnFixedGUI = null;

		public Action<ModEntry> OnShowGUI = null;

		public Action<ModEntry> OnHideGUI = null;

		public Action<ModEntry> OnSaveGUI = null;

		public Action<ModEntry, float> OnUpdate = null;

		public Action<ModEntry, float> OnLateUpdate = null;

		public Action<ModEntry, float> OnFixedUpdate = null;

		private Dictionary<long, MethodInfo> mCache = new Dictionary<long, MethodInfo>();

		private bool mStarted = false;

		private bool mErrorOnLoading = false;

		public bool Enabled = true;

		private bool mFirstLoading = true;

		private int mReloaderCount = 0;

		private bool mActive = false;

		public Assembly Assembly => mAssembly;

		public bool CanReload { get; private set; }

		public bool Started => mStarted;

		public bool ErrorOnLoading => mErrorOnLoading;

		public bool Toggleable => OnToggle != null;

		public bool Loaded => Assembly != null;

		public bool Active
		{
			get
			{
				return mActive;
			}
			set
			{
				if (value && !Loaded)
				{
					Stopwatch stopwatch = Stopwatch.StartNew();
					Load();
					Logger.NativeLog($"Loading time {(float)stopwatch.ElapsedMilliseconds / 1000f:f2} s.");
				}
				else
				{
					if (!mStarted || mErrorOnLoading)
					{
						return;
					}
					try
					{
						if (value)
						{
							if (!mActive)
							{
								if (OnToggle == null || OnToggle(this, arg2: true))
								{
									mActive = true;
									Logger.Log("Active.");
								}
								else
								{
									Logger.Log("Unsuccessfully.");
									Logger.NativeLog("OnToggle(true) failed.");
								}
							}
						}
						else if (!forbidDisableMods && mActive)
						{
							if (OnToggle != null && OnToggle(this, arg2: false))
							{
								mActive = false;
								Logger.Log("Inactive.");
							}
							else if (OnToggle != null)
							{
								Logger.NativeLog("OnToggle(false) failed.");
							}
							if (OnToggle == null)
							{
								mActive = false;
							}
						}
					}
					catch (Exception e)
					{
						Logger.LogException("OnToggle", e);
					}
				}
			}
		}

		public ModEntry(ModInfo info, string path)
		{
			Info = info;
			Path = path;
			Logger = new ModLogger(Info.Id);
			Version = ParseVersion(info.Version);
			ManagerVersion = ((!string.IsNullOrEmpty(info.ManagerVersion)) ? ParseVersion(info.ManagerVersion) : ((!string.IsNullOrEmpty(Config.MinimalManagerVersion)) ? ParseVersion(Config.MinimalManagerVersion) : new Version()));
			GameVersion = ((!string.IsNullOrEmpty(info.GameVersion)) ? ParseVersion(info.GameVersion) : new Version());
			if (info.Requirements != null && info.Requirements.Length != 0)
			{
				Regex regex = new Regex("(.*)-(\\d+\\.\\d+\\.\\d+).*");
				string[] requirements = info.Requirements;
				foreach (string text in requirements)
				{
					Match match = regex.Match(text);
					if (match.Success)
					{
						Requirements.Add(match.Groups[1].Value, ParseVersion(match.Groups[2].Value));
					}
					else if (!Requirements.ContainsKey(text))
					{
						Requirements.Add(text, null);
					}
				}
			}
			if (info.LoadAfter != null && info.LoadAfter.Length != 0)
			{
				LoadAfter.AddRange(info.LoadAfter);
			}
		}

		public bool Load()
		{
			if (Loaded)
			{
				return !mErrorOnLoading;
			}
			mErrorOnLoading = false;
			Logger.Log("Version '" + Info.Version + "'. Loading.");
			if (string.IsNullOrEmpty(Info.AssemblyName))
			{
				mErrorOnLoading = true;
				Logger.Error("AssemblyName is null.");
			}
			if (string.IsNullOrEmpty(Info.EntryMethod))
			{
				mErrorOnLoading = true;
				Logger.Error("EntryMethod is null.");
			}
			if (!string.IsNullOrEmpty(Info.ManagerVersion) && ManagerVersion > GetVersion())
			{
				mErrorOnLoading = true;
				Logger.Error("Mod Manager must be version '" + Info.ManagerVersion + "' or higher.");
			}
			if (!string.IsNullOrEmpty(Info.GameVersion) && gameVersion != VER_0 && GameVersion > gameVersion)
			{
				mErrorOnLoading = true;
				Logger.Error("Game must be version '" + Info.GameVersion + "' or higher.");
			}
			if (Requirements.Count > 0)
			{
				foreach (KeyValuePair<string, Version> requirement in Requirements)
				{
					string key = requirement.Key;
					ModEntry modEntry = FindMod(key);
					if (modEntry == null)
					{
						mErrorOnLoading = true;
						Logger.Error("Required mod '" + key + "' missing.");
					}
					else if (requirement.Value != null && requirement.Value > modEntry.Version)
					{
						mErrorOnLoading = true;
						Logger.Error($"Required mod '{key}' must be version '{requirement.Value}' or higher.");
					}
					else if (!modEntry.Active)
					{
						modEntry.Enabled = true;
						modEntry.Active = true;
						if (!modEntry.Active)
						{
							Logger.Log("Required mod '" + key + "' inactive.");
						}
					}
				}
			}
			if (LoadAfter.Count > 0)
			{
				foreach (string item in LoadAfter)
				{
					ModEntry modEntry2 = FindMod(item);
					if (modEntry2 == null)
					{
						Logger.Log("Optional mod '" + item + "' not found.");
					}
					else if (!modEntry2.Active && modEntry2.Enabled)
					{
						modEntry2.Active = true;
						if (!modEntry2.Active)
						{
							Logger.Log("Optional mod '" + item + "' enabled, but inactive.");
						}
					}
				}
			}
			if (mErrorOnLoading)
			{
				return false;
			}
			string text = System.IO.Path.Combine(Path, Info.AssemblyName);
			string text2 = text.Replace(".dll", ".pdb");
			string value = string.Empty;
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			int num = Array.IndexOf(commandLineArgs, "--umm-" + Info.Id + "-assembly-path");
			if (num != -1 && commandLineArgs.Length > num + 1)
			{
				value = (text = commandLineArgs[num + 1]);
			}
			if (File.Exists(text))
			{
				if (!string.IsNullOrEmpty(value))
				{
					try
					{
						mAssembly = Assembly.LoadFile(text);
						mFirstLoading = false;
					}
					catch (Exception e)
					{
						mErrorOnLoading = true;
						Logger.Error("Error loading file '" + text + "'.");
						Logger.LogException(e);
						return false;
					}
				}
				else
				{
					try
					{
						string text3 = text;
						string destFileName = text2;
						bool flag = false;
						if (mFirstLoading)
						{
							FileInfo fileInfo = new FileInfo(text);
							ushort num2 = (ushort)((long)fileInfo.LastWriteTimeUtc.GetHashCode() + (long)version.GetHashCode() + ManagerVersion.GetHashCode()).GetHashCode();
							text3 = text + $".{num2}.cache";
							destFileName = text3 + ".pdb";
							flag = File.Exists(text3);
							if (!flag)
							{
								string[] files = Directory.GetFiles(Path, "*.cache*");
								foreach (string path in files)
								{
									try
									{
										File.Delete(path);
									}
									catch (Exception)
									{
									}
								}
							}
						}
						if (ManagerVersion >= VER_0_13)
						{
							if (mFirstLoading)
							{
								if (!flag)
								{
									bool flag2 = false;
									ModuleDefMD moduleDefMD = ModuleDefMD.Load(File.ReadAllBytes(text));
									foreach (AssemblyRef assemblyRef in moduleDefMD.GetAssemblyRefs())
									{
										if (assemblyRef.FullName.StartsWith("0Harmony, Version=1."))
										{
											assemblyRef.Name = "0Harmony-1.2";
											flag2 = true;
										}
									}
									if (flag2)
									{
										moduleDefMD.Write(text3);
									}
									else
									{
										File.Copy(text, text3, overwrite: true);
									}
									if (File.Exists(text2))
									{
										File.Copy(text2, destFileName, overwrite: true);
									}
								}
								mAssembly = Assembly.LoadFile(text3);
								Type[] types = mAssembly.GetTypes();
								foreach (Type type in types)
								{
									if (type.GetCustomAttributes(typeof(EnableReloadingAttribute), inherit: true).Any())
									{
										CanReload = true;
										break;
									}
								}
							}
							else
							{
								ModuleDefMD moduleDefMD2 = ModuleDefMD.Load(File.ReadAllBytes(text));
								AssemblyDef assembly = moduleDefMD2.Assembly;
								string text4 = assembly.Name;
								int num3 = ++mReloaderCount;
								assembly.Name = text4 + num3;
								using MemoryStream memoryStream = new MemoryStream();
								moduleDefMD2.Write(memoryStream);
								if (File.Exists(text2))
								{
									mAssembly = Assembly.Load(memoryStream.ToArray(), File.ReadAllBytes(text2));
								}
								else
								{
									mAssembly = Assembly.Load(memoryStream.ToArray());
								}
							}
						}
						else
						{
							if (!flag)
							{
								bool flag3 = false;
								ModuleDefMD moduleDefMD3 = ModuleDefMD.Load(File.ReadAllBytes(text));
								foreach (TypeRef typeRef in moduleDefMD3.GetTypeRefs())
								{
									if (typeRef.FullName == "UnityModManagerNet.UnityModManager")
									{
										typeRef.ResolutionScope = new AssemblyRefUser(thisModuleDef.Assembly);
										flag3 = true;
									}
								}
								foreach (MemberRef item2 in from member in moduleDefMD3.GetMemberRefs()
									where member.IsFieldRef
									select member)
								{
									if (item2.Name == "modsPath" && item2.Class.FullName == "UnityModManagerNet.UnityModManager")
									{
										item2.Name = "OldModsPath";
										flag3 = true;
									}
								}
								foreach (AssemblyRef assemblyRef2 in moduleDefMD3.GetAssemblyRefs())
								{
									if (assemblyRef2.FullName.StartsWith("0Harmony, Version=1."))
									{
										assemblyRef2.Name = "0Harmony-1.2";
										flag3 = true;
									}
								}
								if (flag3)
								{
									moduleDefMD3.Write(text3);
								}
								else
								{
									File.Copy(text, text3, overwrite: true);
								}
							}
							mAssembly = Assembly.LoadFile(text3);
						}
						mFirstLoading = false;
					}
					catch (Exception e2)
					{
						mErrorOnLoading = true;
						Logger.Error("Error loading file '" + text + "'.");
						Logger.LogException(e2);
						return false;
					}
				}
				try
				{
					object[] param = new object[1] { this };
					Type[] types2 = new Type[1] { typeof(ModEntry) };
					if (FindMethod(Info.EntryMethod, types2, showLog: false) == null)
					{
						param = null;
						types2 = null;
					}
					if (!Invoke(Info.EntryMethod, out var result, param, types2) || (result != null && !(bool)result))
					{
						mErrorOnLoading = true;
						Logger.Log("Not loaded.");
					}
				}
				catch (Exception ex2)
				{
					mErrorOnLoading = true;
					Logger.Log(ex2.ToString());
					return false;
				}
				mStarted = true;
				if (!mErrorOnLoading)
				{
					Active = true;
					return true;
				}
			}
			else
			{
				mErrorOnLoading = true;
				Logger.Error("File '" + text + "' not found.");
			}
			return false;
		}

		internal void Reload()
		{
			if (!mStarted || !CanReload)
			{
				return;
			}
			if (OnSaveGUI != null)
			{
				OnSaveGUI(this);
			}
			Logger.Log("Reloading...");
			if (Toggleable)
			{
				bool forbidDisableMods = UnityModManager.forbidDisableMods;
				UnityModManager.forbidDisableMods = false;
				Active = false;
				UnityModManager.forbidDisableMods = forbidDisableMods;
			}
			else
			{
				mActive = false;
			}
			try
			{
				if (!Active && (OnUnload == null || OnUnload(this)))
				{
					mCache.Clear();
					Type type = typeof(Traverse).Assembly.GetType("HarmonyLib.AccessCache");
					object value = typeof(Traverse).GetField("Cache", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
					string[] array = new string[6] { "declaredFields", "declaredProperties", "declaredMethods", "inheritedFields", "inheritedProperties", "inheritedMethods" };
					string[] array2 = array;
					foreach (string name in array2)
					{
						IDictionary dictionary = (IDictionary)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
						dictionary.Clear();
					}
					Assembly assembly = Assembly;
					mAssembly = null;
					mStarted = false;
					mErrorOnLoading = false;
					OnToggle = null;
					OnGUI = null;
					OnFixedGUI = null;
					OnShowGUI = null;
					OnHideGUI = null;
					OnSaveGUI = null;
					OnUnload = null;
					OnUpdate = null;
					OnFixedUpdate = null;
					OnLateUpdate = null;
					CustomRequirements = null;
					if (!Load())
					{
						return;
					}
					Type[] types = assembly.GetTypes();
					Type[] array3 = types;
					foreach (Type type2 in array3)
					{
						Type type3 = Assembly.GetType(type2.FullName);
						if (!(type3 != null))
						{
							continue;
						}
						FieldInfo[] fields = type2.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						foreach (FieldInfo fieldInfo in fields)
						{
							if (!fieldInfo.GetCustomAttributes(typeof(SaveOnReloadAttribute), inherit: true).Any())
							{
								continue;
							}
							FieldInfo field = type3.GetField(fieldInfo.Name);
							if (!(field != null))
							{
								continue;
							}
							Logger.Log("Copying field '" + fieldInfo.DeclaringType.Name + "." + fieldInfo.Name + "'");
							try
							{
								if (fieldInfo.FieldType != field.FieldType)
								{
									if (fieldInfo.FieldType.IsEnum && field.FieldType.IsEnum)
									{
										field.SetValue(null, Convert.ToInt32(fieldInfo.GetValue(null)));
									}
									else if ((!fieldInfo.FieldType.IsClass || !field.FieldType.IsClass) && fieldInfo.FieldType.IsValueType && !field.FieldType.IsValueType)
									{
									}
								}
								else
								{
									field.SetValue(null, fieldInfo.GetValue(null));
								}
							}
							catch (Exception ex)
							{
								Logger.Error(ex.ToString());
							}
						}
					}
					return;
				}
				if (Active)
				{
					Logger.Log("Must be deactivated.");
				}
			}
			catch (Exception ex2)
			{
				Logger.Error(ex2.ToString());
			}
			Logger.Log("Reloading canceled.");
		}

		public bool Invoke(string namespaceClassnameMethodname, out object result, object[] param = null, Type[] types = null)
		{
			result = null;
			try
			{
				MethodInfo methodInfo = FindMethod(namespaceClassnameMethodname, types);
				if (methodInfo != null)
				{
					result = methodInfo.Invoke(null, param);
					return true;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error trying to call '" + namespaceClassnameMethodname + "'.");
				Logger.LogException(e);
			}
			return false;
		}

		private MethodInfo FindMethod(string namespaceClassnameMethodname, Type[] types, bool showLog = true)
		{
			long num = namespaceClassnameMethodname.GetHashCode();
			if (types != null)
			{
				Type[] array = types;
				foreach (Type type in array)
				{
					num += type.GetHashCode();
				}
			}
			if (!mCache.TryGetValue(num, out var value))
			{
				if (mAssembly != null)
				{
					string text = null;
					string text2 = null;
					int num2 = namespaceClassnameMethodname.LastIndexOf('.');
					if (num2 != -1)
					{
						text = namespaceClassnameMethodname.Substring(0, num2);
						text2 = namespaceClassnameMethodname.Substring(num2 + 1);
						Type type2 = mAssembly.GetType(text);
						if (type2 != null)
						{
							if (types == null)
							{
								types = new Type[0];
							}
							value = type2.GetMethod(text2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
							if (value == null && showLog)
							{
								if (types.Length != 0)
								{
									Logger.Log("Method '" + namespaceClassnameMethodname + "[" + string.Join(", ", types.Select((Type x) => x.Name).ToArray()) + "]' not found.");
								}
								else
								{
									Logger.Log("Method '" + namespaceClassnameMethodname + "' not found.");
								}
							}
						}
						else if (showLog)
						{
							Logger.Error("Class '" + text + "' not found.");
						}
					}
					else if (showLog)
					{
						Logger.Error("Function name error '" + namespaceClassnameMethodname + "'.");
					}
				}
				else if (showLog)
				{
					PFLog.UnityModManager.Error("Can't find method '" + namespaceClassnameMethodname + "'. Mod '" + Info.Id + "' is not loaded.");
				}
				mCache[num] = value;
			}
			return value;
		}
	}

	public class Repository
	{
		[Serializable]
		public class Release : IEquatable<Release>
		{
			public string Id;

			public string Version;

			public string DownloadUrl;

			public bool Equals(Release other)
			{
				return Id.Equals(other.Id);
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					return false;
				}
				return obj is Release other && Equals(other);
			}

			public override int GetHashCode()
			{
				return Id.GetHashCode();
			}
		}

		public Release[] Releases;
	}

	public class ModSettings
	{
		public virtual void Save(ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public virtual string GetPath(ModEntry modEntry)
		{
			return Path.Combine(modEntry.Path, "Settings.xml");
		}

		public static void Save<T>(T data, ModEntry modEntry) where T : ModSettings, new()
		{
			Save(data, modEntry, null);
		}

		public static void Save<T>(T data, ModEntry modEntry, XmlAttributeOverrides attributes) where T : ModSettings, new()
		{
			string path = data.GetPath(modEntry);
			try
			{
				using StreamWriter textWriter = new StreamWriter(path);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), attributes);
				xmlSerializer.Serialize(textWriter, data);
			}
			catch (Exception e)
			{
				modEntry.Logger.Error("Can't save " + path + ".");
				modEntry.Logger.LogException(e);
			}
		}

		public static T Load<T>(ModEntry modEntry) where T : ModSettings, new()
		{
			T val = new T();
			string path = val.GetPath(modEntry);
			if (File.Exists(path))
			{
				try
				{
					using FileStream stream = File.OpenRead(path);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
					return (T)xmlSerializer.Deserialize(stream);
				}
				catch (Exception e)
				{
					modEntry.Logger.Error("Can't read " + path + ".");
					modEntry.Logger.LogException(e);
				}
			}
			return val;
		}

		public static T Load<T>(ModEntry modEntry, XmlAttributeOverrides attributes) where T : ModSettings, new()
		{
			T val = new T();
			string path = val.GetPath(modEntry);
			if (File.Exists(path))
			{
				try
				{
					using FileStream stream = File.OpenRead(path);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), attributes);
					return (T)xmlSerializer.Deserialize(stream);
				}
				catch (Exception e)
				{
					modEntry.Logger.Error("Can't read " + path + ".");
					modEntry.Logger.LogException(e);
				}
			}
			return val;
		}
	}

	public class ModInfo : IEquatable<ModInfo>
	{
		public string Id;

		public string DisplayName;

		public string Author;

		public string Version;

		public string ManagerVersion;

		public string GameVersion;

		public string[] Requirements;

		public string[] LoadAfter;

		public string AssemblyName;

		public string EntryMethod;

		public string HomePage;

		public string Repository;

		[NonSerialized]
		public bool IsCheat = true;

		public static implicit operator bool(ModInfo exists)
		{
			return exists != null;
		}

		public bool Equals(ModInfo other)
		{
			return Id.Equals(other.Id);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return obj is ModInfo other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	private static class Textures
	{
		private static string WindowBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAEACAYAAACZCaebAAAAnElEQVRIS63MtQHDQADAwPdEZmaG/fdJCq2g7qqLvu/7hRBCZOF9X0ILz/MQWrjvm1DHdV3MFs7zJLRwHAehhX3fCS1s20ZoYV1XQgvLshDqmOeZ2cI0TYQWxnEktDAMA6GFvu8JLXRdR2ihbVtCHU3TMFuo65rQQlVVhBbKsiS0UBQFoYU8zwktZFlGqCNNU2YLSZIQWojjmFDCH22GtZAncD8TAAAAAElFTkSuQmCC";

		private static string SettingsNormalBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAQAAABKfvVzAAAACXBIWXMAAA3XAAAN1wFCKJt4AAAJsmlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTgtMDktMDlUMTY6MTI6NTArMDM6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTA5LTA5VDE4OjMyOjQ2KzAzOjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDE4LTA5LTA5VDE4OjMyOjQ2KzAzOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpiZTNlYTRkYy0zYjU4LTIxNDctOWQzMi04NmRiYTFjNmM4MjMiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDoyMmYzNGE4Yy1jZjBiLTVkNDMtODRkYy05ODgyY2UyMTFhYTMiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDpiNjZhYTU5Ny1mODA5LTA0NGYtOWYzYi0wMTZlODdiODFkZjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmI2NmFhNTk3LWY4MDktMDQ0Zi05ZjNiLTAxNmU4N2I4MWRmMSIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxNjoxMjo1MCswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY29udmVydGVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJmcm9tIGltYWdlL3BuZyB0byBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDoyNGZjMTdlMS1jOWY3LTk5NDUtYmFkOC00NWZhNGI1MjgwNTgiIHN0RXZ0OndoZW49IjIwMTgtMDktMDlUMTg6MzI6MjcrMDM6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmZiYTVjMTY5LTkzNGItOGI0NS1hMjg3LTc2NzJkYjc1MjY2ZiIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxODozMjo0NiswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY29udmVydGVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJkZXJpdmVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJjb252ZXJ0ZWQgZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6YmUzZWE0ZGMtM2I1OC0yMTQ3LTlkMzItODZkYmExYzZjODIzIiBzdEV2dDp3aGVuPSIyMDE4LTA5LTA5VDE4OjMyOjQ2KzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDpmYmE1YzE2OS05MzRiLThiNDUtYTI4Ny03NjcyZGI3NTI2NmYiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6YjY2YWE1OTctZjgwOS0wNDRmLTlmM2ItMDE2ZTg3YjgxZGYxIiBzdFJlZjpvcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6YjY2YWE1OTctZjgwOS0wNDRmLTlmM2ItMDE2ZTg3YjgxZGYxIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+KBN97QAAAGtJREFUOI3FkTESgCAMBDcOhX6ZHr+snZZy4MRBUbcLmYS7nCWAkYMVIWpJAGDJXgyXwW/XWNraBgIG5EOFpNJDs6RwttWj+YebkhTnBN/l4EjqlIPipvJ+Dl08CHN2s2h/Bac8lXRpmknLHSshDz5/7DVxAAAAAElFTkSuQmCC";

		private static string SettingsActiveBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAQAAABKfvVzAAAACXBIWXMAAA3XAAAN1wFCKJt4AAAJsmlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTgtMDktMDlUMTY6MTI6NTArMDM6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTA5LTA5VDE4OjMzOjAyKzAzOjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDE4LTA5LTA5VDE4OjMzOjAyKzAzOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDowNTVkNWY0My00MmViLTdiNDMtOTdkZi1mYjc2MWY2NzcyNjkiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDoyZjNlNWU4NC04NzM4LTdlNDEtYmExZi00MzljMWU3YjI2NTEiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDpiNjZhYTU5Ny1mODA5LTA0NGYtOWYzYi0wMTZlODdiODFkZjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmI2NmFhNTk3LWY4MDktMDQ0Zi05ZjNiLTAxNmU4N2I4MWRmMSIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxNjoxMjo1MCswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY29udmVydGVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJmcm9tIGltYWdlL3BuZyB0byBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDoyNGZjMTdlMS1jOWY3LTk5NDUtYmFkOC00NWZhNGI1MjgwNTgiIHN0RXZ0OndoZW49IjIwMTgtMDktMDlUMTg6MzI6MjcrMDM6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjQ1NGEwNjIxLTQwZmMtNTU0Yy1hZDkzLWNkM2ZjMzIxMTdlYyIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxODozMzowMiswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY29udmVydGVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJkZXJpdmVkIiBzdEV2dDpwYXJhbWV0ZXJzPSJjb252ZXJ0ZWQgZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6MDU1ZDVmNDMtNDJlYi03YjQzLTk3ZGYtZmI3NjFmNjc3MjY5IiBzdEV2dDp3aGVuPSIyMDE4LTA5LTA5VDE4OjMzOjAyKzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDo0NTRhMDYyMS00MGZjLTU1NGMtYWQ5My1jZDNmYzMyMTE3ZWMiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6YjY2YWE1OTctZjgwOS0wNDRmLTlmM2ItMDE2ZTg3YjgxZGYxIiBzdFJlZjpvcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6YjY2YWE1OTctZjgwOS0wNDRmLTlmM2ItMDE2ZTg3YjgxZGYxIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+rc0l4AAAAF5JREFUOI3FkUsSgCAMQx+OC73/Zd3VpUYhTMcPb0chk9CUAFg42LCUAIjzxAsmf11ziP4jFcDXkZKCtENaMFdmdgV/9aC8Hek+api1I1nGFKdcdjamOOVppP6nVz3uep4WJ6dlbD4AAAAASUVORK5CYII=";

		private static string StatusActiveBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAIf2lUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTgtMDktMDlUMTc6MDg6NTIrMDM6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMDktMDlUMTc6MDk6MDErMDM6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTA5LTA5VDE3OjA5OjAxKzAzOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyOWQ1NWMzNi0xNzYxLTE1NDYtOTgyMC1kMWRkNjliZDE0NTciIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDplOTEwMGY5Yi00NTU4LWE2NDYtYTY3Ny0xY2I0NjY3YTRjYjciIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDplODZhMjVmNi1lNmU0LTdlNDEtYWI0MC02MzZkMzljZDkwZjgiIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmU4NmEyNWY2LWU2ZTQtN2U0MS1hYjQwLTYzNmQzOWNkOTBmOCIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxNzowODo1MiswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NWYwNzJhODUtYjU4Zi1mMzQ4LTliOGQtZGQyZjE3NDQyOGY2IiBzdEV2dDp3aGVuPSIyMDE4LTA5LTA5VDE3OjA5OjAxKzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjb252ZXJ0ZWQiIHN0RXZ0OnBhcmFtZXRlcnM9ImZyb20gYXBwbGljYXRpb24vdm5kLmFkb2JlLnBob3Rvc2hvcCB0byBpbWFnZS9wbmciLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImRlcml2ZWQiIHN0RXZ0OnBhcmFtZXRlcnM9ImNvbnZlcnRlZCBmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDoyOWQ1NWMzNi0xNzYxLTE1NDYtOTgy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";

		private static string StatusInactiveBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAIf2lUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTgtMDktMDlUMTc6MDg6NTIrMDM6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMDktMDlUMTc6NTg6MzErMDM6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTA5LTA5VDE3OjU4OjMxKzAzOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDphYjJiZWI3Ni0wY2JiLWIyNDctYjdhYS0zMWI1NjYzYjJjNDEiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDpiNzRkODNkZC03NmM0LWQ1NDYtYWEyNi02N2Y3YjMyMGNlYzgiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDplODZhMjVmNi1lNmU0LTdlNDEtYWI0MC02MzZkMzljZDkwZjgiIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmU4NmEyNWY2LWU2ZTQtN2U0MS1hYjQwLTYzNmQzOWNkOTBmOCIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxNzowODo1MiswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6ZThjZTMxMGEtODRkYi1lNjQyLWEyYWYtOWQyNDQ0OWUwOTBkIiBzdEV2dDp3aGVuPSIyMDE4LTA5LTA5VDE3OjU4OjMxKzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjb252ZXJ0ZWQiIHN0RXZ0OnBhcmFtZXRlcnM9ImZyb20gYXBwbGljYXRpb24vdm5kLmFkb2JlLnBob3Rvc2hvcCB0byBpbWFnZS9wbmciLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImRlcml2ZWQiIHN0RXZ0OnBhcmFtZXRlcnM9ImNvbnZlcnRlZCBmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDphYjJiZWI3Ni0wY2JiLWIyNDctYjdhYS0zMWI1NjYzYjJjNDEiIHN0RXZ0OndoZW49IjIwMTgtMDktMDlUMTc6NTg6MzErMDM6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPC9yZGY6U2VxPiA8L3htcE1NOkhpc3Rvcnk+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOmU4Y2UzMTBhLTg0ZGItZTY0Mi1hMmFmLTlkMjQ0NDllMDkwZCIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDplODZhMjVmNi1lNmU0LTdlNDEtYWI0MC02MzZkMzljZDkwZjgiIHN0UmVmOm9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDplODZhMjVmNi1lNmU0LTdlNDEtYWI0MC02MzZkMzljZDkwZjgiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz6CJz7aAAAD3UlEQVRIiaWWS0grVxzGf2dmjIYQm/iKWRhEr6UQrQXRhXTVLHqr9QVxobhxewUXQttr6baPS6GL0nZ53VUkGxeF3m677sJ2fKT4SNCSaywSIYnJvDJdmBkmqba39MDHOcOc833/1/nPiK2tLWRZRlGUhlmSJGZmZsLAEvAe8CbQC+jAH8AJ8COwDRR4YChCCCRJQgjhYm5uLgR8CGzec8YHvFHH+8C3wOfAl/cJKUIIAJd8YWHhSf0QAOfn51xfX1MqlTAMAyEEra2t+P1+Ojs7icVi1A3ZBNaA7xoEnIVt2ySTyY+BTwHOzs7IZrNomtbgIYBhGBSLRS4vLzk+Pqa/v5/BwUHqhoWAzxxeeX5+HiEEy8vLq8DXAPv7+6TTaUzTdMWd2UGtVsO2bXRdJ5/PY5omPT09AAngHNgDkGq1GisrK68BzwFUVSWdTmNZ1n9COp1GVVXH8Od1T+4EgKcAR0dHqKqKaZqYpollWe7aMAx37YV3j6qqHB4eOiIfAQjbtjuAa4BUKkWlUkGSpAZ44+/NmRMqL/x+P4uLi862ToW7Oufg4ICbm5sHyR8SaBbRNI2DgwPi8TjAkgK8C5DJZNB1/X8L1Go1MpmMI/BYAd4yTZNcLtcgcN8FfEigWSSXy2GaJoqijCpApFgsUigUqNVqf7P+VXLQLKDrOsVikXA4HFEArVwu+wzDcG9qM7kkSQCuiHMvvOTeWZIkSqUS4XAYBXhZrVaDQggqlUpDSP6tipqJHQQCAarVKkBeAX63bfv1lpYWNE3Dtu0GkWbcF38vALq6uhw7flWAnwKBwEwoFOL09NTd5ITkVZPsPRMKhQgEAgAvFGC7u7v7m1gsxt7eHuVyuSHO95E3i3jJA4EAsViM7u5ugG356uqqMjs722aa5tsXFxdu42puA95nb9twepGTh4GBASYnJ4lEIl8APyh1C54NDQ09jcfjFAoFstks9R7l5uQhD5x3kiTR399PPB5naGgI4BmAZNs2q6urN7IsP0kkEsTjcaLRaIOlTgk3w7snGo0yPDxMIpFAluU14AZAHh8fx7IsUqnUL0tLS9bg4OA7hUKB29tbyuUyhmH8Y9X4fD76+voYGxsjmUzS3t7+CfCV46U8MTGBbdtYlsXOzs7PKysrf46MjEx3dHQghGi4VJZlAeDz+QgGg/T29jI6OsrU1BTT09P4/f41LzmAWF9fR9M0F4ZhkEqlwsAH1Wp1U1VVTk5OyOfzFItFAILBIJFIhEePHjEyMkJbW9uDH32xsbHhkuu67sKyLHZ3d8PAMvCYu9+WaP3cS+A34AXw/X3EzvgLFKrw+QQN6BEAAAAASUVORK5CYII=";

		private static string StatusNeedRestartBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAIf2lUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTgtMDktMDlUMTc6MDg6NTIrMDM6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMDktMDlUMTc6NTc6MTArMDM6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE4LTA5LTA5VDE3OjU3OjEwKzAzOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDphYjljYTE2MS01NTA1LTM2NDItYjBmZi04NWM2YTQ4OTQzOTMiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDpkZjAzYzgxNS01ZGFhLTZjNDUtOWU4ZC03NWJhYWViNWY0OGEiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDplODZhMjVmNi1lNmU0LTdlNDEtYWI0MC02MzZkMzljZDkwZjgiIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmU4NmEyNWY2LWU2ZTQtN2U0MS1hYjQwLTYzNmQzOWNkOTBmOCIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0wOVQxNzowODo1MiswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6MGE1N2VhNDUtMjc3ZC01MjQxLWE2MjktYjlmYTFhYTNlNTk0IiBzdEV2dDp3aGVuPSIyMDE4LTA5LTA5VDE3OjU3OjEwKzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjb252ZXJ0ZWQiIHN0RXZ0OnBhcmFtZXRlcnM9ImZyb20gYXBwbGljYXRpb24vdm5kLmFkb2JlLnBob3Rvc2hvcCB0byBpbWFnZS9wbmciLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImRlcml2ZWQiIHN0RXZ0OnBhcmFtZXRlcnM9ImNvbnZlcnRlZCBmcm9tIGFwcGxpY2F0aW9uL3ZuZC5hZG9iZS5waG90b3Nob3AgdG8gaW1hZ2UvcG5nIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDphYjljYTE2MS01NTA1LTM2NDItYjBm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";

		private static string WWWBase64 = "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAfCAYAAAAfrhY5AAAACXBIWXMAAA7EAAAOxAGVKw4bAAAGjWlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiB4bXA6Q3JlYXRlRGF0ZT0iMjAxOC0wOS0xOFQyMDowMjo0OSswMzowMCIgeG1wOk1vZGlmeURhdGU9IjIwMTgtMDktMTlUMTI6MjE6MDkrMDM6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMDktMTlUMTI6MjE6MDkrMDM6MDAiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjllZWQwYzk5LWQ4YmItMTk0Yi1hOTU2LWI0Mzg4MzZiNGE1NyIgeG1wTU06RG9jdW1lbnRJRD0iYWRvYmU6ZG9jaWQ6cGhvdG9zaG9wOjE0YmE2MTBjLTcxMzMtM2E0Yy05NDkyLWUwNTczZDE5YmUxOCIgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjRlYjUxNDViLTY5YjEtODE0Zi1hNmEyLWRhNGU3ZDliMjlmMyI+IDx4bXBNTTpIaXN0b3J5PiA8cmRmOlNlcT4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNyZWF0ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NGViNTE0NWItNjliMS04MTRmLWE2YTItZGE0ZTdkOWIyOWYzIiBzdEV2dDp3aGVuPSIyMDE4LTA5LTE4VDIwOjAyOjQ5KzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDozYWU1NzdiOS00MWEwLTU1NGUtYTU1NC0zMmViZmI1ZDFjNzMiIHN0RXZ0OndoZW49IjIwMTgtMDktMThUMjA6MDY6MTErMDM6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjllZWQwYzk5LWQ4YmItMTk0Yi1hOTU2LWI0Mzg4MzZiNGE1NyIgc3RFdnQ6d2hlbj0iMjAxOC0wOS0xOVQxMjoyMTowOSswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz5HbzubAAAEM0lEQVRIicWXTWwVVRTHf2doQUKnMWkhgOK7o2gMoiCGHZBoQozs+LCyMOwq8T2iW1pJCCZQjStiZxpwZ4wp5WslCxdScFUaJMpHjBrmloABeWxmXiFt6RwXnfcYXqcfiTY9izfz7v3N+Z+cuffMuaKqiAgzWp8uKJSH3xC0U9DVCi2itKjwQOAByh+JOF8M3VvyK4ckmcmdqiKzEff86DOEdpDCzFHqEHA8LDYfmUncmVIwiHZ6fvS3F8SKyDpN2INqr6JXaWR5WHQFICy6kiSyQtGrqPZqwh5V2eAFsXpBdKfQE22fSiNX3PhxF8pJRFag2h8W3TYHeQ6R3Q1jC7aF7e69LD+0r+luw9iCbYjsRmSFLbm7gAsgK52E08aPD89KfE2fLhRhvwqjoMWw6L6zpk8XqnAE4K9Pl9zOc1QdF6HrrWPaGH7c9DawT2FMhM41fbpwWnEviL56VK6MoKyzxeZnwmJzDyL66H7lCoIR1Z35CaxZG+A9GI8vI6Jh0fVtqXmRiqx/VK6MGD/uysK1BVcIKu856LkZnP83U1DkXVtq+vGpBSckx+dUeEIESL55EowqAOnq/AnVSfsunZuUci+INWfs/bxxVMX48cXqnKpO/JggOmCC+GResF5PvNX40WDuXJ4IYPxo0OuJt+bPxae8IOqsic+H1SrcfFmDtfYMsJ2J5dAGnAB2p9cy0JLOAZwBNgHLsqwx5oS1tgxcNMbsALDWKnDfGLPMWlvPKnDWAQZSx8+mjgG2pKItdcEOAEvrWWttlb1Uxy+11uaxAJccoLqYXgE2p/ebgVdzMjUTWy8+LSthGAJcAx4CG4HvgA+B31PwOrA240zTIDYC3wJ7qqwxprZNrbXXgNemYMeNMWurRWYgBQD602s1wrxtVmXP17FZG5iGHYQntT2brvM8bQNMbf3TzNW/giw7kCd+C7gJ2AyYW2BS1taxU4nfMsZk2UFgfvf5vFY4ByZ6NOPHp/KguarttQiqjrwg7p/Tr1oQ/5z9qtWaCeNHQyLyQl60/7OFYdF9sa57ddrnfAEoIOx98j+jZ/y4ywtiLXRX1mefMX501Qti9bqjXdnx+vQaP/7AC2I1QfRbdrwQVN70glizPdykvt2W3I7FrU2LHEevGD8a8YK4hKq0NLgbUCyO5DYcVROhF+Vm4Z67IX3Hnxg/GnXQXxa3Ni2yJbcjy09qnW+0yajCYYFGoNvrqfRf3itjidABsPro8PN5wi/5D1dNRMD+C4fksempXBQ4KtAAfH6jTUbrn8k9NNiieyCBHajeAbaYID6NcBfV3seN4+cK3ZXlWX7V18Mrx+XxD6r6faKUvSA+K7BJ0dsJ7AiL7sHcTM3yrNYB8hGCmRacsFBVj9lS85fTQbM+KAJwUB2zfPh10aQDeFmVVoFWhbIIZeBPVI6E/zRdm+0p9V+lFDjVQcCm0AAAAABJRU5ErkJggg==";

		private static string UpdatesBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEIAAABCCAYAAADjVADoAAAACXBIWXMAAA7EAAAOxAGVKw4bAAAFwmlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDIgNzkuMTYwOTI0LCAyMDE3LzA3LzEzLTAxOjA2OjM5ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiB4bXA6Q3JlYXRlRGF0ZT0iMjAxOC0wOS0xOFQyMDozNjo0NSswMzowMCIgeG1wOk1vZGlmeURhdGU9IjIwMTgtMDktMTlUMTE6MDY6NTkrMDM6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMTgtMDktMTlUMTE6MDY6NTkrMDM6MDAiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOmE1Zjg4NDg3LWU0MmUtMDc0OS04MDYwLWE5YmMwNWM2Yzg2NCIgeG1wTU06RG9jdW1lbnRJRD0iYWRv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";

		private static string QuestionBase64 = "iVBORw0KGgoAAAANSUhEUgAAABQAAAAWCAYAAADAQbwGAAABN2lDQ1BBZG9iZSBSR0IgKDE5OTgpAAAokZWPv0rDUBSHvxtFxaFWCOLgcCdRUGzVwYxJW4ogWKtDkq1JQ5ViEm6uf/oQjm4dXNx9AidHwUHxCXwDxamDQ4QMBYvf9J3fORzOAaNi152GUYbzWKt205Gu58vZF2aYAoBOmKV2q3UAECdxxBjf7wiA10277jTG+38yH6ZKAyNguxtlIYgK0L/SqQYxBMygn2oQD4CpTto1EE9AqZf7G1AKcv8ASsr1fBBfgNlzPR+MOcAMcl8BTB1da4Bakg7UWe9Uy6plWdLuJkEkjweZjs4zuR+HiUoT1dFRF8jvA2AxH2w3HblWtay99X/+PRHX82Vun0cIQCw9F1lBeKEuf1UYO5PrYsdwGQ7vYXpUZLs3cLcBC7dFtlqF8hY8Dn8AwMZP/fNTP8gAAAAJcEhZcwAACxMAAAsTAQCanBgAAAj9aVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjYtYzE0MiA3OS4xNjA5MjQsIDIwMTcvMDcvMTMtMDE6MDY6MzkgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6ZGM9Imh0dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8xLjEvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIiB4bXA6Q3JlYXRlRGF0ZT0iMjAyMi0xMC0wOFQxMDowNzoxNiswMzowMCIgeG1wOk1ldGFkYXRhRGF0ZT0iMjAyMi0xMC0wOFQxMjowOTo0NiswMzowMCIgeG1wOk1vZGlmeURhdGU9IjIwMjItMTAtMDhUMTI6MDk6NDYrMDM6MDAiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjFlYmE4Yjg0LWZhMGEtOWY0MS05OTE3LWIwN2U0ZjMxMTlhMiIgeG1wTU06RG9jdW1lbnRJRD0iYWRvYmU6ZG9jaWQ6cGhvdG9zaG9wOjFiZDVlYzRmLTNjOGQtN2M0NS1hZWMzLTI2NzE4YTZhNGI0YiIgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjcwZTcwODU1LWE2NzMtNDY0NC04MmMxLTE5MmVhNWM5YTJjZiIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMyI+IDx4bXBNTTpIaXN0b3J5PiA8cmRmOlNlcT4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNyZWF0ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NzBlNzA4NTUtYTY3My00NjQ0LTgyYzEtMTkyZWE1YzlhMmNmIiBzdEV2dDp3aGVuPSIyMDIyLTEwLTA4VDEwOjA3OjE2KzAzOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDo0Y2QwZDZjOC1hZjJiLTQ3NGYtYThlZS04NDQyYWIzMzMxNGQiIHN0RXZ0OndoZW49IjIwMjItMTAtMDhUMTI6MDk6NDYrMDM6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNvbnZlcnRlZCIgc3RFdnQ6cGFyYW1ldGVycz0iZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iZGVyaXZlZCIgc3RFdnQ6cGFyYW1ldGVycz0iY29udmVydGVkIGZyb20gYXBwbGljYXRpb24vdm5kLmFkb2JlLnBob3Rvc2hvcCB0byBpbWFnZS9wbmciLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjFlYmE4Yjg0LWZhMGEtOWY0MS05OTE3LWIwN2U0ZjMxMTlhMiIgc3RFdnQ6d2hlbj0iMjAyMi0xMC0wOFQxMjowOTo0NiswMzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6NGNkMGQ2YzgtYWYyYi00NzRmLWE4ZWUtODQ0MmFiMzMzMTRkIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjcwZTcwODU1LWE2NzMtNDY0NC04MmMxLTE5MmVhNWM5YTJjZiIgc3RSZWY6b3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjcwZTcwODU1LWE2NzMtNDY0NC04MmMxLTE5MmVhNWM5YTJjZiIvPiA8cGhvdG9zaG9wOlRleHRMYXllcnM+IDxyZGY6QmFnPiA8cmRmOmxpIHBob3Rvc2hvcDpMYXllck5hbWU9Ij8iIHBob3Rvc2hvcDpMYXllclRleHQ9Ij8iLz4gPC9yZGY6QmFnPiA8L3Bob3Rvc2hvcDpUZXh0TGF5ZXJzPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/Pm3hzHgAAAEiSURBVDiNrZXhbYMwEEafrSxAR2AFOkI6Ah0hmcBihuAJwgh0hGaEsEJHgA3s/DCoBIxt0n4SEtadH3dn3yGstXilZQacgArIZpYG+EKZm2+b3IAdgR4ogHeUESgjgDNQAt9oWfm2ilWEWhbAHRhG2M/CfgKu4+qMMk0swunLU8rPcoBh4RsEzutVeOzMgDla5jFgs/GepHUNU6Rlj8tkQJm3ucl/ymFYyW9ZLn+L0N3NO5ADN5T5WLrsjbANwfYBtbwCR+CyBQPAWht/alHZWlhbiyrmmwrsbS3aFN94yq4VMzwn6lNKDd0VUab7L6ADuSsT1WudEtD+TonoEPXQssUN1Q74XM3HhcIRuqlcjqsC33zcBXxBMWADTD+jjoT5+AD4rb8zjOF5rwAAAABJRU5ErkJggg==";

		public static Texture2D Window = null;

		public static Texture2D SettingsNormal = null;

		public static Texture2D SettingsActive = null;

		public static Texture2D StatusActive = null;

		public static Texture2D StatusInactive = null;

		public static Texture2D StatusNeedRestart = null;

		public static Texture2D WWW = null;

		public static Texture2D Updates = null;

		public static Texture2D Question = null;

		public static void Init()
		{
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Expected O, but got Unknown
			//IL_0256: Expected O, but got Unknown
			FieldInfo[] source = (from x in typeof(Textures).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
				where x.FieldType == typeof(string)
				select x).ToArray();
			FieldInfo[] array = (from x in typeof(Textures).GetFields(BindingFlags.Static | BindingFlags.Public)
				where x.FieldType == typeof(Texture2D)
				select x).ToArray();
			FieldInfo[] array2 = array;
			foreach (FieldInfo fieldInfo in array2)
			{
				fieldInfo.SetValue(null, (object)new Texture2D(2, 2, (TextureFormat)5, false, true));
			}
			if (unityVersion.Major >= 2017)
			{
				Assembly assembly = Assembly.Load("UnityEngine.ImageConversionModule");
				MethodInfo method = assembly.GetType("UnityEngine.ImageConversion").GetMethod("LoadImage", new Type[2]
				{
					typeof(Texture2D),
					typeof(byte[])
				});
				if (method != null)
				{
					FieldInfo[] array3 = array;
					foreach (FieldInfo f in array3)
					{
						method.Invoke(null, new object[2]
						{
							(object)(Texture2D)f.GetValue(null),
							Convert.FromBase64String(((string)source.FirstOrDefault((FieldInfo x) => x.Name == f.Name + "Base64")?.GetValue(null)) ?? "")
						});
					}
				}
			}
			else
			{
				MethodInfo method2 = typeof(Texture2D).GetMethod("LoadImage", new Type[1] { typeof(byte[]) });
				if (method2 != null)
				{
					FieldInfo[] array4 = array;
					foreach (FieldInfo f in array4)
					{
						method2.Invoke((object)(Texture2D)f.GetValue(null), new object[1] { Convert.FromBase64String(((string)source.FirstOrDefault((FieldInfo x) => x.Name == f.Name + "Base64")?.GetValue(null)) ?? "") });
					}
				}
			}
			int size = 128;
			SettingsNormal.ResizeToIfLess(size);
			SettingsActive.ResizeToIfLess(size);
			WWW.ResizeToIfLess(size);
			Updates.ResizeToIfLess(size);
		}
	}

	public class UI : MonoBehaviour
	{
		private class PopupToggleGroup_GUI
		{
			internal static readonly List<PopupToggleGroup_GUI> mList = new List<PopupToggleGroup_GUI>();

			internal readonly HashSet<int> mDestroyCounter = new HashSet<int>();

			private const int MARGIN = 100;

			private int mId;

			private Rect mWindowRect;

			private Vector2 mScrollPosition;

			private int mWidth;

			private int mHeight;

			private int mRecalculateFrame;

			private bool mOpened;

			public int? newSelected;

			public int selected;

			public readonly string[] values;

			public string title;

			public int unique;

			private bool Recalculating => mRecalculateFrame == Time.frameCount;

			public bool Opened
			{
				get
				{
					return mOpened;
				}
				set
				{
					mOpened = value;
					if (value)
					{
						Reset();
					}
				}
			}

			public PopupToggleGroup_GUI(string[] values)
			{
				mId = GetNextWindowId();
				mList.Add(this);
				this.values = values;
			}

			public void Button(string text = null, GUIStyle style = null, params GUILayoutOption[] option)
			{
				mDestroyCounter.Clear();
				if (!GUILayout.Button(text ?? values[selected], style ?? GUI.skin.button, option))
				{
					return;
				}
				if (!Opened)
				{
					foreach (PopupToggleGroup_GUI m in mList)
					{
						m.Opened = false;
					}
					Opened = true;
				}
				else
				{
					Opened = false;
				}
			}

			public void Render()
			{
				//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
				//IL_0100: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Expected O, but got Unknown
				//IL_0136: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Expected O, but got Unknown
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				if (Recalculating)
				{
					mWindowRect = GUILayout.Window(mId, mWindowRect, new WindowFunction(WindowFunction), "", window, Array.Empty<GUILayoutOption>());
					if (((Rect)(ref mWindowRect)).width > 0f)
					{
						mWidth = (int)Math.Min(Math.Max(((Rect)(ref mWindowRect)).width, 250f), Screen.width - 100);
						mHeight = (int)Math.Min(((Rect)(ref mWindowRect)).height, Screen.height - 100);
						((Rect)(ref mWindowRect)).x = Math.Max(Screen.width - mWidth, 0) / 2;
						((Rect)(ref mWindowRect)).y = Math.Max(Screen.height - mHeight, 0) / 2;
					}
				}
				else
				{
					mWindowRect = GUILayout.Window(mId, mWindowRect, new WindowFunction(WindowFunction), "", window, (GUILayoutOption[])(object)new GUILayoutOption[2]
					{
						GUILayout.Width((float)mWidth),
						GUILayout.Height((float)(mHeight + 20))
					});
					GUI.BringWindowToFront(mId);
				}
			}

			private void WindowFunction(int windowId)
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				if (title != null)
				{
					GUILayout.Label(title, h1, Array.Empty<GUILayoutOption>());
				}
				if (!Recalculating)
				{
					mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, Array.Empty<GUILayoutOption>());
				}
				if (values != null)
				{
					int num = 0;
					string[] array = values;
					foreach (string text in array)
					{
						if (GUILayout.Button((num == selected) ? ("<b>" + text + "</b>") : text, Array.Empty<GUILayoutOption>()))
						{
							newSelected = num;
							Opened = false;
						}
						num++;
					}
				}
				if (!Recalculating)
				{
					GUILayout.EndScrollView();
				}
			}

			internal void Reset()
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				mRecalculateFrame = Time.frameCount;
				mWindowRect = new Rect(-9000f, 0f, 0f, 0f);
			}
		}

		private class Column
		{
			public string name;

			public float width;

			public bool expand = false;

			public bool skip = false;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Func<Color32, Color32> _003C_003E9__47_0;

			public static Action<ModEntry> _003C_003E9__60_0;

			public static Action<ModEntry> _003C_003E9__60_1;

			public static UnityAction _003C_003E9__66_0;

			public static Func<Column, bool> _003C_003E9__68_2;

			public static Func<Column, float> _003C_003E9__68_3;

			public static Func<Column, bool> _003C_003E9__68_4;

			public static Func<Column, float> _003C_003E9__68_5;

			public static Action<int> _003C_003E9__68_0;

			public static Action<int> _003C_003E9__68_1;

			public static Func<ModEntry, bool> _003C_003E9__80_0;

			public static Converter<object, float> _003C_003E9__118_6;

			public static Converter<object, int> _003C_003E9__118_7;

			public static Converter<object, long> _003C_003E9__118_8;

			public static Converter<object, double> _003C_003E9__118_9;

			internal Color32 _003CPrepareGUI_003Eb__47_0(Color32 x)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				((Color32)(ref x))._002Ector((byte)30, (byte)30, (byte)30, byte.MaxValue);
				return x;
			}

			internal void _003Cset_ShowModSettings_003Eb__60_0(ModEntry mod)
			{
				if (mod.Active && mod.OnHideGUI != null && mod.OnGUI != null)
				{
					try
					{
						mod.OnHideGUI(mod);
					}
					catch (ExitGUIException)
					{
					}
					catch (Exception e)
					{
						mod.Logger.LogException("OnHideGUI", e);
					}
				}
			}

			internal void _003Cset_ShowModSettings_003Eb__60_1(ModEntry mod)
			{
				if (mod.Active && mod.OnShowGUI != null && mod.OnGUI != null)
				{
					try
					{
						mod.OnShowGUI(mod);
					}
					catch (ExitGUIException)
					{
					}
					catch (Exception e)
					{
						mod.Logger.LogException("OnShowGUI", e);
					}
				}
			}

			internal void _003CWindowFunction_003Eb__66_0()
			{
			}

			internal bool _003CDrawTab_003Eb__68_2(Column x)
			{
				return !x.skip;
			}

			internal float _003CDrawTab_003Eb__68_3(Column x)
			{
				return x.width;
			}

			internal bool _003CDrawTab_003Eb__68_4(Column x)
			{
				return x.expand && !x.skip;
			}

			internal float _003CDrawTab_003Eb__68_5(Column x)
			{
				return x.width;
			}

			internal void _003CDrawTab_003Eb__68_0(int i)
			{
				Params.CheckUpdates = i;
			}

			internal void _003CDrawTab_003Eb__68_1(int i)
			{
				Params.ShowOnStart = i;
			}

			internal bool _003CFirstLaunch_003Eb__80_0(ModEntry x)
			{
				return !x.ErrorOnLoading;
			}

			internal float _003CDraw_003Eb__118_6(object x)
			{
				return (float)x;
			}

			internal int _003CDraw_003Eb__118_7(object x)
			{
				return (int)x;
			}

			internal long _003CDraw_003Eb__118_8(object x)
			{
				return (long)x;
			}

			internal double _003CDraw_003Eb__118_9(object x)
			{
				return (double)x;
			}
		}

		private static UI mInstance = null;

		public static GUIStyle window = null;

		public static GUIStyle h1 = null;

		public static GUIStyle h2 = null;

		public static GUIStyle bold = null;

		public static GUIStyle button = null;

		private static GUIStyle settings = null;

		private static GUIStyle status = null;

		private static GUIStyle www = null;

		private static GUIStyle updates = null;

		private static GUIStyle question = null;

		private static GUIStyle tooltipBox = null;

		private bool mFirstLaunched = false;

		private bool mInit = false;

		private bool mOpened = false;

		private Rect mWindowRect = new Rect(0f, 0f, 0f, 0f);

		private Vector2 mWindowSize = Vector2.zero;

		private Vector2 mExpectedWindowSize = Vector2.zero;

		private Resolution mCurrentResolution;

		private GUIContent mTooltip = null;

		private float mUIScale = 1f;

		private float mExpectedUIScale = 1f;

		private bool mUIScaleChanged;

		private string[] mOSfonts = null;

		private string mDefaultFont = "Arial";

		private int mSelectedFont;

		public int globalFontSize = 13;

		public int tabId = 0;

		public string[] tabs = new string[3] { "Mods", "Logs", "Settings" };

		private List<Column> mOriginColumns = new List<Column>
		{
			new Column
			{
				name = "Name",
				width = 200f,
				expand = true
			},
			new Column
			{
				name = "Version",
				width = 60f
			},
			new Column
			{
				name = "Requirements",
				width = 150f,
				expand = true
			},
			new Column
			{
				name = "On/Off",
				width = 50f
			},
			new Column
			{
				name = "Status",
				width = 50f
			}
		};

		private List<Column> mColumns = new List<Column>();

		private Vector2[] mScrollPosition = (Vector2[])(object)new Vector2[0];

		private int mPreviousShowModSettings = -1;

		private int mShowModSettings = -1;

		private static List<string> mJoinList = new List<string>();

		private static string[] mCheckUpdateStrings = new string[2] { "Never", "Automatic" };

		private static string[] mShowOnStartStrings = new string[2] { "No", "Yes" };

		private static string[] mHotkeyNames = new string[4] { "CTRL+F10", "ScrollLock", "Num *", "~" };

		private GameObject mCanvas = null;

		private static int mLastWindowId = 0;

		private static Type[] fieldTypes = new Type[13]
		{
			typeof(int),
			typeof(long),
			typeof(float),
			typeof(double),
			typeof(int[]),
			typeof(long[]),
			typeof(float[]),
			typeof(double[]),
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Color),
			typeof(string)
		};

		private static Type[] sliderTypes = new Type[4]
		{
			typeof(int),
			typeof(long),
			typeof(float),
			typeof(double)
		};

		private static Type[] toggleTypes = new Type[1] { typeof(bool) };

		private static Type[] specialTypes = new Type[5]
		{
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Color),
			typeof(KeyBinding)
		};

		private static float drawHeight = 22f;

		private static List<int> collapsibleStates = new List<int>();

		public static UI Instance => mInstance;

		public bool Opened => mOpened;

		private int ShowModSettings
		{
			get
			{
				return mShowModSettings;
			}
			set
			{
				Action<ModEntry> action = delegate(ModEntry mod)
				{
					if (mod.Active && mod.OnHideGUI != null && mod.OnGUI != null)
					{
						try
						{
							mod.OnHideGUI(mod);
						}
						catch (ExitGUIException)
						{
						}
						catch (Exception e2)
						{
							mod.Logger.LogException("OnHideGUI", e2);
						}
					}
				};
				Action<ModEntry> action2 = delegate(ModEntry mod)
				{
					if (mod.Active && mod.OnShowGUI != null && mod.OnGUI != null)
					{
						try
						{
							mod.OnShowGUI(mod);
						}
						catch (ExitGUIException)
						{
						}
						catch (Exception e)
						{
							mod.Logger.LogException("OnShowGUI", e);
						}
					}
				};
				mShowModSettings = value;
				if (mShowModSettings != mPreviousShowModSettings)
				{
					if (mShowModSettings == -1)
					{
						action(ModEntries[mPreviousShowModSettings]);
					}
					else if (mPreviousShowModSettings == -1)
					{
						action2(ModEntries[mShowModSettings]);
					}
					else
					{
						action(ModEntries[mPreviousShowModSettings]);
						action2(ModEntries[mShowModSettings]);
					}
					mPreviousShowModSettings = mShowModSettings;
				}
			}
		}

		internal bool GameCursorVisible { get; set; }

		internal CursorLockMode GameCursorLockMode { get; set; }

		public static bool PopupToggleGroup(ref int selected, string[] values, GUIStyle style = null, params GUILayoutOption[] buttonOption)
		{
			bool changed = false;
			int newSelected = selected;
			PopupToggleGroup(selected, values, delegate(int i)
			{
				changed = true;
				newSelected = i;
			}, style, buttonOption);
			selected = newSelected;
			return changed;
		}

		public static bool PopupToggleGroup(ref int selected, string[] values, string title, GUIStyle style = null, params GUILayoutOption[] buttonOption)
		{
			bool changed = false;
			int newSelected = selected;
			PopupToggleGroup(selected, values, delegate(int i)
			{
				changed = true;
				newSelected = i;
			}, title, style, buttonOption);
			selected = newSelected;
			return changed;
		}

		public static bool PopupToggleGroup(ref int selected, string[] values, string title, int unique, GUIStyle style = null, params GUILayoutOption[] buttonOption)
		{
			bool changed = false;
			int newSelected = selected;
			PopupToggleGroup(selected, values, delegate(int i)
			{
				changed = true;
				newSelected = i;
			}, title, unique, style, buttonOption);
			selected = newSelected;
			return changed;
		}

		public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] buttonOption)
		{
			PopupToggleGroup(selected, values, onChange, null, style, buttonOption);
		}

		public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, string title, GUIStyle style = null, params GUILayoutOption[] buttonOption)
		{
			PopupToggleGroup(selected, values, onChange, title, 0, style, buttonOption);
		}

		public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, string title, int unique, GUIStyle style = null, params GUILayoutOption[] buttonOption)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (values.Length == 0)
			{
				throw new IndexOutOfRangeException();
			}
			bool flag = false;
			if (selected >= values.Length)
			{
				selected = values.Length - 1;
				flag = true;
			}
			else if (selected < 0)
			{
				selected = 0;
				flag = true;
			}
			PopupToggleGroup_GUI popupToggleGroup_GUI = null;
			foreach (PopupToggleGroup_GUI m in PopupToggleGroup_GUI.mList)
			{
				if ((unique == 0 && m.title == title && m.values.SequenceEqual(values)) || (unique != 0 && m.unique == unique && m.title == title))
				{
					popupToggleGroup_GUI = m;
					break;
				}
			}
			if (popupToggleGroup_GUI == null)
			{
				popupToggleGroup_GUI = new PopupToggleGroup_GUI(values);
				popupToggleGroup_GUI.title = title;
				popupToggleGroup_GUI.unique = unique;
			}
			if (popupToggleGroup_GUI.newSelected.HasValue && selected != popupToggleGroup_GUI.newSelected.Value && popupToggleGroup_GUI.newSelected.Value < values.Length)
			{
				selected = popupToggleGroup_GUI.newSelected.Value;
				flag = true;
			}
			popupToggleGroup_GUI.selected = selected;
			popupToggleGroup_GUI.newSelected = null;
			popupToggleGroup_GUI.Button(null, style, buttonOption);
			if (flag)
			{
				try
				{
					onChange(selected);
				}
				catch (Exception ex)
				{
					PFLog.UnityModManager.Error("PopupToggleGroup: " + ex.GetType()?.ToString() + " - " + ex.Message);
				}
			}
		}

		public static bool ToggleGroup(ref int selected, string[] values, GUIStyle style = null, params GUILayoutOption[] option)
		{
			bool changed = false;
			int newSelected = selected;
			ToggleGroup(selected, values, delegate(int i)
			{
				changed = true;
				newSelected = i;
			}, style, option);
			selected = newSelected;
			return changed;
		}

		public static void ToggleGroup(int selected, string[] values, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (values.Length == 0)
			{
				throw new IndexOutOfRangeException();
			}
			bool flag = false;
			if (selected >= values.Length)
			{
				selected = values.Length - 1;
				flag = true;
			}
			else if (selected < 0)
			{
				selected = 0;
				flag = true;
			}
			int num = 0;
			foreach (string text in values)
			{
				bool flag2 = selected == num;
				if (GUILayout.Toggle(flag2, text, style ?? GUI.skin.toggle, option) && !flag2)
				{
					selected = num;
					flag = true;
				}
				num++;
			}
			if (flag)
			{
				try
				{
					onChange(selected);
				}
				catch (Exception ex)
				{
					PFLog.UnityModManager.Error("ToggleGroup: " + ex.GetType()?.ToString() + " - " + ex.Message);
				}
			}
		}

		internal static bool Load()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				new GameObject(typeof(UI).FullName, new Type[1] { typeof(UI) });
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return false;
		}

		private void Awake()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Expected O, but got Unknown
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Expected O, but got Unknown
			PFLog.UnityModManager.Log("Spawning OwlcatUnityModManager.");
			mInstance = this;
			Object.DontDestroyOnLoad((Object)(object)this);
			mWindowSize = ClampWindowSize(new Vector2(Params.WindowWidth, Params.WindowHeight));
			mExpectedWindowSize = mWindowSize;
			mUIScale = Mathf.Clamp(Params.UIScale, 0.5f, 5f);
			mExpectedUIScale = mUIScale;
			Textures.Init();
			mOSfonts = Font.GetOSInstalledFontNames();
			if (mOSfonts.Length == 0)
			{
				PFLog.UnityModManager.Error("No compatible font found in OS. If you play through Wine, install winetricks allfonts.");
				OpenUnityFileLog();
			}
			else
			{
				if (string.IsNullOrEmpty(Params.UIFont))
				{
					Params.UIFont = mDefaultFont;
				}
				if (!mOSfonts.Contains(Params.UIFont))
				{
					Params.UIFont = mOSfonts.First();
				}
				mSelectedFont = Array.IndexOf(mOSfonts, Params.UIFont);
			}
			Harmony val = new Harmony("UnityModManager.UI");
			MethodInfo method = typeof(Screen).GetMethod("set_lockCursor");
			MethodInfo method2 = typeof(Screen_lockCursor_Patch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
			val.Patch((MethodBase)method, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}

		private void Start()
		{
			CalculateWindowPos();
			FirstLaunch();
			if (Params.CheckUpdates == 1)
			{
				CheckModUpdates();
			}
		}

		private void OnDestroy()
		{
			PFLog.UnityModManager.Log("OwlcatUnityModManager: Destroying when exit.");
			SaveSettingsAndParams();
		}

		private void Update()
		{
			if (Opened)
			{
				Cursor.lockState = (CursorLockMode)0;
				Cursor.visible = true;
			}
			try
			{
				KeyBinding.BindKeyboard();
			}
			catch (Exception ex)
			{
				PFLog.UnityModManager.Exception(ex, "BindKeyboard");
			}
			float deltaTime = Time.deltaTime;
			foreach (ModEntry modEntry in ModEntries)
			{
				if (modEntry.Active && modEntry.OnUpdate != null)
				{
					try
					{
						modEntry.OnUpdate(modEntry, deltaTime);
					}
					catch (Exception e)
					{
						modEntry.Logger.LogException("OnUpdate", e);
					}
				}
			}
			if (Params.Hotkey.Up() || Param.DefaultHotkey.Up())
			{
				ToggleWindow();
			}
			if (mOpened && Param.EscapeHotkey.Up())
			{
				ToggleWindow();
			}
		}

		private void FixedUpdate()
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			foreach (ModEntry modEntry in ModEntries)
			{
				if (modEntry.Active && modEntry.OnFixedUpdate != null)
				{
					try
					{
						modEntry.OnFixedUpdate(modEntry, fixedDeltaTime);
					}
					catch (Exception e)
					{
						modEntry.Logger.LogException("OnFixedUpdate", e);
					}
				}
			}
		}

		private void LateUpdate()
		{
			float deltaTime = Time.deltaTime;
			foreach (ModEntry modEntry in ModEntries)
			{
				if (modEntry.Active && modEntry.OnLateUpdate != null)
				{
					try
					{
						modEntry.OnLateUpdate(modEntry, deltaTime);
					}
					catch (Exception e)
					{
						modEntry.Logger.LogException("OnLateUpdate", e);
					}
				}
			}
		}

		private void PrepareGUI()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Expected O, but got Unknown
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Expected O, but got Unknown
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Expected O, but got Unknown
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Expected O, but got Unknown
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Expected O, but got Unknown
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Expected O, but got Unknown
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Expected O, but got Unknown
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Expected O, but got Unknown
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Expected O, but got Unknown
			window = new GUIStyle();
			window.name = "umm window";
			window.normal.background = Textures.Window;
			((Texture)window.normal.background).wrapMode = (TextureWrapMode)0;
			h1 = new GUIStyle();
			h1.name = "umm h1";
			h1.normal.textColor = Color.white;
			h1.fontStyle = (FontStyle)1;
			h1.alignment = (TextAnchor)4;
			h2 = new GUIStyle();
			h2.name = "umm h2";
			h2.normal.textColor = new Color(0.6f, 0.91f, 1f);
			h2.fontStyle = (FontStyle)1;
			bold = new GUIStyle(GUI.skin.label);
			bold.name = "umm bold";
			bold.normal.textColor = Color.white;
			bold.fontStyle = (FontStyle)1;
			button = new GUIStyle(GUI.skin.button);
			button.name = "umm button";
			settings = new GUIStyle();
			settings.alignment = (TextAnchor)4;
			settings.stretchHeight = true;
			status = new GUIStyle();
			status.alignment = (TextAnchor)4;
			status.stretchHeight = true;
			www = new GUIStyle();
			www.alignment = (TextAnchor)4;
			www.stretchHeight = true;
			updates = new GUIStyle();
			updates.alignment = (TextAnchor)4;
			updates.stretchHeight = true;
			question = new GUIStyle();
			tooltipBox = new GUIStyle();
			tooltipBox.alignment = (TextAnchor)4;
			tooltipBox.normal.textColor = Color.white;
			tooltipBox.normal.background = new Texture2D(2, 2, (TextureFormat)4, false);
			tooltipBox.normal.background.SetPixels32(((IEnumerable<Color32>)(object)new Color32[4]).Select(delegate(Color32 x)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				((Color32)(ref x))._002Ector((byte)30, (byte)30, (byte)30, byte.MaxValue);
				return x;
			}).ToArray());
			tooltipBox.normal.background.Apply();
			tooltipBox.hover = tooltipBox.normal;
			tooltipBox.border.left = 2;
			tooltipBox.border.right = 2;
			tooltipBox.border.top = 2;
			tooltipBox.border.bottom = 2;
			tooltipBox.richText = true;
		}

		private void ScaleGUI()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			GUI.skin.font = Font.CreateDynamicFontFromOSFont(Params.UIFont, Scale(globalFontSize));
			GUI.skin.label.clipping = (TextClipping)0;
			GUI.skin.button.padding = new RectOffset(Scale(10), Scale(10), Scale(3), Scale(3));
			GUI.skin.horizontalSlider.fixedHeight = Scale(12);
			GUI.skin.horizontalSlider.border = RectOffset(3, 0);
			GUI.skin.horizontalSlider.padding = RectOffset(Scale(-1), 0);
			GUI.skin.horizontalSlider.margin = RectOffset(Scale(4), Scale(8));
			GUI.skin.horizontalSliderThumb.fixedHeight = Scale(12);
			GUI.skin.horizontalSliderThumb.border = RectOffset(4, 0);
			GUI.skin.horizontalSliderThumb.padding = RectOffset(Scale(7), 0);
			GUI.skin.horizontalSliderThumb.margin = RectOffset(0);
			GUI.skin.toggle.margin.left = Scale(10);
			window.padding = RectOffset(Scale(5));
			h1.fontSize = Scale(16);
			h1.margin = RectOffset(0, Scale(5));
			h2.fontSize = Scale(13);
			h2.margin = RectOffset(0, Scale(3));
			button.fontSize = Scale(13);
			button.padding = RectOffset(Scale(30), Scale(5));
			int value = 28;
			settings.fixedWidth = Scale(24);
			settings.fixedHeight = Scale(value);
			status.fixedWidth = Scale(12);
			status.fixedHeight = Scale(value);
			www.fixedWidth = Scale(24);
			www.fixedHeight = Scale(value);
			updates.fixedWidth = Scale(26);
			updates.fixedHeight = Scale(value);
			question.fixedWidth = Scale(10);
			question.fixedHeight = Scale(11);
			question.margin = RectOffset(0, 9);
			mColumns.Clear();
			foreach (Column mOriginColumn in mOriginColumns)
			{
				mColumns.Add(new Column
				{
					name = mOriginColumn.name,
					width = Scale(mOriginColumn.width),
					expand = mOriginColumn.expand,
					skip = mOriginColumn.skip
				});
			}
		}

		private void OnGUI()
		{
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Expected O, but got Unknown
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			if (!mInit)
			{
				mInit = true;
				PrepareGUI();
				ScaleGUI();
			}
			List<PopupToggleGroup_GUI> list = new List<PopupToggleGroup_GUI>(0);
			bool flag = false;
			foreach (PopupToggleGroup_GUI m in PopupToggleGroup_GUI.mList)
			{
				m.mDestroyCounter.Add(Time.frameCount);
				if (m.mDestroyCounter.Count > 1)
				{
					list.Add(m);
				}
				else if (m.Opened && !flag)
				{
					m.Render();
					flag = true;
				}
			}
			foreach (PopupToggleGroup_GUI item in list)
			{
				PopupToggleGroup_GUI.mList.Remove(item);
			}
			if (mOpened)
			{
				int width = ((Resolution)(ref mCurrentResolution)).width;
				Resolution currentResolution = Screen.currentResolution;
				if (width == ((Resolution)(ref currentResolution)).width)
				{
					int height = ((Resolution)(ref mCurrentResolution)).height;
					currentResolution = Screen.currentResolution;
					if (height == ((Resolution)(ref currentResolution)).height)
					{
						goto IL_0159;
					}
				}
				mCurrentResolution = Screen.currentResolution;
				CalculateWindowPos();
				goto IL_0159;
			}
			goto IL_01eb;
			IL_01eb:
			foreach (ModEntry modEntry in ModEntries)
			{
				if (modEntry.Active && modEntry.OnFixedGUI != null)
				{
					try
					{
						modEntry.OnFixedGUI(modEntry);
					}
					catch (Exception e)
					{
						modEntry.Logger.LogException("OnFixedGUI", e);
					}
				}
			}
			return;
			IL_0159:
			if (mUIScaleChanged)
			{
				mUIScaleChanged = false;
				ScaleGUI();
			}
			Color backgroundColor = GUI.backgroundColor;
			Color color = GUI.color;
			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;
			mWindowRect = GUILayout.Window(0, mWindowRect, new WindowFunction(WindowFunction), "", window, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(mWindowSize.y) });
			GUI.backgroundColor = backgroundColor;
			GUI.color = color;
			goto IL_01eb;
		}

		public void TryShowModSettingsUI(int i)
		{
			ShowModSettings = i;
		}

		public static int Scale(int value)
		{
			if (!Object.op_Implicit((Object)(object)Instance))
			{
				return value;
			}
			return (int)((float)value * Instance.mUIScale);
		}

		public static float Scale(float value)
		{
			if (!Object.op_Implicit((Object)(object)Instance))
			{
				return value;
			}
			return value * Instance.mUIScale;
		}

		private void CalculateWindowPos()
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			mWindowSize = ClampWindowSize(mWindowSize);
			mWindowRect = new Rect(((float)Screen.width - mWindowSize.x) / 2f, ((float)Screen.height - mWindowSize.y) / 2f, 0f, 0f);
		}

		private Vector2 ClampWindowSize(Vector2 orig)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(Mathf.Clamp(orig.x, (float)Mathf.Min(960, Screen.width), (float)Screen.width), Mathf.Clamp(orig.y, (float)Mathf.Min(720, Screen.height), (float)Screen.height));
		}

		private void WindowFunction(int windowId)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Invalid comparison between Unknown and I4
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_027b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0255: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.current.type == 7)
			{
				mTooltip = null;
			}
			if (KeyBinding.Ctrl())
			{
				GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
			}
			GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
			object obj = _003C_003Ec._003C_003E9__66_0;
			if (obj == null)
			{
				UnityAction val = delegate
				{
				};
				_003C_003Ec._003C_003E9__66_0 = val;
				obj = (object)val;
			}
			UnityAction buttons = (UnityAction)obj;
			GUILayout.Label("Mod Manager " + version, h1, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			int num = tabId;
			num = GUILayout.Toolbar(num, tabs, button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (num != tabId)
			{
				tabId = num;
			}
			GUILayout.Space(5f);
			if (mScrollPosition.Length != tabs.Length)
			{
				mScrollPosition = (Vector2[])(object)new Vector2[tabs.Length];
			}
			DrawTab(tabId, ref buttons);
			GUILayout.FlexibleSpace();
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("Close", button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
			{
				ToggleWindow();
			}
			if (GUILayout.Button("Save", button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
			{
				SaveSettingsAndParams();
			}
			buttons.Invoke();
			GUILayout.EndHorizontal();
			if (mTooltip != null && (int)Event.current.type == 7)
			{
				Vector2 val2 = tooltipBox.CalcSize(mTooltip) + Vector2.one * 10f;
				Vector2 mousePosition = Event.current.mousePosition;
				if (val2.x + mousePosition.x < ((Rect)(ref mWindowRect)).width)
				{
					GUI.Box(new Rect(mousePosition.x, mousePosition.y + 10f, val2.x, val2.y), mTooltip.text, tooltipBox);
				}
				else
				{
					GUI.Box(new Rect(mousePosition.x - val2.x, mousePosition.y + 10f, val2.x, val2.y), mTooltip.text, tooltipBox);
				}
			}
			else
			{
				GUI.Box(new Rect(-9999f, 0f, 0f, 0f), "");
			}
		}

		private void DrawTab(int tabId, ref UnityAction buttons)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b8d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b9c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ba1: Unknown result type (might be due to invalid IL or missing references)
			GUILayoutOption val = GUILayout.MinWidth(mWindowSize.x);
			string text = tabs[tabId];
			string text2 = text;
			if (!(text2 == "Mods"))
			{
				if (text2 == "Settings")
				{
					mScrollPosition[tabId] = GUILayout.BeginScrollView(mScrollPosition[tabId], (GUILayoutOption[])(object)new GUILayoutOption[1] { val });
					GUILayout.BeginVertical(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Hotkey (default Ctrl+F10)", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					DrawKeybinding(ref Params.Hotkey, "UMMHotkey", null, GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();
					GUILayout.Space(5f);
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Check updates", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					ToggleGroup(Params.CheckUpdates, mCheckUpdateStrings, delegate(int i)
					{
						Params.CheckUpdates = i;
					}, null, GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();
					GUILayout.Space(5f);
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Show this window on startup", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					ToggleGroup(Params.ShowOnStart, mShowOnStartStrings, delegate(int i)
					{
						Params.ShowOnStart = i;
					}, null, GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();
					GUILayout.Space(5f);
					GUILayout.BeginVertical(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
					GUILayout.Label("Window size", bold, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Width ", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					mExpectedWindowSize.x = GUILayout.HorizontalSlider(mExpectedWindowSize.x, (float)Mathf.Min(Screen.width, 960), (float)Screen.width, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
					GUILayout.Label(" " + mExpectedWindowSize.x.ToString("f0") + " px ", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Height", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					mExpectedWindowSize.y = GUILayout.HorizontalSlider(mExpectedWindowSize.y, (float)Mathf.Min(Screen.height, 720), (float)Screen.height, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
					GUILayout.Label(" " + mExpectedWindowSize.y.ToString("f0") + " px ", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					GUILayout.EndHorizontal();
					if (GUILayout.Button("Apply", button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
					{
						mWindowSize.x = ((Mathf.Floor(mExpectedWindowSize.x) % 2f > 0f) ? Mathf.Ceil(mExpectedWindowSize.x) : Mathf.Floor(mExpectedWindowSize.x));
						mWindowSize.y = ((Mathf.Floor(mExpectedWindowSize.y) % 2f > 0f) ? Mathf.Ceil(mExpectedWindowSize.y) : Mathf.Floor(mExpectedWindowSize.y));
						CalculateWindowPos();
						Params.WindowWidth = mWindowSize.x;
						Params.WindowHeight = mWindowSize.y;
					}
					GUILayout.EndVertical();
					GUILayout.Space(5f);
					GUILayout.BeginVertical(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
					GUILayout.Label("UI", bold, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					GUILayout.Label("Font", (GUILayoutOption[])(object)new GUILayoutOption[2]
					{
						GUILayout.ExpandWidth(false),
						GUILayout.ExpandWidth(false)
					});
					PopupToggleGroup(ref mSelectedFont, mOSfonts, null, GUI.skin.button, GUILayout.Width(200f));
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Scale", (GUILayoutOption[])(object)new GUILayoutOption[2]
					{
						GUILayout.ExpandWidth(false),
						GUILayout.ExpandWidth(false)
					});
					mExpectedUIScale = GUILayout.HorizontalSlider(mExpectedUIScale, 0.5f, 5f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
					GUILayout.Label(" " + mExpectedUIScale.ToString("f2"), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					GUILayout.EndHorizontal();
					if (GUILayout.Button("Apply", button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }) && (mUIScale != mExpectedUIScale || mOSfonts[mSelectedFont] != Params.UIFont))
					{
						mUIScaleChanged = true;
						mUIScale = mExpectedUIScale;
						Params.UIScale = mUIScale;
						Params.UIFont = mOSfonts[mSelectedFont];
					}
					GUILayout.EndVertical();
					GUILayout.EndVertical();
					GUILayout.EndScrollView();
				}
				return;
			}
			mScrollPosition[tabId] = GUILayout.BeginScrollView(mScrollPosition[tabId], (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				val,
				GUILayout.ExpandHeight(false)
			});
			float amountWidth = mColumns.Where((Column x) => !x.skip).Sum((Column x) => x.width);
			float expandWidth = mColumns.Where((Column x) => x.expand && !x.skip).Sum((Column x) => x.width);
			List<ModEntry> modEntries = ModEntries;
			GUILayoutOption[] array = mColumns.Select((Column x) => x.expand ? GUILayout.Width(x.width / expandWidth * (mWindowSize.x - 60f + expandWidth - amountWidth)) : GUILayout.Width(x.width)).ToArray();
			GUILayout.BeginVertical(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
			GUILayout.BeginHorizontal(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
			for (int j = 0; j < mColumns.Count; j++)
			{
				if (!mColumns[j].skip)
				{
					GUILayout.Label(mColumns[j].name, (GUILayoutOption[])(object)new GUILayoutOption[1] { array[j] });
				}
			}
			GUILayout.EndHorizontal();
			int k = 0;
			for (int count = modEntries.Count; k < count; k++)
			{
				int num = -1;
				GUILayout.BeginVertical(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				if (modEntries[k].OnGUI != null || modEntries[k].CanReload)
				{
					if (GUILayout.Button(modEntries[k].Info.DisplayName, GUI.skin.label, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) }))
					{
						ShowModSettings = ((ShowModSettings == k) ? (-1) : k);
					}
					if (GUILayout.Button((Texture)(object)((ShowModSettings == k) ? Textures.SettingsActive : Textures.SettingsNormal), settings, Array.Empty<GUILayoutOption>()))
					{
						ShowModSettings = ((ShowModSettings == k) ? (-1) : k);
					}
				}
				else
				{
					GUILayout.Label(modEntries[k].Info.DisplayName, Array.Empty<GUILayoutOption>());
				}
				if (!string.IsNullOrEmpty(modEntries[k].Info.HomePage))
				{
					GUILayout.Space(10f);
					if (GUILayout.Button((Texture)(object)Textures.WWW, www, Array.Empty<GUILayoutOption>()))
					{
						Application.OpenURL(modEntries[k].Info.HomePage);
					}
				}
				if (modEntries[k].NewestVersion != null)
				{
					GUILayout.Space(10f);
					GUILayout.Box((Texture)(object)Textures.Updates, updates, Array.Empty<GUILayoutOption>());
				}
				GUILayout.Space(20f);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				GUILayout.Label(modEntries[k].Info.Version, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
				GUILayout.EndHorizontal();
				if (modEntries[k].ManagerVersion > GetVersion())
				{
					GUILayout.Label("<color=\"#CD5C5C\">Manager-" + modEntries[k].Info.ManagerVersion + "</color>", (GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				}
				else if (gameVersion != VER_0 && modEntries[k].GameVersion > gameVersion)
				{
					GUILayout.Label("<color=\"#CD5C5C\">Game-" + modEntries[k].Info.GameVersion + "</color>", (GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				}
				else if (modEntries[k].Requirements.Count > 0)
				{
					GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
					mJoinList.Clear();
					foreach (KeyValuePair<string, Version> requirement in modEntries[k].Requirements)
					{
						string key = requirement.Key;
						ModEntry modEntry = FindMod(key);
						mJoinList.Add(((modEntry == null || (requirement.Value != null && requirement.Value > modEntry.Version) || !modEntry.Active) && modEntries[k].Active) ? ("<color=\"#CD5C5C\">" + key + "</color> ") : key);
					}
					GUILayout.Label(string.Join(", ", mJoinList.ToArray()), Array.Empty<GUILayoutOption>());
					GUILayout.EndHorizontal();
				}
				else if (!string.IsNullOrEmpty(modEntries[k].CustomRequirements))
				{
					GUILayout.Label(modEntries[k].CustomRequirements, (GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				}
				else
				{
					GUILayout.Label("-", (GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				}
				if (!forbidDisableMods)
				{
					bool enabled = modEntries[k].Enabled;
					enabled = GUILayout.Toggle(enabled, "", (GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
					if (enabled != modEntries[k].Enabled)
					{
						modEntries[k].Enabled = enabled;
						if (modEntries[k].Toggleable)
						{
							modEntries[k].Active = enabled;
						}
						else if (enabled && !modEntries[k].Loaded)
						{
							modEntries[k].Active = enabled;
						}
					}
				}
				else
				{
					GUILayout.Label("", (GUILayoutOption[])(object)new GUILayoutOption[1] { array[++num] });
				}
				if (modEntries[k].Active)
				{
					GUILayout.Box((Texture)(object)(modEntries[k].Enabled ? Textures.StatusActive : Textures.StatusNeedRestart), status, Array.Empty<GUILayoutOption>());
				}
				else
				{
					GUILayout.Box((Texture)(object)((!modEntries[k].Enabled) ? Textures.StatusInactive : Textures.StatusNeedRestart), status, Array.Empty<GUILayoutOption>());
				}
				if (modEntries[k].ErrorOnLoading)
				{
					GUILayout.Label("!!!", Array.Empty<GUILayoutOption>());
				}
				GUILayout.EndHorizontal();
				if (ShowModSettings == k)
				{
					if (modEntries[k].CanReload)
					{
						GUILayout.Label("Debug", h2, Array.Empty<GUILayoutOption>());
						if (GUILayout.Button("Reload", button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
						{
							modEntries[k].Reload();
						}
						GUILayout.Space(5f);
					}
					if (modEntries[k].Active && modEntries[k].OnGUI != null)
					{
						GUILayout.Label("Options", h2, Array.Empty<GUILayoutOption>());
						try
						{
							modEntries[k].OnGUI(modEntries[k]);
						}
						catch (Exception e)
						{
							modEntries[k].Logger.LogException("OnGUI", e);
							ShowModSettings = -1;
							GUIUtility.ExitGUI();
						}
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Space(10f);
			GUILayout.Box((Texture)(object)Textures.SettingsNormal, settings, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			GUILayout.Label("Options", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(15f);
			GUILayout.Box((Texture)(object)Textures.WWW, www, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			GUILayout.Label("Home page", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(15f);
			GUILayout.Box((Texture)(object)Textures.Updates, updates, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			GUILayout.Label("Available update", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(15f);
			GUILayout.Box((Texture)(object)Textures.StatusActive, status, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			GUILayout.Label("Active", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(10f);
			GUILayout.Box((Texture)(object)Textures.StatusInactive, status, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			GUILayout.Label("Inactive", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(10f);
			GUILayout.Box((Texture)(object)Textures.StatusNeedRestart, status, Array.Empty<GUILayoutOption>());
			GUILayout.Space(3f);
			GUILayout.Label("Need restart", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(10f);
			GUILayout.Label("!!!", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(3f);
			GUILayout.Label("Errors", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(10f);
			GUILayout.Label("[CTRL + LClick]", bold, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.Space(3f);
			GUILayout.Label("Drag window", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			GUILayout.EndHorizontal();
			if (!GUI.changed)
			{
			}
		}

		public void FirstLaunch()
		{
			if (!mFirstLaunched && (Params.ShowOnStart != 0 || !ModEntries.All((ModEntry x) => !x.ErrorOnLoading)))
			{
				ToggleWindow(open: true);
			}
		}

		public void ToggleWindow()
		{
			ToggleWindow(!mOpened);
		}

		public void ToggleWindow(bool open)
		{
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			if (open == mOpened)
			{
				return;
			}
			if (open)
			{
				mFirstLaunched = true;
			}
			if (!open)
			{
				int showModSettings = ShowModSettings;
				ShowModSettings = -1;
				mShowModSettings = showModSettings;
			}
			else
			{
				ShowModSettings = mShowModSettings;
			}
			try
			{
				mOpened = open;
				BlockGameUI(open);
				if (open)
				{
					GameCursorLockMode = Cursor.lockState;
					GameCursorVisible = Cursor.visible;
					Cursor.visible = true;
					Cursor.lockState = (CursorLockMode)0;
				}
				else
				{
					Cursor.visible = GameCursorVisible;
					Cursor.lockState = GameCursorLockMode;
				}
			}
			catch (Exception ex)
			{
				PFLog.UnityModManager.Exception(ex, "ToggleWindow");
			}
		}

		private void BlockGameUI(bool value)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Expected O, but got Unknown
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			if (value)
			{
				mCanvas = new GameObject("UMM blocking UI", new Type[2]
				{
					typeof(Canvas),
					typeof(GraphicRaycaster)
				});
				Canvas component = mCanvas.GetComponent<Canvas>();
				component.renderMode = (RenderMode)0;
				component.sortingOrder = 32767;
				Object.DontDestroyOnLoad((Object)(object)mCanvas);
				GameObject val = new GameObject("Image", new Type[1] { typeof(Image) });
				val.transform.SetParent(mCanvas.transform);
				RectTransform component2 = val.GetComponent<RectTransform>();
				component2.anchorMin = new Vector2(0f, 0f);
				component2.anchorMax = new Vector2(1f, 1f);
				component2.offsetMin = Vector2.zero;
				component2.offsetMax = Vector2.zero;
				((Graphic)val.GetComponent<Image>()).color = new Color(0f, 0f, 0f, 0.3f);
			}
			else if (Object.op_Implicit((Object)(object)mCanvas))
			{
				Object.Destroy((Object)(object)mCanvas);
			}
		}

		private static RectOffset RectOffset(int value)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new RectOffset(value, value, value, value);
		}

		private static RectOffset RectOffset(int x, int y)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new RectOffset(x, x, y, y);
		}

		public static int GetNextWindowId()
		{
			return ++mLastWindowId;
		}

		public static void RenderTooltip(string str, GUIStyle style = null, params GUILayoutOption[] options)
		{
			BeginTooltip(str);
			EndTooltip(str, style, options);
		}

		[Obsolete("Use new version with title.")]
		public static bool DrawKeybinding(ref KeyBinding key, GUIStyle style = null, params GUILayoutOption[] option)
		{
			return DrawKeybinding(ref key, null, style, option);
		}

		public static bool DrawKeybinding(ref KeyBinding key, string title, GUIStyle style = null, params GUILayoutOption[] option)
		{
			return DrawKeybinding(ref key, title, 0, style, option);
		}

		public static bool DrawKeybinding(ref KeyBinding key, string title, int unique, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			if (key == null)
			{
				key = new KeyBinding();
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			byte[] array = new byte[3] { 1, 2, 4 };
			string[] array2 = new string[3] { "Ctrl", "Shift", "Alt" };
			byte b = key.modifiers;
			for (int i = 0; i < array.Length; i++)
			{
				if (GUILayout.Toggle((b & array[i]) != 0, array2[i], (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
				{
					b |= array[i];
				}
				else if ((b & array[i]) != 0)
				{
					b ^= array[i];
				}
			}
			GUILayout.Label(" + ", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			int selected = key.Index;
			if (PopupToggleGroup(ref selected, KeyBinding.KeysName, title, unique, style, option))
			{
				key.Change((KeyCode)Enum.Parse(typeof(KeyCode), KeyBinding.KeysCode[selected]), b);
				result = true;
			}
			if (key.modifiers != b)
			{
				key.modifiers = b;
				result = true;
			}
			GUILayout.EndHorizontal();
			return result;
		}

		public static bool DrawVector(ref Vector2 vec, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			float[] values = new float[2] { vec.x, vec.y };
			string[] labels = new string[2] { "x", "y" };
			if (DrawFloatMultiField(ref values, labels, style, option))
			{
				vec = new Vector2(values[0], values[1]);
				return true;
			}
			return false;
		}

		public static void DrawVector(Vector2 vec, Action<Vector2> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (DrawVector(ref vec, style, option))
			{
				onChange(vec);
			}
		}

		public static bool DrawVector(ref Vector3 vec, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			float[] values = new float[3] { vec.x, vec.y, vec.z };
			string[] labels = new string[3] { "x", "y", "z" };
			if (DrawFloatMultiField(ref values, labels, style, option))
			{
				vec = new Vector3(values[0], values[1], values[2]);
				return true;
			}
			return false;
		}

		public static void DrawVector(Vector3 vec, Action<Vector3> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (DrawVector(ref vec, style, option))
			{
				onChange(vec);
			}
		}

		public static bool DrawVector(ref Vector4 vec, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			float[] values = new float[4] { vec.x, vec.y, vec.z, vec.w };
			string[] labels = new string[4] { "x", "y", "z", "w" };
			if (DrawFloatMultiField(ref values, labels, style, option))
			{
				vec = new Vector4(values[0], values[1], values[2], values[3]);
				return true;
			}
			return false;
		}

		public static void DrawVector(Vector4 vec, Action<Vector4> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (DrawVector(ref vec, style, option))
			{
				onChange(vec);
			}
		}

		public static bool DrawColor(ref Color vec, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			float[] values = new float[4] { vec.r, vec.g, vec.b, vec.a };
			string[] labels = new string[4] { "r", "g", "b", "a" };
			if (DrawFloatMultiField(ref values, labels, style, option))
			{
				vec = new Color(values[0], values[1], values[2], values[3]);
				return true;
			}
			return false;
		}

		public static void DrawColor(Color vec, Action<Color> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (DrawColor(ref vec, style, option))
			{
				onChange(vec);
			}
		}

		public static bool DrawFloatMultiField(ref float[] values, string[] labels, GUIStyle style = null, params GUILayoutOption[] option)
		{
			if (values == null || values.Length == 0)
			{
				throw new ArgumentNullException("values");
			}
			if (labels == null || labels.Length == 0)
			{
				throw new ArgumentNullException("labels");
			}
			if (values.Length != labels.Length)
			{
				throw new ArgumentOutOfRangeException("labels");
			}
			bool result = false;
			float[] array = new float[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label(labels[i], (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
				string text = GUILayout.TextField(values[i].ToString("f6"), style ?? GUI.skin.textField, option);
				GUILayout.EndHorizontal();
				float result2;
				if (string.IsNullOrEmpty(text))
				{
					array[i] = 0f;
				}
				else if (float.TryParse(text, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out result2))
				{
					array[i] = result2;
				}
				else
				{
					array[i] = 0f;
				}
				if (array[i] != values[i])
				{
					result = true;
				}
			}
			values = array;
			return result;
		}

		public static bool DrawFloatField(ref float value, string label, GUIStyle style = null, params GUILayoutOption[] option)
		{
			float num = value;
			GUILayout.Label(label, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			string text = GUILayout.TextField(value.ToString("f6"), style ?? GUI.skin.textField, option);
			float result;
			if (string.IsNullOrEmpty(text))
			{
				value = 0f;
			}
			else if (float.TryParse(text, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result))
			{
				value = result;
			}
			else
			{
				value = 0f;
			}
			return value != num;
		}

		public static void DrawFloatField(float value, string label, Action<float> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (DrawFloatField(ref value, label, style, option))
			{
				onChange(value);
			}
		}

		public static bool DrawIntField(ref int value, string label, GUIStyle style = null, params GUILayoutOption[] option)
		{
			int num = value;
			GUILayout.Label(label, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			string text = GUILayout.TextField(value.ToString(), style ?? GUI.skin.textField, option);
			int result;
			if (string.IsNullOrEmpty(text))
			{
				value = 0;
			}
			else if (int.TryParse(text, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result))
			{
				value = result;
			}
			else
			{
				value = 0;
			}
			return value != num;
		}

		public static void DrawIntField(int value, string label, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
		{
			if (onChange == null)
			{
				throw new ArgumentNullException("onChange");
			}
			if (DrawIntField(ref value, label, style, option))
			{
				onChange(value);
			}
		}

		private static bool DependsOn(string str, object container, Type type, ModEntry mod)
		{
			string[] array = str.Split('|', StringSplitOptions.None);
			if (array.Length != 2)
			{
				throw new Exception("VisibleOn/InvisibleOn(" + str + ") must have 2 params, name and value, e.g (FieldName|True) or (#PropertyName|True).");
			}
			if (!str.StartsWith("#"))
			{
				FieldInfo field = type.GetField(array[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null)
				{
					throw new Exception("Field '" + array[0] + "' not found. Insert # at the beginning for properties.");
				}
				if (!field.FieldType.IsPrimitive && !field.FieldType.IsEnum)
				{
					throw new Exception("Type '" + field.FieldType.Name + "' is not supported.");
				}
				object obj = null;
				if (field.FieldType.IsEnum)
				{
					try
					{
						obj = Enum.Parse(field.FieldType, array[1]);
						if (obj == null)
						{
							throw new Exception("Value '" + array[1] + "' cannot be parsed.");
						}
					}
					catch (Exception ex)
					{
						mod.Logger.Log("Parse value VisibleOn/InvisibleOn(" + str + ")");
						throw ex;
					}
				}
				else if (field.FieldType == typeof(string))
				{
					obj = array[1];
				}
				else
				{
					try
					{
						obj = Convert.ChangeType(array[1], field.FieldType);
						if (obj == null)
						{
							throw new Exception("Value '" + array[1] + "' cannot be parsed.");
						}
					}
					catch (Exception ex2)
					{
						mod.Logger.Log("Parse value VisibleOn/InvisibleOn(" + str + ")");
						throw ex2;
					}
				}
				object value = field.GetValue(container);
				return value.GetHashCode() == obj.GetHashCode();
			}
			array[0] = array[0].TrimStart('#');
			PropertyInfo property = type.GetProperty(array[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null)
			{
				throw new Exception("Property '" + array[0] + "' not found.");
			}
			if (!property.PropertyType.IsPrimitive && !property.PropertyType.IsEnum)
			{
				throw new Exception("Type '" + property.PropertyType.Name + "' is not supported.");
			}
			object obj2 = null;
			if (property.PropertyType.IsEnum)
			{
				try
				{
					obj2 = Enum.Parse(property.PropertyType, array[1]);
					if (obj2 == null)
					{
						throw new Exception("Value '" + array[1] + "' cannot be parsed.");
					}
				}
				catch (Exception ex3)
				{
					mod.Logger.Log("Parse value VisibleOn/InvisibleOn(" + str + ")");
					throw ex3;
				}
			}
			else if (property.PropertyType == typeof(string))
			{
				obj2 = array[1];
			}
			else
			{
				try
				{
					obj2 = Convert.ChangeType(array[1], property.PropertyType);
					if (obj2 == null)
					{
						throw new Exception("Value '" + array[1] + "' cannot be parsed.");
					}
				}
				catch (Exception ex4)
				{
					mod.Logger.Log("Parse value VisibleOn/InvisibleOn(" + str + ")");
					throw ex4;
				}
			}
			object value2 = property.GetValue(container, null);
			return value2.GetHashCode() == obj2.GetHashCode();
		}

		private static void BeginTooltip(string tooltip)
		{
		}

		private static void EndTooltip(string tooltip, GUIStyle style = null, params GUILayoutOption[] options)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Invalid comparison between Unknown and I4
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(tooltip))
			{
				return;
			}
			GUILayout.Box((Texture)(object)Textures.Question, style ?? question, options);
			if ((int)Event.current.type == 7)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (((Rect)(ref lastRect)).Contains(Event.current.mousePosition))
				{
					ShowTooltip(tooltip);
				}
			}
		}

		private static void BeginHorizontalTooltip(DrawAttribute a)
		{
			if (!string.IsNullOrEmpty(a.Tooltip) && a.Vertical)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			}
		}

		private static void EndHorizontalTooltip(DrawAttribute a)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Invalid comparison between Unknown and I4
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(a.Tooltip))
			{
				return;
			}
			GUILayout.Box((Texture)(object)Textures.Question, question, Array.Empty<GUILayoutOption>());
			if ((int)Event.current.type == 7)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (((Rect)(ref lastRect)).Contains(Event.current.mousePosition))
				{
					ShowTooltip(a.Tooltip);
				}
			}
			if (a.Vertical)
			{
				GUILayout.EndHorizontal();
			}
		}

		private static void ShowTooltip(string str)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			Instance.mTooltip = new GUIContent(str);
		}

		private static bool Draw(object container, Type type, ModEntry mod, DrawFieldMask defaultMask, int unique)
		{
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Expected O, but got Unknown
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Expected O, but got Unknown
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Expected O, but got Unknown
			//IL_08cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ac3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ac8: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bbd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0be1: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			List<GUILayoutOption> list = new List<GUILayoutOption>();
			DrawFieldMask drawFieldMask = defaultMask;
			object[] customAttributes = type.GetCustomAttributes(typeof(DrawFieldsAttribute), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				DrawFieldsAttribute drawFieldsAttribute = (DrawFieldsAttribute)customAttributes[i];
				drawFieldMask = drawFieldsAttribute.Mask;
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo[] array = fields;
			foreach (FieldInfo f in array)
			{
				DrawAttribute drawAttribute = new DrawAttribute();
				object[] customAttributes2 = f.GetCustomAttributes(typeof(DrawAttribute), inherit: false);
				if (customAttributes2.Length != 0)
				{
					object[] array2 = customAttributes2;
					for (int k = 0; k < array2.Length; k++)
					{
						DrawAttribute drawAttribute2 = (DrawAttribute)array2[k];
						drawAttribute = drawAttribute2;
						drawAttribute.Width = ((drawAttribute.Width != 0) ? Scale(drawAttribute.Width) : 0);
						drawAttribute.Height = ((drawAttribute.Height != 0) ? Scale(drawAttribute.Height) : 0);
					}
					if (drawAttribute.Type == DrawType.Ignore)
					{
						continue;
					}
					if (!string.IsNullOrEmpty(drawAttribute.VisibleOn))
					{
						if (!DependsOn(drawAttribute.VisibleOn, container, type, mod))
						{
							continue;
						}
					}
					else if (!string.IsNullOrEmpty(drawAttribute.InvisibleOn) && DependsOn(drawAttribute.InvisibleOn, container, type, mod))
					{
						continue;
					}
				}
				else
				{
					if ((drawFieldMask & DrawFieldMask.OnlyDrawAttr) != 0 || ((drawFieldMask & DrawFieldMask.SkipNotSerialized) != 0 && f.IsNotSerialized) || (((drawFieldMask & DrawFieldMask.Public) <= DrawFieldMask.Any || !f.IsPublic) && ((drawFieldMask & DrawFieldMask.Serialized) <= DrawFieldMask.Any || f.GetCustomAttributes(typeof(SerializeField), inherit: false).Length == 0) && ((drawFieldMask & DrawFieldMask.Public) != 0 || (drawFieldMask & DrawFieldMask.Serialized) != 0)))
					{
						continue;
					}
					object[] customAttributes3 = f.GetCustomAttributes(typeof(RangeAttribute), inherit: false);
					int num = 0;
					if (num < customAttributes3.Length)
					{
						RangeAttribute val = (RangeAttribute)customAttributes3[num];
						drawAttribute.Type = DrawType.Slider;
						drawAttribute.Min = val.min;
						drawAttribute.Max = val.max;
					}
				}
				object[] customAttributes4 = f.GetCustomAttributes(typeof(SpaceAttribute), inherit: false);
				for (int l = 0; l < customAttributes4.Length; l++)
				{
					SpaceAttribute val2 = (SpaceAttribute)customAttributes4[l];
					GUILayout.Space((float)Scale((int)val2.height));
				}
				object[] customAttributes5 = f.GetCustomAttributes(typeof(HeaderAttribute), inherit: false);
				for (int m = 0; m < customAttributes5.Length; m++)
				{
					HeaderAttribute val3 = (HeaderAttribute)customAttributes5[m];
					GUILayout.Label(val3.header, bold, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
				}
				string text = ((drawAttribute.Label == null) ? f.Name : drawAttribute.Label);
				bool flag;
				bool flag2;
				if (((f.FieldType.IsClass && !f.FieldType.IsArray) || (f.FieldType.IsValueType && !f.FieldType.IsPrimitive && !f.FieldType.IsEnum)) && !Array.Exists(specialTypes, (Type x) => x == f.FieldType))
				{
					defaultMask = drawFieldMask;
					object[] customAttributes6 = f.GetCustomAttributes(typeof(DrawFieldsAttribute), inherit: false);
					for (int n = 0; n < customAttributes6.Length; n++)
					{
						DrawFieldsAttribute drawFieldsAttribute2 = (DrawFieldsAttribute)customAttributes6[n];
						defaultMask = drawFieldsAttribute2.Mask;
					}
					flag = drawAttribute.Box || (drawAttribute.Collapsible && collapsibleStates.Exists((int x) => x == f.MetadataToken));
					flag2 = f.GetCustomAttributes(typeof(HorizontalAttribute), inherit: false).Length != 0 || f.FieldType.GetCustomAttributes(typeof(HorizontalAttribute), inherit: false).Length != 0;
					if (flag2)
					{
						GUILayout.BeginHorizontal(GUIStyle.op_Implicit(flag ? "box" : ""), Array.Empty<GUILayoutOption>());
						flag = false;
					}
					if (drawAttribute.Collapsible)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					if (!string.IsNullOrEmpty(text))
					{
						BeginHorizontalTooltip(drawAttribute);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndHorizontalTooltip(drawAttribute);
					}
					bool flag3 = true;
					if (drawAttribute.Collapsible)
					{
						if (!string.IsNullOrEmpty(text))
						{
							GUILayout.Space(5f);
						}
						flag3 = collapsibleStates.Exists((int x) => x == f.MetadataToken);
						if (GUILayout.Button(flag3 ? "Hide" : "Show", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
						{
							if (flag3)
							{
								collapsibleStates.Remove(f.MetadataToken);
							}
							else
							{
								collapsibleStates.Add(f.MetadataToken);
							}
						}
						GUILayout.EndHorizontal();
					}
					if (flag3)
					{
						if (flag)
						{
							GUILayout.BeginVertical(GUIStyle.op_Implicit("box"), Array.Empty<GUILayoutOption>());
						}
						object value = f.GetValue(container);
						if (typeof(Object).IsAssignableFrom(f.FieldType))
						{
							Object val4 = (Object)((value is Object) ? value : null);
							if (val4 != null)
							{
								GUILayout.Label(val4.name, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
								goto IL_0669;
							}
						}
						if (Draw(value, f.FieldType, mod, defaultMask, f.Name.GetHashCode() + unique))
						{
							result = true;
							f.SetValue(container, value);
						}
						goto IL_0669;
					}
					goto IL_0678;
				}
				list.Clear();
				if (drawAttribute.Type == DrawType.Auto)
				{
					if (Array.Exists(fieldTypes, (Type x) => x == f.FieldType))
					{
						drawAttribute.Type = DrawType.Field;
					}
					else if (Array.Exists(toggleTypes, (Type x) => x == f.FieldType))
					{
						drawAttribute.Type = DrawType.Toggle;
					}
					else if (f.FieldType.IsEnum)
					{
						if (f.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length == 0)
						{
							drawAttribute.Type = DrawType.PopupList;
						}
					}
					else if (f.FieldType == typeof(KeyBinding))
					{
						drawAttribute.Type = DrawType.KeyBinding;
					}
				}
				if (drawAttribute.Type == DrawType.Field)
				{
					if (!Array.Exists(fieldTypes, (Type x) => x == f.FieldType) && !f.FieldType.IsArray)
					{
						throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.Field}");
					}
					list.Add((drawAttribute.Width != 0) ? GUILayout.Width((float)drawAttribute.Width) : GUILayout.Width((float)Scale(100)));
					list.Add((drawAttribute.Height != 0) ? GUILayout.Height((float)drawAttribute.Height) : GUILayout.Height((float)Scale((int)drawHeight)));
					if (f.FieldType == typeof(Vector2))
					{
						if (drawAttribute.Vertical)
						{
							GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
						}
						else
						{
							GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						}
						BeginHorizontalTooltip(drawAttribute);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndHorizontalTooltip(drawAttribute);
						if (!drawAttribute.Vertical)
						{
							GUILayout.Space((float)Scale(5));
						}
						Vector2 vec = (Vector2)f.GetValue(container);
						if (DrawVector(ref vec, null, list.ToArray()))
						{
							f.SetValue(container, vec);
							result = true;
						}
						if (drawAttribute.Vertical)
						{
							GUILayout.EndVertical();
							continue;
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						continue;
					}
					if (f.FieldType == typeof(Vector3))
					{
						if (drawAttribute.Vertical)
						{
							GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
						}
						else
						{
							GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						}
						BeginHorizontalTooltip(drawAttribute);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndHorizontalTooltip(drawAttribute);
						if (!drawAttribute.Vertical)
						{
							GUILayout.Space((float)Scale(5));
						}
						Vector3 vec2 = (Vector3)f.GetValue(container);
						if (DrawVector(ref vec2, null, list.ToArray()))
						{
							f.SetValue(container, vec2);
							result = true;
						}
						if (drawAttribute.Vertical)
						{
							GUILayout.EndVertical();
							continue;
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						continue;
					}
					if (f.FieldType == typeof(Vector4))
					{
						if (drawAttribute.Vertical)
						{
							GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
						}
						else
						{
							GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						}
						BeginHorizontalTooltip(drawAttribute);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndHorizontalTooltip(drawAttribute);
						if (!drawAttribute.Vertical)
						{
							GUILayout.Space((float)Scale(5));
						}
						Vector4 vec3 = (Vector4)f.GetValue(container);
						if (DrawVector(ref vec3, null, list.ToArray()))
						{
							f.SetValue(container, vec3);
							result = true;
						}
						if (drawAttribute.Vertical)
						{
							GUILayout.EndVertical();
							continue;
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						continue;
					}
					if (f.FieldType == typeof(Color))
					{
						if (drawAttribute.Vertical)
						{
							GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
						}
						else
						{
							GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						}
						BeginHorizontalTooltip(drawAttribute);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndHorizontalTooltip(drawAttribute);
						if (!drawAttribute.Vertical)
						{
							GUILayout.Space((float)Scale(5));
						}
						Color vec4 = (Color)f.GetValue(container);
						if (DrawColor(ref vec4, null, list.ToArray()))
						{
							f.SetValue(container, vec4);
							result = true;
						}
						if (drawAttribute.Vertical)
						{
							GUILayout.EndVertical();
							continue;
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						continue;
					}
					object value2 = f.GetValue(container);
					Type type2 = null;
					object[] array3 = null;
					if (f.FieldType.IsArray)
					{
						if (value2 is IEnumerable source)
						{
							array3 = source.Cast<object>().ToArray();
							type2 = value2.GetType().GetElementType();
						}
					}
					else
					{
						array3 = new object[1] { value2 };
						type2 = value2.GetType();
					}
					if (array3 == null)
					{
						continue;
					}
					bool flag4 = false;
					drawAttribute.Vertical = drawAttribute.Vertical || f.FieldType.IsArray;
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					}
					else
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					if (f.FieldType.IsArray)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						BeginTooltip(drawAttribute.Tooltip);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndTooltip(drawAttribute.Tooltip, null);
						GUILayout.Space((float)Scale(5));
						if (GUILayout.Button("+", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
						{
							Array.Resize(ref array3, Math.Min(array3.Length + 1, int.MaxValue));
							array3[array3.Length - 1] = Convert.ChangeType("0", type2);
							flag4 = true;
							result = true;
						}
						if (GUILayout.Button("-", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
						{
							Array.Resize(ref array3, Math.Max(array3.Length - 1, 0));
							if (array3.Length != 0)
							{
								array3[array3.Length - 1] = Convert.ChangeType("0", type2);
							}
							flag4 = true;
							result = true;
						}
						GUILayout.EndHorizontal();
					}
					else
					{
						BeginHorizontalTooltip(drawAttribute);
						GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
						EndHorizontalTooltip(drawAttribute);
					}
					if (!drawAttribute.Vertical)
					{
						GUILayout.Space((float)Scale(5));
					}
					if (array3.Length != 0)
					{
						bool flag5 = f.FieldType == typeof(float) || f.FieldType == typeof(double) || f.FieldType == typeof(float[]) || f.FieldType == typeof(double[]);
						for (int num2 = 0; num2 < array3.Length; num2++)
						{
							string text2 = array3[num2].ToString();
							if (drawAttribute.Precision >= 0 && flag5 && double.TryParse(text2, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var result2))
							{
								text2 = result2.ToString($"f{drawAttribute.Precision}");
							}
							if (f.FieldType.IsArray)
							{
								GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
								GUILayout.Label($"  [{num2}] ", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
							}
							string text3 = ((f.FieldType == typeof(string)) ? GUILayout.TextField(text2, drawAttribute.MaxLength, list.ToArray()) : GUILayout.TextField(text2, list.ToArray()));
							if (f.FieldType.IsArray)
							{
								GUILayout.EndHorizontal();
							}
							if (!(text3 != text2))
							{
								continue;
							}
							double result3;
							if (string.IsNullOrEmpty(text3))
							{
								if (f.FieldType != typeof(string))
								{
									text3 = "0";
								}
							}
							else if (double.TryParse(text3, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result3))
							{
								result3 = Math.Max(result3, drawAttribute.Min);
								result3 = Math.Min(result3, drawAttribute.Max);
								text3 = result3.ToString();
							}
							else
							{
								text3 = "0";
							}
							array3[num2] = Convert.ChangeType(text3, type2);
							result = true;
							flag4 = true;
						}
					}
					if (flag4)
					{
						if (f.FieldType.IsArray)
						{
							if (type2 == typeof(float))
							{
								f.SetValue(container, Array.ConvertAll(array3, (object x) => (float)x));
							}
							else if (type2 == typeof(int))
							{
								f.SetValue(container, Array.ConvertAll(array3, (object x) => (int)x));
							}
							else if (type2 == typeof(long))
							{
								f.SetValue(container, Array.ConvertAll(array3, (object x) => (long)x));
							}
							else if (type2 == typeof(double))
							{
								f.SetValue(container, Array.ConvertAll(array3, (object x) => (double)x));
							}
						}
						else
						{
							f.SetValue(container, array3[0]);
						}
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.EndVertical();
					}
					else
					{
						GUILayout.EndHorizontal();
					}
				}
				else if (drawAttribute.Type == DrawType.Slider)
				{
					if (!Array.Exists(sliderTypes, (Type x) => x == f.FieldType))
					{
						throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.Slider}");
					}
					list.Add((drawAttribute.Width != 0) ? GUILayout.Width((float)drawAttribute.Width) : GUILayout.Width((float)Scale(200)));
					list.Add((drawAttribute.Height != 0) ? GUILayout.Height((float)drawAttribute.Height) : GUILayout.Height((float)Scale((int)drawHeight)));
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					}
					else
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					BeginHorizontalTooltip(drawAttribute);
					GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					EndHorizontalTooltip(drawAttribute);
					if (!drawAttribute.Vertical)
					{
						GUILayout.Space((float)Scale(5));
					}
					string s = f.GetValue(container).ToString();
					if (!double.TryParse(s, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var result4))
					{
						result4 = 0.0;
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					float num3 = (float)result4;
					float num4 = GUILayout.HorizontalSlider(num3, (float)drawAttribute.Min, (float)drawAttribute.Max, list.ToArray());
					if (!drawAttribute.Vertical)
					{
						GUILayout.Space((float)Scale(5));
					}
					GUILayout.Label(num4.ToString(), (GUILayoutOption[])(object)new GUILayoutOption[2]
					{
						GUILayout.ExpandWidth(false),
						GUILayout.Height((float)Scale((int)drawHeight))
					});
					if (drawAttribute.Vertical)
					{
						GUILayout.EndHorizontal();
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.EndVertical();
					}
					else
					{
						GUILayout.EndHorizontal();
					}
					if (num4 != num3)
					{
						if ((f.FieldType == typeof(float) || f.FieldType == typeof(double)) && drawAttribute.Precision >= 0)
						{
							num4 = (float)Math.Round(num4, drawAttribute.Precision);
						}
						f.SetValue(container, Convert.ChangeType(num4, f.FieldType));
						result = true;
					}
				}
				else if (drawAttribute.Type == DrawType.Toggle)
				{
					if (!Array.Exists(toggleTypes, (Type x) => x == f.FieldType))
					{
						throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.Toggle}");
					}
					list.Add(GUILayout.ExpandWidth(false));
					list.Add((drawAttribute.Height != 0) ? GUILayout.Height((float)drawAttribute.Height) : GUILayout.Height((float)Scale((int)drawHeight)));
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					}
					else
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					BeginHorizontalTooltip(drawAttribute);
					GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					EndHorizontalTooltip(drawAttribute);
					bool flag6 = (bool)f.GetValue(container);
					bool flag7 = GUILayout.Toggle(flag6, "", list.ToArray());
					if (drawAttribute.Vertical)
					{
						GUILayout.EndVertical();
					}
					else
					{
						GUILayout.EndHorizontal();
					}
					if (flag7 != flag6)
					{
						f.SetValue(container, Convert.ChangeType(flag7, f.FieldType));
						result = true;
					}
				}
				else if (drawAttribute.Type == DrawType.ToggleGroup)
				{
					if (!f.FieldType.IsEnum)
					{
						throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.ToggleGroup}");
					}
					if (f.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length != 0)
					{
						throw new Exception($"Type {f.FieldType}/{DrawType.ToggleGroup} incompatible with Flag attribute.");
					}
					list.Add(GUILayout.ExpandWidth(false));
					list.Add((drawAttribute.Height != 0) ? GUILayout.Height((float)drawAttribute.Height) : GUILayout.Height((float)Scale((int)drawHeight)));
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					}
					else
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					BeginHorizontalTooltip(drawAttribute);
					GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					EndHorizontalTooltip(drawAttribute);
					if (!drawAttribute.Vertical)
					{
						GUILayout.Space((float)Scale(5));
					}
					string[] names = Enum.GetNames(f.FieldType);
					int selected = (int)f.GetValue(container);
					if (ToggleGroup(ref selected, names, null, list.ToArray()))
					{
						object value3 = Enum.Parse(f.FieldType, names[selected]);
						f.SetValue(container, value3);
						result = true;
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.EndVertical();
					}
					else
					{
						GUILayout.EndHorizontal();
					}
				}
				else if (drawAttribute.Type == DrawType.PopupList)
				{
					if (!f.FieldType.IsEnum)
					{
						throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.PopupList}");
					}
					if (f.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length != 0)
					{
						throw new Exception($"Type {f.FieldType}/{DrawType.ToggleGroup} incompatible with Flag attribute.");
					}
					list.Add(GUILayout.ExpandWidth(false));
					list.Add((drawAttribute.Height != 0) ? GUILayout.Height((float)drawAttribute.Height) : GUILayout.Height((float)Scale((int)drawHeight)));
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					}
					else
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					BeginHorizontalTooltip(drawAttribute);
					GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					EndHorizontalTooltip(drawAttribute);
					if (!drawAttribute.Vertical)
					{
						GUILayout.Space((float)Scale(5));
					}
					string[] names2 = Enum.GetNames(f.FieldType);
					int selected2 = (int)f.GetValue(container);
					if (PopupToggleGroup(ref selected2, names2, text, unique, null, list.ToArray()))
					{
						object value4 = Enum.Parse(f.FieldType, names2[selected2]);
						f.SetValue(container, value4);
						result = true;
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.EndVertical();
					}
					else
					{
						GUILayout.EndHorizontal();
					}
				}
				else if (drawAttribute.Type == DrawType.KeyBinding)
				{
					if (f.FieldType != typeof(KeyBinding))
					{
						throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.KeyBinding}");
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					}
					else
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					}
					BeginHorizontalTooltip(drawAttribute);
					GUILayout.Label(text, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
					EndHorizontalTooltip(drawAttribute);
					if (!drawAttribute.Vertical)
					{
						GUILayout.Space((float)Scale(5));
					}
					KeyBinding key = (KeyBinding)f.GetValue(container);
					if (DrawKeybinding(ref key, text, unique, null, list.ToArray()))
					{
						f.SetValue(container, key);
						result = true;
					}
					if (drawAttribute.Vertical)
					{
						GUILayout.EndVertical();
						continue;
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				continue;
				IL_0669:
				if (flag)
				{
					GUILayout.EndVertical();
				}
				goto IL_0678;
				IL_0678:
				if (flag2)
				{
					GUILayout.EndHorizontal();
				}
			}
			return result;
		}

		public static void DrawFields<T>(ref T container, ModEntry mod, DrawFieldMask defaultMask, Action onChange = null) where T : new()
		{
			DrawFields(ref container, mod, 0, defaultMask, onChange);
		}

		public static void DrawFields<T>(ref T container, ModEntry mod, int unique, DrawFieldMask defaultMask, Action onChange = null) where T : new()
		{
			object obj = container;
			if (!Draw(obj, typeof(T), mod, defaultMask, unique))
			{
				return;
			}
			container = (T)obj;
			if (onChange != null)
			{
				try
				{
					onChange();
				}
				catch (Exception e)
				{
					mod.Logger.LogException(e);
				}
			}
		}
	}

	private static class Screen_lockCursor_Patch
	{
		private static bool Prefix(bool value)
		{
			if ((Object)(object)UI.Instance != (Object)null && UI.Instance.Opened)
			{
				if (value)
				{
					UI.Instance.GameCursorVisible = false;
					UI.Instance.GameCursorLockMode = (CursorLockMode)1;
				}
				else
				{
					UI.Instance.GameCursorLockMode = (CursorLockMode)0;
					UI.Instance.GameCursorVisible = true;
				}
				Cursor.visible = true;
				Cursor.lockState = (CursorLockMode)0;
				return false;
			}
			return true;
		}
	}

	private static readonly Version VER_0 = new Version();

	private static readonly Version VER_0_13 = new Version(0, 13);

	private static ModuleDefMD thisModuleDef = ModuleDefMD.Load(typeof(UnityModManager).Module);

	private static bool forbidDisableMods;

	public static readonly List<ModEntry> ModEntries = new List<ModEntry>();

	internal static bool started;

	internal static bool initialized;

	public static Version unityVersion { get; private set; }

	public static Version gameVersion { get; private set; } = new Version();


	public static Version version { get; private set; } = new Version(0, 25, 0);


	public static string ModsPath => Path.Combine(Application.persistentDataPath, "UnityModManager");

	internal static Param Params { get; set; } = new Param();


	internal static GameInfo Config { get; set; } = new GameInfo();


	public static bool Initialize()
	{
		if (initialized)
		{
			return true;
		}
		initialized = true;
		PFLog.UnityModManager.Log("Initialize.");
		PFLog.UnityModManager.Log($"Version: {version}.");
		try
		{
			PFLog.UnityModManager.Log(string.Format("OS: {0} {1}.", Environment.OSVersion, Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")));
			PFLog.UnityModManager.Log($"Net Framework: {Environment.Version}.");
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			PFLog.UnityModManager.Exception(ex, (string)null);
		}
		unityVersion = ParseVersion(Application.unityVersion);
		PFLog.UnityModManager.Log($"Unity Engine: {unityVersion}.");
		Params = Param.Load();
		if (!Directory.Exists(ModsPath))
		{
			Directory.CreateDirectory(ModsPath);
		}
		PFLog.UnityModManager.Log("Mods path: " + ModsPath + ".");
		PFLog.System.Log("Mods path: " + ModsPath + ".");
		KeyBinding.Initialize();
		AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		return true;
	}

	private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.FullName == args.Name);
		if (assembly != null)
		{
			return assembly;
		}
		string text = null;
		if (args.Name.StartsWith("0Harmony12"))
		{
			text = "0Harmony12.dll";
		}
		else if (args.Name.StartsWith("0Harmony, Version=1.") || args.Name.StartsWith("0Harmony-1.2"))
		{
			text = "0Harmony-1.2.dll";
		}
		else if (args.Name.StartsWith("0Harmony, Version=2."))
		{
			text = "0Harmony.dll";
		}
		if (text != null)
		{
			string path = Path.Combine(Path.GetDirectoryName(typeof(UnityModManager).Assembly.Location), text);
			if (File.Exists(path))
			{
				try
				{
					return Assembly.LoadFile(path);
				}
				catch (Exception ex)
				{
					PFLog.UnityModManager.Error(ex.ToString());
				}
			}
			else
			{
				Debug.LogError((object)"No harmony found.");
			}
		}
		string text2 = args.RequestingAssembly?.Location;
		if (text2 == null)
		{
			return null;
		}
		string directoryName = Path.GetDirectoryName(text2);
		AssemblyName name = new AssemblyName(args.Name);
		string text3 = Directory.EnumerateFiles(directoryName, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault((string f) => Path.GetFileNameWithoutExtension(f).Equals(name.Name, StringComparison.InvariantCultureIgnoreCase));
		if (!string.IsNullOrEmpty(text3))
		{
			return Assembly.LoadFile(text3);
		}
		return null;
	}

	private static void GetActiveModInfo()
	{
		string path = Path.Combine(Application.persistentDataPath, "ActiveUMMItemsInfo.txt");
		if (ModEntries == null || ModEntries.Count == 0)
		{
			return;
		}
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path, append: true);
			foreach (ModEntry modEntry in ModEntries)
			{
				if (modEntry.Active)
				{
					streamWriter.WriteLine(modEntry.Info.Id + "~~~" + modEntry.Info.Version);
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Exception(ex, (string)null);
		}
	}

	private static ExtendedModInfo ToExtendedModInfo(ModEntry modEntry)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		ExtendedModInfo val = new ExtendedModInfo
		{
			Id = modEntry.Info.Id,
			Author = modEntry.Info.Author,
			Path = modEntry.Path,
			Version = modEntry.Info.Version,
			IsUmmMod = true,
			Enabled = modEntry.Active,
			DisplayName = modEntry.Info.DisplayName,
			Description = null,
			HasSettings = (modEntry.OnGUI != null)
		};
		if (modEntry.Version != null && modEntry.NewestVersion != null)
		{
			val.UpdateRequired = modEntry.Version < modEntry.NewestVersion;
		}
		return val;
	}

	private static ModEntry FindModInfo(string modId)
	{
		if (!started)
		{
			PFLog.UnityModManager.Error("Trying to get mod info, when UMM is not initialized.");
			return null;
		}
		if (ModEntries == null || ModEntries.Count == 0)
		{
			PFLog.UnityModManager.Log("Umm has no active mods.");
			return null;
		}
		List<ModEntry> list = ModEntries.Where((ModEntry x) => x.Info.Id == modId).ToList();
		if (list == null || list.Count != 1)
		{
			PFLog.UnityModManager.Log("Mod with id: " + modId + " not found in UMM.");
			return null;
		}
		return list[0];
	}

	public static void EnableMod(string modId, bool state)
	{
		if (string.IsNullOrEmpty(modId))
		{
			PFLog.UnityModManager.Error("Got empty or null string instead of mod id");
			return;
		}
		ModEntry modEntry = FindModInfo(modId);
		if (modEntry == null)
		{
			PFLog.UnityModManager.Log("Couldn't find mod with id " + modId + " while trying to enable or disable mod in UMM");
			return;
		}
		modEntry.Active = state;
		modEntry.Enabled = state;
		foreach (Param.Mod modParam in Params.ModParams)
		{
			if (modParam.Id != modEntry.Info.Id)
			{
				continue;
			}
			modParam.Enabled = state;
			break;
		}
		Params.Save();
	}

	public static void OpenModInfoWindow(string modId)
	{
		if ((Object)(object)UI.Instance == (Object)null)
		{
			PFLog.UnityModManager.Error("UMM UI is not initialized. You should initialize UI first.");
			return;
		}
		int num = -1;
		ModEntry modEntry = null;
		for (int i = 0; i < ModEntries.Count; i++)
		{
			if (!(ModEntries[i].Info.Id != modId))
			{
				num = i;
				modEntry = ModEntries[i];
				break;
			}
		}
		if (num < 0 || modEntry == null)
		{
			PFLog.UnityModManager.Log("Mod with id " + modId + " not found among UMM mods.");
			return;
		}
		if (!modEntry.Active)
		{
			PFLog.UnityModManager.Error("Can't show UI for Inactive mod, it may be not loaded, mod id " + modId);
			return;
		}
		if (modEntry.OnGUI == null)
		{
			PFLog.UnityModManager.Error("Target mod " + modId + " has no ui (either OnGUI == null or OnShowGUI == null)");
			return;
		}
		UI.Instance.ToggleWindow(open: true);
		UI.Instance.TryShowModSettingsUI(num);
	}

	public static void CheckForUpdates()
	{
		if (started && (Object)(object)UI.Instance != (Object)null)
		{
			CheckModUpdates();
		}
	}

	public static ExtendedModInfo GetModInfo(string modId)
	{
		if (string.IsNullOrEmpty(modId))
		{
			PFLog.UnityModManager.Error("Got empty or null string instead of mod id");
			return null;
		}
		ModEntry modEntry = FindModInfo(modId);
		return (modEntry == null) ? null : ToExtendedModInfo(modEntry);
	}

	public static List<ExtendedModInfo> GetAllModsInfo()
	{
		if (!started)
		{
			PFLog.UnityModManager.Error("Trying to get UMM mods before UMM was initialized");
			return null;
		}
		if (ModEntries == null || ModEntries.Count == 0)
		{
			PFLog.UnityModManager.Error("No Mods found in UMM.");
			return null;
		}
		List<ExtendedModInfo> list = new List<ExtendedModInfo>();
		foreach (ModEntry modEntry in ModEntries)
		{
			ExtendedModInfo item = ToExtendedModInfo(modEntry);
			list.Add(item);
		}
		return list;
	}

	public static void Start(string passedGameVersion)
	{
		try
		{
			_Start(passedGameVersion);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			PFLog.UnityModManager.Exception(ex, (string)null);
			OpenUnityFileLog();
		}
	}

	public static void UiFirstLaunch()
	{
		if (!initialized || (Object)(object)UI.Instance == (Object)null)
		{
			PFLog.UnityModManager.Error("UI.Instance is null. Either UnityModManager start failed or you call UnityModManager.UI.Instance before UnityModManager.Start");
		}
		else
		{
			UI.Instance.FirstLaunch();
		}
	}

	private static void _Start(string passedGameVersion)
	{
		if (!Initialize())
		{
			PFLog.UnityModManager.Log("Cancel start due to an error.");
			OpenUnityFileLog();
			return;
		}
		if (started)
		{
			PFLog.UnityModManager.Log("Cancel start. Already started.");
			return;
		}
		started = true;
		ParseGameVersion(passedGameVersion);
		if (Directory.Exists(ModsPath))
		{
			PFLog.UnityModManager.Log("Parsing mods.");
			Dictionary<string, ModEntry> dictionary = new Dictionary<string, ModEntry>();
			int num = 0;
			string[] directories = Directory.GetDirectories(ModsPath);
			foreach (string text in directories)
			{
				string text2 = Path.Combine(text, Config.ModInfo);
				if (!File.Exists(Path.Combine(text, Config.ModInfo)))
				{
					text2 = Path.Combine(text, Config.ModInfo.ToLower());
				}
				if (File.Exists(text2))
				{
					num++;
					PFLog.UnityModManager.Log("Reading file '" + text2 + "'.");
					try
					{
						ModInfo modInfo = File.ReadAllText(text2).FromJson<ModInfo>();
						if (string.IsNullOrEmpty(modInfo.Id))
						{
							PFLog.UnityModManager.Error("Id is null.");
							continue;
						}
						if (dictionary.ContainsKey(modInfo.Id))
						{
							PFLog.UnityModManager.Error("Id '" + modInfo.Id + "' already uses another mod.");
							continue;
						}
						if (string.IsNullOrEmpty(modInfo.AssemblyName))
						{
							modInfo.AssemblyName = modInfo.Id + ".dll";
						}
						ModEntry value = new ModEntry(modInfo, text + Path.DirectorySeparatorChar);
						dictionary.Add(modInfo.Id, value);
					}
					catch (Exception ex)
					{
						PFLog.UnityModManager.Error("Error parsing file '" + text2 + "'.");
						Debug.LogException(ex);
					}
				}
				else
				{
					PFLog.UnityModManager.Log("File not found " + text2);
				}
			}
			if (dictionary.Count > 0)
			{
				PFLog.UnityModManager.Log("Sorting mods.");
				TopoSort(dictionary);
				Params.ReadModParams();
				PFLog.UnityModManager.Log("Loading mods.");
				foreach (ModEntry modEntry in ModEntries)
				{
					if (!modEntry.Enabled)
					{
						modEntry.Logger.Log("To skip (disabled).");
					}
					else
					{
						modEntry.Active = true;
					}
				}
			}
			PFLog.UnityModManager.Log($"Finish. Successful loaded {ModEntries.Count((ModEntry x) => !x.ErrorOnLoading)}/{num} mods.".ToUpper());
			IEnumerable<Assembly> enumerable = from x in AppDomain.CurrentDomain.GetAssemblies()
				where x.ManifestModule.Name == "UnityModManager.dll"
				select x;
			if (enumerable.Count() > 1)
			{
				PFLog.UnityModManager.Error("Detected extra copies of UMM.");
				foreach (Assembly item in enumerable)
				{
					PFLog.UnityModManager.Log("- " + item.CodeBase);
				}
			}
		}
		else
		{
			PFLog.UnityModManager.Log("Directory '" + ModsPath + "' not exists.");
		}
		if (!UI.Load())
		{
			PFLog.UnityModManager.Error("Can't load UI.");
		}
		GetActiveModInfo();
		CheckModUpdates();
	}

	private static void ParseGameVersion(string passedGameVersion)
	{
		gameVersion = new Version(1, 0, 0);
	}

	private static bool GetValueFromMember(Type cls, ref object instance, string name, bool _static)
	{
		BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | (_static ? BindingFlags.Static : (BindingFlags.Instance | BindingFlags.FlattenHierarchy));
		FieldInfo field = cls.GetField(name, bindingAttr);
		if (field != null)
		{
			instance = field.GetValue(instance);
			return true;
		}
		PropertyInfo property = cls.GetProperty(name, bindingAttr);
		if (property != null)
		{
			instance = property.GetValue(instance, null);
			return true;
		}
		MethodInfo method = cls.GetMethod(name, bindingAttr, null, Type.EmptyTypes, null);
		if (method != null)
		{
			instance = method.Invoke(instance, null);
			return true;
		}
		PFLog.UnityModManager.Error("Class '" + cls.FullName + "' does not have a " + (_static ? "static" : "non-static") + " member '" + name + "'");
		return false;
	}

	private static void DFS(string id, Dictionary<string, ModEntry> mods)
	{
		if (ModEntries.Any((ModEntry m) => m.Info.Id == id))
		{
			return;
		}
		foreach (string key in mods[id].Requirements.Keys)
		{
			if (mods.ContainsKey(key))
			{
				DFS(key, mods);
			}
		}
		foreach (string item in mods[id].LoadAfter)
		{
			if (mods.ContainsKey(item))
			{
				DFS(item, mods);
			}
		}
		ModEntries.Add(mods[id]);
	}

	private static void TopoSort(Dictionary<string, ModEntry> mods)
	{
		foreach (string key in mods.Keys)
		{
			DFS(key, mods);
		}
	}

	public static ModEntry FindMod(string id)
	{
		return ModEntries.FirstOrDefault((ModEntry x) => x.Info.Id == id);
	}

	public static Version GetVersion()
	{
		return version;
	}

	public static void SaveSettingsAndParams()
	{
		Params.Save();
		foreach (ModEntry modEntry in ModEntries)
		{
			if (modEntry.Active && modEntry.OnSaveGUI != null)
			{
				try
				{
					modEntry.OnSaveGUI(modEntry);
				}
				catch (Exception e)
				{
					modEntry.Logger.LogException("OnSaveGUI", e);
				}
			}
		}
	}

	private static void CheckModUpdates()
	{
		PFLog.UnityModManager.Log("Checking updates.");
		if (!HasNetworkConnectionFixed())
		{
			PFLog.UnityModManager.Log("No network connection or firewall blocked.");
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ModEntry modEntry in ModEntries)
		{
			if (!string.IsNullOrEmpty(modEntry.Info.Repository))
			{
				hashSet.Add(modEntry.Info.Repository);
			}
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		foreach (string item in hashSet)
		{
			if (unityVersion < new Version(5, 4))
			{
				((MonoBehaviour)UI.Instance).StartCoroutine(DownloadString_5_3(item, ParseRepository));
			}
			else
			{
				((MonoBehaviour)UI.Instance).StartCoroutine(DownloadString(item, ParseRepository));
			}
		}
	}

	private static void ParseRepository(string json, string url)
	{
		if (string.IsNullOrEmpty(json))
		{
			return;
		}
		try
		{
			Repository repository = json.FromJson<Repository>();
			if (repository == null || repository.Releases == null || repository.Releases.Length == 0)
			{
				return;
			}
			Repository.Release[] releases = repository.Releases;
			foreach (Repository.Release release in releases)
			{
				if (string.IsNullOrEmpty(release.Id) || string.IsNullOrEmpty(release.Version))
				{
					continue;
				}
				ModEntry modEntry = FindMod(release.Id);
				if (modEntry != null)
				{
					Version version = ParseVersion(release.Version);
					if (modEntry.Version < version && (modEntry.NewestVersion == null || modEntry.NewestVersion < version))
					{
						modEntry.NewestVersion = version;
					}
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Log($"Error checking mod updates on '{url}'.");
			PFLog.UnityModManager.Log(ex.Message);
		}
	}

	public static bool HasNetworkConnectionFixed()
	{
		if (HasNetworkConnection())
		{
			return true;
		}
		try
		{
			using Ping ping = new Ping();
			IPAddress[] hostAddresses = Dns.GetHostAddresses("www.google.com");
			PingReply pingReply = ping.Send(hostAddresses.First((IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork), 3000);
			if (pingReply != null && pingReply.Status == IPStatus.Success)
			{
				return true;
			}
			PFLog.UnityModManager.Error($"Checking for network failed with status {pingReply.Status}");
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Exception(ex, "Exception while checking internet connection.");
		}
		return false;
	}

	private static bool HasNetworkConnection()
	{
		try
		{
			using Ping ping = new Ping();
			return ping.Send("www.google.com.mx", 3000).Status == IPStatus.Success;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		return false;
	}

	private static IEnumerator DownloadString(string url, UnityAction<string, string> handler)
	{
		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.Send();
		Version ver = ParseVersion(Application.unityVersion);
		MethodInfo isError = ((ver.Major < 2017) ? typeof(UnityWebRequest).GetMethod("get_isError") : typeof(UnityWebRequest).GetMethod("get_isNetworkError"));
		if (isError == null || (bool)isError.Invoke(www, null))
		{
			PFLog.UnityModManager.Log(www.error);
			PFLog.UnityModManager.Log($"Error downloading '{url}'.");
		}
		else
		{
			handler.Invoke(www.downloadHandler.text, url);
		}
	}

	private static IEnumerator DownloadString_5_3(string url, UnityAction<string, string> handler)
	{
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			PFLog.UnityModManager.Log(www.error);
			PFLog.UnityModManager.Log($"Error downloading '{url}'.");
		}
		else
		{
			handler.Invoke(www.text, url);
		}
	}

	public static void OpenUnityFileLog()
	{
		new Thread((ThreadStart)delegate
		{
			Thread.CurrentThread.IsBackground = true;
			string[] array = new string[2]
			{
				Application.persistentDataPath,
				Application.dataPath
			};
			string[] array2 = new string[2] { "Player.log", "output_log.txt" };
			string[] array3 = array;
			foreach (string path in array3)
			{
				string[] array4 = array2;
				foreach (string path2 in array4)
				{
					string text = Path.Combine(path, path2);
					if (File.Exists(text))
					{
						Thread.Sleep(500);
						Application.OpenURL(text);
						return;
					}
				}
			}
		}).Start();
	}

	public static Version ParseVersion(string str)
	{
		string[] array = str.Split('.', StringSplitOptions.None);
		if (array.Length >= 3)
		{
			Regex regex = new Regex("\\D");
			return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
		}
		if (array.Length >= 2)
		{
			Regex regex2 = new Regex("\\D");
			return new Version(int.Parse(regex2.Replace(array[0], "")), int.Parse(regex2.Replace(array[1], "")));
		}
		if (array.Length >= 1)
		{
			Regex regex3 = new Regex("\\D");
			return new Version(int.Parse(regex3.Replace(array[0], "")), 0);
		}
		PFLog.UnityModManager.Error("Error parsing version " + str);
		return new Version();
	}

	public static bool IsUnixPlatform()
	{
		int platform = (int)Environment.OSVersion.Platform;
		return platform == 4 || platform == 6 || platform == 128;
	}

	public static bool IsMacPlatform()
	{
		int platform = (int)Environment.OSVersion.Platform;
		return platform == 6;
	}

	public static bool IsLinuxPlatform()
	{
		int platform = (int)Environment.OSVersion.Platform;
		return platform == 4 || platform == 128;
	}
}

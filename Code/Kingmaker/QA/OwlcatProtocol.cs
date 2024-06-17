using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Core.Cheats;
using Core.Reflection;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.QA.Arbiter;
using Kingmaker.QA.Clockwork;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.QA;

public class OwlcatProtocol
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public sealed class ProtocolHandlerAttribute : Attribute
	{
		public string Address { get; set; }
	}

	private static Dictionary<string, MethodInfo> s_ProtocolHandlers;

	[ProtocolHandler(Address = "owlcat://remote-save/")]
	private static void RemoteSaveCommand(string value)
	{
		SavesStorageAccess.Load(value);
	}

	[ProtocolHandler(Address = "owlcat://view-point/")]
	private static void ViewPointCommand(string value)
	{
		Func<string, float> obj = delegate(string strparam)
		{
			try
			{
				strparam = strparam.Replace("$", "");
				return float.Parse(strparam, CultureInfo.InvariantCulture);
			}
			catch
			{
				return 0f;
			}
		};
		CommandLineArguments commandLineArguments = CommandLineArguments.Parse(value.Split('&'));
		string areaName = commandLineArguments.Get("area")?.Replace("$", "");
		string areaEnterPointName = commandLineArguments.Get("enterpoint")?.Replace("$", "");
		float x = obj(commandLineArguments.Get("x"));
		float y = obj(commandLineArguments.Get("y"));
		float z = obj(commandLineArguments.Get("z"));
		float rotation = obj(commandLineArguments.Get("rotation"));
		float zoom = obj(commandLineArguments.Get("zoom"));
		string instruction = commandLineArguments.Get("instruction")?.Replace("$", "");
		int id = (int)obj(commandLineArguments.Get("id"));
		ArbiterInstantMoveCameraStarter arbiterInstantMoveCameraStarter = new ArbiterInstantMoveCameraStarter();
		if (value.Contains("instruction"))
		{
			arbiterInstantMoveCameraStarter.AddArbiterMoveCameraTask(instruction, id);
		}
		else
		{
			arbiterInstantMoveCameraStarter.AddSimpleMoveCameraTask(areaName, areaEnterPointName, x, y, z, rotation, zoom);
		}
	}

	[ProtocolHandler(Address = "owlcat://clockwork/")]
	private static void ClockworkCommand(string value)
	{
		if (Application.isPlaying)
		{
			Kingmaker.QA.Clockwork.Clockwork.Instance.Start(value);
		}
	}

	[ProtocolHandler(Address = "owlcat://unity-exit/")]
	private static void UnityExitCommand()
	{
		if (Kingmaker.QA.Clockwork.Clockwork.IsRunning)
		{
			Kingmaker.QA.Clockwork.Clockwork.Instance.Reporter.ForceReport();
		}
		if (Application.isPlaying)
		{
			Application.Quit();
		}
	}

	private static void CollectHandlers()
	{
		if (s_ProtocolHandlers.Empty())
		{
			s_ProtocolHandlers = (from x in (from x in typeof(OwlcatProtocol).GetMembersSafe(BindingFlags.Static | BindingFlags.NonPublic)
					select x as MethodInfo).NotNull()
				where x.HasAttribute<ProtocolHandlerAttribute>()
				select x).ToDictionary((MethodInfo x) => x.GetAttribute<ProtocolHandlerAttribute>().Address);
		}
	}

	[Cheat(Name = "owlcat_protocol")]
	public static void OwlcatProtocolHandler(string message)
	{
		CollectHandlers();
		string[] array = message.Split('?');
		if (array.Length == 0)
		{
			PFLog.Default.Error("OwlcatProtocol: No command specified");
			return;
		}
		string text = array[0];
		string text2 = ((array.Length > 1) ? array[1] : string.Empty);
		if (!s_ProtocolHandlers.TryGetValue(array[0], out var value))
		{
			PFLog.Default.Error("OwlcatProtocol: No handler for command [" + text + "]");
			return;
		}
		value.Invoke(null, new object[1] { text2 });
	}
}

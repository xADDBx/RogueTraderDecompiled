using System;
using System.IO;
using System.Threading.Tasks;
using Core.Cheats;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Cheats;

internal static class CheatsHelper
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private const string RelativeConfigPath = "..\\Config";

	[Cheat]
	public static async Task Exec(string path, bool silent = false)
	{
		string text = Path.Combine(Application.dataPath, "..\\Config", path);
		if (!File.Exists(text))
		{
			if (silent)
			{
				return;
			}
			throw new FileNotFoundException("File not found", text);
		}
		await using FileStream stream = File.OpenRead(text);
		using StreamReader reader = new StreamReader(stream);
		for (string text2 = await reader.ReadLineAsync(); text2 != null; text2 = await reader.ReadLineAsync())
		{
			try
			{
				await (CheatsManagerHolder.Instance?.Execute(text2) ?? Task.CompletedTask);
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
		}
	}

	public static async void Run(string line)
	{
		try
		{
			await CheatsManagerHolder.System.Parser.Execute(line);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}
}

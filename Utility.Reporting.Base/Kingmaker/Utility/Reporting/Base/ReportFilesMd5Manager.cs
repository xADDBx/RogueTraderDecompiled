using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Kingmaker.Utility.Reporting.Base;

public class ReportFilesMd5Manager
{
	public Action<string> ReportError;

	public const string CheckSumsFileName = "checksums";

	public const string Separator = "===========\n";

	public string GetCompareChecksumsString(string filePath)
	{
		string fileName = Path.GetFileName(filePath);
		string directoryName = Path.GetDirectoryName(filePath);
		string text = CompareChecksums(fileName, directoryName);
		if (!string.IsNullOrEmpty(text))
		{
			return fileName + ": " + text + "\n";
		}
		return text;
	}

	private string CompareChecksums(string fileName, string fileFolder)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string text = Path.Combine(fileFolder, "_temp");
		string sourceArchiveFileName = Path.Combine(fileFolder, fileName);
		string text2 = "";
		try
		{
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			ZipFile.ExtractToDirectory(sourceArchiveFileName, text);
			string input;
			try
			{
				input = File.ReadAllText(Path.Combine(text, "checksums"));
			}
			catch (Exception)
			{
				text2 = "no_checksum_info";
				throw;
			}
			string[] array = Regex.Split(input, "===========\n");
			foreach (string text3 in array)
			{
				try
				{
					string[] array2 = text3.Split(new string[1] { ": " }, StringSplitOptions.None);
					string text4 = string.Empty;
					string value = string.Empty;
					try
					{
						text4 = array2[0];
					}
					catch
					{
					}
					try
					{
						value = array2[1].Replace("\n", "");
					}
					catch
					{
					}
					if (!string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(value))
					{
						if (dictionary.ContainsKey(text4))
						{
							dictionary[text4] = value;
						}
						else
						{
							dictionary.Add(text4, value);
						}
					}
				}
				catch (Exception ex2)
				{
					ReportError?.Invoke("Exceptions compare save checksums: \n" + ex2.Message + "\n" + ex2.StackTrace);
				}
			}
			array = Directory.GetFiles(text);
			foreach (string path in array)
			{
				string fileName2 = Path.GetFileName(path);
				if (fileName2 == "checksums" || fileName2 == "history" || fileName2.StartsWith("header") || fileName2 == "statistic.json" || fileName2.EndsWith(".png") || fileName2.EndsWith(".fog"))
				{
					continue;
				}
				try
				{
					using MD5 mD = MD5.Create();
					using FileStream inputStream = File.OpenRead(path);
					string fileName3 = Path.GetFileName(path);
					if (dictionary.ContainsKey(fileName3) && Encoding.Default.GetString(mD.ComputeHash(inputStream)).Replace("\n", "") != dictionary[fileName3])
					{
						if (string.IsNullOrEmpty(text2))
						{
							text2 = "Modified save files: ";
						}
						text2 = text2 + fileName3 + " ";
					}
				}
				catch (Exception ex3)
				{
					ReportError?.Invoke("Exceptions compare save checksums: \n" + ex3.Message + "\n" + ex3.StackTrace);
				}
			}
		}
		catch (Exception ex4)
		{
			if (text2 != "no_checksum_info")
			{
				text2 += "Exceptions!";
			}
			ReportError?.Invoke("Failed compare save checksums: \n" + ex4.Message + "\n" + ex4.StackTrace);
		}
		finally
		{
			try
			{
				Directory.Delete(text, recursive: true);
			}
			catch (Exception ex5)
			{
				ReportError?.Invoke("Failed compare save checksums: \n" + ex5.Message + "\n" + ex5.StackTrace);
			}
		}
		return text2;
	}
}

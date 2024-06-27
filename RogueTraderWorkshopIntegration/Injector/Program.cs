using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Injector;

internal class Program
{
	private static void Main()
	{
		string text = FindInstallLocation();
		if (text == "")
		{
			Console.WriteLine("Could not find Steam Game File location (by scanning Player.Log). Patching failed.");
			return;
		}
		string text2 = Path.Combine(text, "WH40KRT_Data", "Managed");
		string text3 = Path.Combine(text2, "RogueTrader.ModInitializer.dll");
		FileInfo fileInfo = new FileInfo(text3);
		FileInfo fileInfo2 = new FileInfo(text3 + ".orig");
		if (fileInfo2.Exists)
		{
			fileInfo2.Delete();
		}
		fileInfo.MoveTo(text3 + ".orig");
		try
		{
			CustomAssemblyResolver assemblyResolver = new CustomAssemblyResolver(text2);
			ReaderParameters parameters = new ReaderParameters
			{
				AssemblyResolver = assemblyResolver
			};
			AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(text3 + ".orig", parameters);
			MethodDefinition methodDefinition = assemblyDefinition.MainModule.Types.FirstOrDefault((TypeDefinition t) => t.FullName == "Code.GameCore.Modding.ModInitializer")?.Methods.FirstOrDefault((MethodDefinition m) => m.Name == "InitializeMods");
			ILProcessor iLProcessor = methodDefinition.Body.GetILProcessor();
			AssemblyDefinition assemblyDefinition2 = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
			TypeReference typeReference = assemblyDefinition.MainModule.ImportReference(assemblyDefinition2.MainModule.Types.First((TypeDefinition t) => t.Name == "SteamWorkshopIntegration"));
			MethodReference method = new MethodReference("Start", assemblyDefinition.MainModule.TypeSystem.Void, typeReference)
			{
				HasThis = true
			};
			MethodReference method2 = new MethodReference("get_Instance", typeReference, typeReference)
			{
				HasThis = false
			};
			Instruction instruction = iLProcessor.Create(OpCodes.Call, method2);
			Instruction instruction2 = iLProcessor.Create(OpCodes.Callvirt, method);
			if (InststructionEquals(instruction, methodDefinition.Body.Instructions.ElementAt(0)) && InststructionEquals(instruction2, methodDefinition.Body.Instructions.ElementAt(1)))
			{
				Console.WriteLine("Game Files already patched.");
			}
			else
			{
				Instruction target = methodDefinition.Body.Instructions.First();
				iLProcessor.InsertBefore(target, instruction);
				iLProcessor.InsertBefore(target, instruction2);
				Console.WriteLine("Succeeded in Patching.");
			}
			assemblyDefinition.Write(text3);
			assemblyDefinition.Dispose();
			File.Delete(text3 + ".orig");
			string text4 = Path.Combine(text2, Assembly.GetExecutingAssembly().GetName().Name + ".dll");
			if (!File.Exists(text4))
			{
				File.Copy(Assembly.GetExecutingAssembly().Location, text4);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			File.Move(text3 + ".orig", text3);
		}
	}

	private static bool InststructionEquals(Instruction i1, Instruction i2)
	{
		if (i1.OpCode == i2.OpCode)
		{
			return i1.Operand.ToString() == i2.Operand.ToString();
		}
		return false;
	}

	private static string FindInstallLocation()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Owlcat Games", "Warhammer 40000 Rogue Trader", "Player.log");
			string value = "Mono path[0]";
			string input = null;
			foreach (string item in File.ReadLines(path))
			{
				if (item.Contains(value))
				{
					input = item;
					break;
				}
			}
			string pattern = "^Mono path\\[0\\] = '(.*?)/WH40KRT_Data/Managed'$";
			Match match = Regex.Match(input, pattern);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
		}
		return "";
	}
}

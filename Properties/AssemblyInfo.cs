using System.Reflection;
using MelonLoader;
using DebugMode;

[assembly: AssemblyTitle(ModInfo.Description)]
[assembly: AssemblyDescription(ModInfo.Description)]
[assembly: AssemblyCompany(ModInfo.Company)]
[assembly: AssemblyProduct(ModInfo.Name)]
[assembly: AssemblyCopyright("Created by " + ModInfo.Author)]
[assembly: AssemblyTrademark(ModInfo.Company)]
[assembly: AssemblyVersion(ModInfo.Version)]
[assembly: AssemblyFileVersion(ModInfo.Version)]
[assembly: MelonInfo(typeof(DebugModeMod), ModInfo.Name, ModInfo.Version, ModInfo.Author, ModInfo.DownloadLink)]

[assembly: MelonGame("poncle", "VampireSurvivors")]
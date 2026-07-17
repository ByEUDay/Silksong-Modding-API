# This is a Hollow Knight: Silksong mod loader that does not rely on BepInEx.
##   How to install: 
1.Download the API.

2.Open the folder and run SilksongPrepatcher.exe or the SilksongPrepatcher file. (Using macOS as an example (not sure about Windows) It's something like SilksongPrepatcher [Managed folder directory] ModLoader.dll (in the same folder as SilksongPrepatcher)).

3.After finishing the installation, copy all the .dll files in the directory (like SilksongModLoader.dll, MonoMod.RuntimeDetour.dll, etc.) into the Managed folder.

# If you want to create a Mod: 
1. Download the API.
2. Open the .sln file.
3. Fill in the following: Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Mod Name", "Mod Name.csproj file", "{33333333-3333-3333-3333-333333333333}" (starting from three, write the number corresponding to the Mod). EndProject and {33333333-3333-3333-3333-333333333333}.Mod Name|Any CPU.ActiveCfg = Debug|Any CPU {33333333-3333-3333-3333-333333333333}.Debug|Any CPU.Build.0 = Debug|Any CPU {33333333-3333-3333-3333-333333333333}.Release|Any CPU.ActiveCfg = Release|Any CPU {33333333-3333-3333-3333-333333333333}.Release|Any CPU.Build.0 = Release|Any CPU.
4. Like:
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "DebugMod", "DebugMod\DebugMod.csproj", "{33333333-3333-3333-3333-333333333333}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
		{33333333-3333-3333-3333-333333333333}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{33333333-3333-3333-3333-333333333333}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{33333333-3333-3333-3333-333333333333}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{33333333-3333-3333-3333-333333333333}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
5. Open the .props file and fill in the path to the Managed file.
6. Compile.

# [EncosyTower.SourceGen][source-gen-csproj]

[source-gen-csproj]: EncosyTower.SourceGen/EncosyTower.SourceGen.csproj

## Build Artifacts

```xml
<Target Name="CopyBuildArtifacts" AfterTargets="Build" Condition=" '$(Configuration)'=='Release' ">
    <ItemGroup>
        <DataFiles Include="$(ProjectDir)$(OutDir)*.dll" />
    </ItemGroup>
    <Copy SourceFiles="@(DataFiles)" DestinationFolder="$(ProjectDir)../../../Packages/com.laicasaane.encosy-tower/EncosyTower.Core/SourceGenerators/" SkipUnchangedFiles="true" />
</Target>
```

- After the Release build is completed, artifacts will be copied to the folder
`Packages/com.laicasaane.encosy-tower/EncosyTower.Core/SourceGenerators/`.

## NuGet Packages

```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.3.1" PrivateAssets="all" />
<PackageReference Include="Microsoft.CodeAnalysis.Workspaces.Common" Version="4.3.1" />
```

- [Microsoft.CodeAnalysis.CSharp][codeanalysis-csharp] enables code analysis
and source generator[^1].
- [Microsoft.CodeAnalysis.Workspaces.Common][codeanalysis-workspaces] enables code fixes[^2].

[codeanalysis-csharp]: https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp

[codeanalysis-workspaces]: https://www.nuget.org/packages/Microsoft.CodeAnalysis.Workspaces.Common

[^1]: [Source Generator Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)

[^2]: [How To Write a C# Analyzer and Code Fix](https://github.com/dotnet/roslyn/blob/main/docs/wiki/How-To-Write-a-C%23-Analyzer-and-Code-Fix.md)

**Notes:**
- Both Unity *2022.3* and *6000.0* are using **Roslyn Analyzer 4.3.1**.
- The version can be found at `[UnityInstallRootPath]/Editor/Data/DotNetSdkRoslyn/csc.deps.json`.

## Debugging

Requirements for debugging a source generator project:

1. Must install the [.NET Compiler Platform SDK][net-compiler-installing].
2. Set the source generator project as the startup project[^3].
3. The [launchSettings.json][launch-settings-json] file locates at the `Properties/` folder specifies
the target project on which source generators will run.

```json
{
  "profiles": {
    "EncosyTower.SourceGen": {
      "commandName": "DebugRoslynComponent",
      "targetProject": "../EncosyTower.SourceGen.Tests/EncosyTower.SourceGen.Tests.csproj"
    }
  }
}
```

[net-compiler-installing]: https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix#install-using-the-visual-studio-installer---workloads-view

[launch-settings-json]: EncosyTower.SourceGen/Properties/launchSettings.json

[^3]: In Visual Studio, right-click on the project and select `Set as Startup Project`.

# [EncosyTower.SourceGen.Tests][source-gen-tests-csproj]

[source-gen-tests-csproj]: EncosyTower.SourceGen.Tests/EncosyTower.SourceGen.Tests.csproj

## References Unity Assemblies

### NuGet Package

```xml
<PackageReference Include="Unity3D" Version="3.0.0" />
```

- This [Unity3D][unity3d][^4] package allows a C# project to reference assemblies of the Unity game engine.

[unity3d]: https://www.nuget.org/packages/Unity3D

[^4]: [Unity3D NuGet Package Usage](https://github.com/Rabadash8820/UnityAssemblies/blob/main/docs/v3/usage.md)

### [Directory.Build.props][directory-build-props]

[directory-build-props]: EncosyTower.SourceGen.Tests/Directory.Build.props

#### Declares the Unity Installation Path

```xml
<UnityInstallRootPath>$([System.Environment]::GetEnvironmentVariable('UNITY_OS_INSTALL_ROOT'))</UnityInstallRootPath>
```

- This is the path to the Unity installation folder.
- In this case it is defined as an environment variable `UNITY_OS_INSTALL_ROOT` on the operating system.

#### Declares the Unity Project Path

```xml
<UnityProjectPath>$(MSBuildProjectDirectory)/../../../</UnityProjectPath>
```

- This is the path to the root folder of this Tower of Encosy project.

#### Reference All Assemblies

```xml
<Reference Include="$(UnityEditorPath)" Private="false" />
```

- Unity Assemblies can be found inside the folder `[UnityProjectPath]/Library/ScriptAssemblies/`.

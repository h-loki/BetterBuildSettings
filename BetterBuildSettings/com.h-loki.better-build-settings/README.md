[← Back to main README](../../README.md)

# Better Build Settings - Core

Modular build profiles for Unity.
Better Build Settings helps you keep build configuration in one place: modules, output folder, build folder name, executable name, and reusable naming variables.

## What it does

- Saves build settings as named profiles.
- Lets you enable only the modules you need for a build.
- Creates the output folder automatically.
- Supports custom build folder and executable names.
- Supports naming variables like `%configName%`, `%productName%`, and `%date:yyyyMMdd_HHmmss%`.

### Installation
Add the package through Unity Package Manager:
```
https://github.com/h-loki/BetterBuildSettings.git?path=/BetterBuildSettings/com.h-loki.better-build-settings
```
### Requirements:

- Unity 2021.3+
- Odin Inspector

## Usage

Open the window:
```
Tools → Build Settings
```
Create or select a profile, configure the output settings, enable the modules you need, then press BUILD.

### Output naming

You can customize:

- build root folder;
- generated build folder name;
- executable file name.

Available variables depend on registered token providers. Built-in variables include:
```
%configName%
%productName%
%target%
%date%
%date:yyyyMMdd_HHmmss%
%time%
```

Example:
```
%configName%_%date:yyyyMMdd_HHmmss%
```

## Custom naming variables

Projects can add their own naming variables by registering an `IBuildNameTokenProvider` from editor code.

Example:

```csharp
using BetterBuildSettings.Editor.Output.Naming;
using UnityEditor;

[InitializeOnLoad]
public static class ProjectBuildNameTokens
{
    static ProjectBuildNameTokens()
    {
        BuildNameTokenRegistry.Register(new Provider());
    }

    private sealed class Provider : IBuildNameTokenProvider
    {
        public bool TryResolve(
            string token,
            string argument,
            BuildContext context,
            out string value)
        {
            if (token == "branch")
            {
                value = System.Environment.GetEnvironmentVariable("GIT_BRANCH");

                if (string.IsNullOrWhiteSpace(value))
                    value = "local";

                return true;
            }

            value = null;
            return false;
        }

        public IEnumerable<BuildNameTokenDescriptor> GetTokenDescriptors()
        {
            yield return new BuildNameTokenDescriptor(
                "branch",
                "%branch%",
                "Current Git branch from the GIT_BRANCH environment variable.");
        }
    }
}
```
Use it in a pattern:
```
%configName%_%branch%_%date:yyyyMMdd_HHmmss%
```


### Notes

Current build target support is focused on Windows standalone builds.

using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBuildSettings.Core
{
    public static class BuildModuleRegistry
    {
        private static readonly Dictionary<string, Func<IBuildModule>> Factories = new();

        public static void Register<TModule>() where TModule : IBuildModule, new()
        {
            var module = new TModule();
            Register(module.Id, () => new TModule());
        }

        public static void Register(string moduleId, Func<IBuildModule> factory)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentException("Module id is required.", nameof(moduleId));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Factories[moduleId] = factory;
        }

        public static IReadOnlyList<IBuildModule> CreateModules()
        {
            return Factories
                .Values
                .Select(factory => factory())
                .Where(module => module != null)
                .OrderBy(module => module.DisplayName)
                .ToArray();
        }
    }
}
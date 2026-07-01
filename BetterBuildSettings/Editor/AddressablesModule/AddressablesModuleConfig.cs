using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor.AddressableAssets;


namespace BetterBuildSettings.Editor.AddressablesModule
{
    [Serializable]
    public sealed class AddressablesModuleConfig
    {
        public bool restoreAfterBuild = true;
        [ValueDropdown(nameof(GetAddressableGroupNames), IsUniqueList = true)]
        public List<string> enabledGroups = new();

        private IEnumerable<string> GetAddressableGroupNames()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                yield break;

            foreach (var group in settings.groups)
            {
                if (group == null)
                    continue;

                yield return group.Name;
            }
        }
    }
}
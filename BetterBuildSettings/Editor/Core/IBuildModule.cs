using Newtonsoft.Json.Linq;

namespace BetterBuildSettings.Editor.Core
{
    public interface IBuildModule
    {
        string Id { get; }
        string DisplayName { get; }
        JObject CreateDefaultPayload();
        void Deserialize(JObject payload);
        JObject Serialize();
        void Apply(BuildContext context);
        void Restore(BuildContext context);
    }
}
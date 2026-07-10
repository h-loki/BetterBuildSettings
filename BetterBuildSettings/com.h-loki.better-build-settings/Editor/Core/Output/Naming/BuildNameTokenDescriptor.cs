namespace BetterBuildSettings.Core.Output.Naming
{
    public readonly struct BuildNameTokenDescriptor
    {
        public readonly string Token;
        public readonly string Usage;
        public readonly string Description;

        public BuildNameTokenDescriptor(
            string token,
            string usage,
            string description)
        {
            Token = token;
            Usage = usage;
            Description = description;
        }
    }
}
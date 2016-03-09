namespace PSEGetLib.Converters
{
    public sealed class MetaOutputSettings : OutputSettings
    {
        public bool UseSingleDirectory { get; set; }

        public MetaOutputSettings()
        {
            UseSingleDirectory = false;
        }

        public string OutputDirectory { get; set; }

    }
}

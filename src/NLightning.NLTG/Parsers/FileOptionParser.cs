namespace NLightning.NLTG.Parsers;

public static class FileOptionParser
{
    public static Options GetOptionsFromFile(string configFile)
    {
        return Options.FromFile(configFile);
    }
}
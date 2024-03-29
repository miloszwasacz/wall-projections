using System;

namespace WallProjections.Models;

/// <summary>
/// Base <see cref="Exception"/> for any expected issues with the config/file management.
/// </summary>
public class ConfigException : Exception
{
    public override string Message => "Something went wrong managing the config";
}

/// <summary>
/// <see cref="ConfigException"/> for if the config package could not be found for importing.
/// </summary>
public class ConfigPackageNotFoundException : ConfigException
{
    /// <summary>
    /// Name of the config package which could not be found.
    /// </summary>
    private string _fileName { get; }

    public override string Message => $"Could not find .wall package with name {_fileName}";

    public ConfigPackageNotFoundException(string fileName)
    {
        _fileName = fileName;
    }
}

public class ConfigNotImportedException : ConfigException
{
    public override string Message => "No config is currently loaded into the program";
}

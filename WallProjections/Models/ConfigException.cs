using System;

namespace WallProjections.Models;

/// <summary>
/// Base <see cref="Exception"/> for any expected issues with the config/file management.
/// </summary>
public abstract class ConfigException : Exception
{
    public override string Message => "Something went wrong managing the config";

    protected ConfigException(Exception cause) : base(null, cause)
    {
    }
}

/// <summary>
/// Thrown if the config package has invalid format.
/// </summary>
public class ConfigPackageFormatException : ConfigException
{
    /// <summary>
    /// Name of the config package which has invalid format.
    /// </summary>
    private readonly string _fileName;

    public override string Message => $".wall package {_fileName} not valid";

    public ConfigPackageFormatException(string fileName, Exception cause) : base(cause)
    {
        _fileName = fileName;
    }
}

/// <summary>
/// Thrown if the config that is imported is invalid
/// </summary>
public class ConfigInvalidException : ConfigException
{
    public override string Message => "Current config is invalid/corrupt";

    public ConfigInvalidException(Exception cause) : base(cause)
    {
    }
}

/// <summary>
/// Thrown if there is not a config imported to the application path when required.
/// </summary>
public class ConfigNotImportedException : ConfigException
{
    public override string Message => "No config is currently loaded into the program";

    public ConfigNotImportedException(Exception cause) : base(cause)
    {
    }
}

/// <summary>
/// Thrown if there is an issue reading/writing to the required locations in the imported config.
/// </summary>
public class ConfigIOException : ConfigException
{
    private readonly string? _filePath;

    public override string Message =>
        _filePath is null ? "Unable to access loaded config files" : $"Unable to access {_filePath}";

    /// <summary>
    /// Constructor if specific path where issue occurred not relevant.
    /// </summary>
    public ConfigIOException(Exception cause) : base(cause)
    {
    }

    /// <summary>
    /// Constructor to tell user path where issue occurred.
    /// </summary>
    /// <param name="filePath"></param>
    public ConfigIOException(string filePath, Exception cause) : base(cause)
    {
        _filePath = filePath;
    }
}

/// <summary>
/// Thrown if an external file could not be read.
/// </summary>
public class ExternalFileReadException : ConfigException
{
    /// <summary>
    /// Name of file which could not be read.
    /// </summary>
    private readonly string _fileName;

    public override string Message => $"Could not find/access {_fileName}";

    /// <summary>
    /// Constructor for <see cref="ExternalFileReadException"/>
    /// </summary>
    /// <param name="fileName">Name of file which could not be read.</param>
    public ExternalFileReadException(string fileName, Exception cause) : base(cause)
    {
        _fileName = fileName;
    }
}

/// <summary>
/// Thrown if a file of the same name already exists.
/// </summary>
public class ConfigDuplicateFileException : ConfigException
{
    /// <summary>
    /// Name of duplicate file.
    /// </summary>
    private readonly string _filePath;

    public override string Message => $"File already exists at {_filePath}";

    /// <summary>
    /// Constructor for <see cref="ConfigDuplicateFileException"/>
    /// </summary>
    /// <param name="fileName">The duplicate imported file name.</param>
    public ConfigDuplicateFileException(string filePath, Exception cause) : base(cause)
    {
        _filePath = filePath;
    }
}

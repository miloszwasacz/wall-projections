using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IContentProvider
{
    /// <summary>
    /// Searches for a media files in the hotspot folder and returns a <see cref="Hotspot.Media"/> object
    /// containing all the data and media paths associated with the hotspot
    /// </summary>
    /// <param name="hotspotId">The <see cref="Hotspot.Id"/> of a <see cref="Hotspot"/></param>
    /// <returns>A container with Hotspot's data</returns>
    /// <exception cref="Config.HotspotNotFoundException">
    /// The hotspot with the given <paramref name="hotspotId">ID</paramref> does not exist
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The hotspot folder does not contain any <i><b>.txt</b></i> files (required to get the description of the hotspot)
    /// </exception>
    public Hotspot.Media GetMedia(int hotspotId);
}

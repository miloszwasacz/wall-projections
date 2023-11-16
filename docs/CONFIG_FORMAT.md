# Formatting for zip File
## config.json
In the root directory of the zip file, place `config.json` with the JSON in the format:
```json
{
    "Hotspots": [
        {
            "Id": 0, // Numerical ID for hotspot. Must match button inputs 1,2,3,4
            "Position": {
                "X": 0, // Numerical value for X position of hotspot on screen. Can be non-integer.
                "Y": 1, // Numerical value for Y position of hotspot on screen. Can be non-integer.
                "R": 2 // Numerical value for radius of hotspot on screen. Can be non-integer.
            }
        }
    ]
}
```
## Media Files
Place all folders and files for media inside a `Media` folder next to `config.json`.

For each `Hotspot` in `config.json`, create a folder in `Media` with matching ID.
> **Example:**
> If `Hotspot.Id == 1`, then create a folder called `1` inside `Media`.

### In this folder, place your media files. These must be:
- `Id.txt`, where ID is `Hotspot.Id`

### And one of:
- `Id.jpeg`
- `Id.JPEG`
- `Id.jpg`
- `Id.JPG`
- `Id.png`
- `Id.PNG`
- `Id.mp4`

where `Id` is `Hotspot.Id` (files can be any name but using `Id` is best for 
readability).

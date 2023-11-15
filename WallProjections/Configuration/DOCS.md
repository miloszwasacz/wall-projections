# Docs for `Configuration`

## Loading zip files
- Import the zip file using `ContentImporter.Load(zipFile)` where `zipFile` is the
path to the zip file.
- The returned `Config` object will be the loaded config and 
all media will have been loaded into a temporary folder for easier access.

## Clean up temporary folder
- Run on shutdown of application.
- Call `ContentImporter.Cleanup(config)` where `config` is the loaded config from above.
- This deletes temporary folder and extracted media.

## Getting folder path for `Hotspot`
1. Call `config.GetHotspot(id)`, where `id` is the ID of the hotspot.
2. Call `IConfig.GetHotspotFolder(hotspot)` where `hotspot` is earlier received hotspot.

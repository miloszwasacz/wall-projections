# Docs for `Configuration`

## Loading zip files
- Instantiate `ContentImporter`
- Import the zip file using `contentImporter.Load(zipFile)` where `zipFile` is the
path to the zip file.
- The returned `IConfig` object will be the loaded config and 
all media will have been loaded into a temporary folder for easier access.

## Clean up temporary folder
- Run on shutdown of application.
- Call `contentImporter.Cleanup(config)` where 
  - `contentImporter` is instantiated `ContentImporter` from above.
  - `config` is the loaded config from above.
- This deletes temporary folder and extracted media.

## Getting folder path for `Hotspot`
1. Call `config.GetHotspot(id)`, where
   - `id` is the ID of the hotspot.
   - `config` is `IConfig` received from `contentImporter.Load()`
2. Call `contentImporter.GetHotspotMediaFolder(hotspot)` where 
   - `hotspot` is earlier received hotspot.
   - `contentImporter` is instantiated `ContentImporter` from zip file load.

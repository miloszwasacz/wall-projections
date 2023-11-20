# Docs for `ContentCache`

## Loading zip files
- Instantiate `ContentCache`
- Import the zip file using `contentCache.Load(zipFile)` where `zipFile` is the
path to the zip file.
- The returned `IConfig` object will be the loaded config and 
all media will have been loaded into a temporary folder for easier access.

## Clean up temporary folder
- Run on shutdown of application.
- Call `contentCache.Cleanup(config)` where 
  - `contentCache` is instantiated `ContentCache` from above.
  - `config` is the loaded config from above.
- This deletes temporary folder and extracted media.

## Getting folder path for `Hotspot`
1. Call `config.GetHotspot(id)`, where
   - `id` is the ID of the hotspot.
   - `config` is `IConfig` received from `contentCache.Load()`
2. Call `contentCache.GetHotspotMediaFolder(hotspot)` where 
   - `hotspot` is earlier received hotspot.
   - `contentCache` is instantiated `ContentCache` from zip file load.

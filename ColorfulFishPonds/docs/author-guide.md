Colorful Fish Ponds
=

Colorful Fish Ponds(CFP) by default makes every fish pond that normally doesn't change color change to the context tag color the fish is set to.

CFP uses content packs to allow for custom color choices.

Content Pack
-
Create a new folder in your Mods folder, naming convention dictates that you name it `[CFP] Mod Name`, but as long as your manifest is correct the folder name doesn't matter.

### Manifest
Create a manifest.json inside your content pack folder.

Example manifest.json:
```json
{
    "Name": "Content Pack Name",
    "Author": "yourName",
    "Version": "1.0.0",
    "Description": "Description.",
    "UniqueID": "yourName.contentPackName",
    "UpdateKeys": [],
    "Dependencies": [],
    "ContentPackFor": {
      "UniqueID": "rokugin.colorfulfishponds",
      "MinimumVersion": "1.0.0"
    }
}
```

### Content
Create a content.json inside your content pack folder, alongside your manifest.json.

Required block:
```json
{
    "Changes": {
        
    }
}
```

The pond color overrides you want go inside Changes.

Example content.json:
```json
{
    "Changes": {
        "Blobfish": {
            "FishID": "800",
            "RequiredPopulation": 1,
            "PondColor": {
                "R": 255,
                "G": 0,
                "B": 255
            }
        },
        "Coral": {
            "FishID": "393",
            "RequiredPopulation": 3,
            "PondColor": {
                "R": 215,
                "G": 117,
                "B": 222
            }
        }
    }
}
```

**Blobfish** is the entry key, it needs to be unique but doesn't need to be related to the actual fish you're overriding color for.

**FishID** is a string reference to the unqualified item ID of the fish you want to override the color of.
ID's can be found in Data/Objects or at https://mateusaquino.github.io/stardewids/

**RequiredPopulation** is the minimum amount of fish required to be in the pond in order for the color override to take effect.

**PondColor** holds the red, green, and blue values for the color override.

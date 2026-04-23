# Gulla.Episerver.ConvertBlocks

An Optimizely CMS 12 admin tool for converting blocks from one block type to another — the same functionality as the built-in "Convert Pages" tool, but for blocks.

## Features

- **Convert a single block** — enter a block content ID (find it in the URL when editing the block in edit mode)
- **Convert all blocks of a type** — select a source block type and convert every instance
- Property mapping — map properties from the source type to the target type, or remove them permanently
- Test mode — preview what would be converted before committing
- Appears in the CMS admin menu under Tools, right below "Convert Pages"

## Installation

```
dotnet add package Gulla.Optimizely.ConvertBlocks
```

Or install via the NuGet package manager. No `Startup.cs` changes required — the admin tool and menu item are registered automatically.

## Requirements

- Optimizely CMS 12
- .NET 6
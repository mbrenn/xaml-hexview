# xaml-hexview

The hexview is a xaml-control element which receives an array of bytes and displays it as a 
hex-view where each byte represents two hexadecimal figures. 

## API

The user control has the following properties: 

- **Bytes**: This byte array will be shown in the user control. 
- **FixedColumnCount**: If bigger than 0, the number of columns will be limited to this value. This
allows you to limit on 8 bytes per row.
- **ShowASCIITranslation**: Shows the translated bytes into the ASCII, so the user can get a visual 
representation of the byte array. 

# Usage via NuGet

Download the following package: **BurnSystems-HexView-Universal** via nuget. 

# Manual Usage 

Download the complete repository and compile the BurnSystem.HexView.Universal project via msbuild 
or Visual Studio 2015. 

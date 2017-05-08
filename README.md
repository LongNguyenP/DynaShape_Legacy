DynaShape is an open-sourced plugin for Dynamo the can be used for constraint-based form finding.

The following dependencies are required to build the source code. They are all available from NuGet
- Math.NET Numerics 3.11
- DynamoVisualProgramming.DynamoCoreNodes 1.2.0
- DynamoVisualProgramming.ZeroTouchLibrary 1.2.0
- DynamoVisualProgramming.WpfUILibrary 1.2.0
- HelixToolkit.Wpf.SharpDX (this package is relatively large, you actually only need HelixToolkit.Wpf.SharpDX.dll)

Alternatively, all the dependent dll files (except Math.NET numerics) can be found in the Dynamo Core main folder (e.g. C:\Program Files\Dynamo\Dynamo Core\1.x).

After you have successfully built DynaShape.dll from the source. You can assemble the package follow in the following structure

DynaShapePackage
..bin
....DynaShape.dll
....DynaShape_DynamoCustomization.xml (provided along with the source code)
....MathNet.Numerics.dll
..pkg.json (provided along with the source code)

For the mouse interaction to work, you need to manually edit the AssemblyPath inside the DynaShape_ViewExtensionDefinition.xml (provided along with the source code) so that it point correctly to the DynaShape.dll within the package folder, then place this xml into to [DynamoCoreMainFolder]\viewExtensions

If you have questions, feel free to contact me at longnguyen.gigabidea@gmail.com or on Twitter (@LongNguyenP)

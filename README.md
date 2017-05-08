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
--pkg.json (provided with the VS solution)
--bin
----DynaShape.dll
----DynaShape_DynamoCustomization.xml (provided with the VS solution)
----MathNet.Numerics.dll

For the mouse interaction to work, you need to manually edit the AssemblyPath inside the DynaShape_ViewExtensionDefinition.xml (provided with the VS solution) so that it points correctly to the DynaShape.dll within the package folder, then place this xml into to [DynamoCoreMainFolder]\viewExtensions

If you have questions, feel free to contact me at longnguyen.gigabidea@gmail.com or on Twitter (@LongNguyenP)

DynaShape is a WIP open-source Dynamo plugin for constraint-based form finding, optimization, and physics simulation.

The source code is written entirely in Visual C#. All the dependent dll files can be found in the the Dynamo Core main folder (e.g. C:\Program Files\Dynamo\Dynamo Core\1.x), except MathNet.Numerics.dll (which can be downloaded from NuGet, recommeded version 3.11 or above)

After DynaShape.dll has been built successfully from the source, you can then assemble the Dynamo package folder according to the following structure:


DynaShape

-- pkg.json (provided with the Visual Studio solution)

-- bin

---- DynaShape.dll
 
---- DynaShape_DynamoCustomization.xml (provided with the Visual Studio  solution)
 
---- MathNet.Numerics.dll


Once the package folder has been assembled, you can "install" the package to Dynamo by to "Dynamo Main Menu > Settings > Manage Node and Package Paths...", and add a path to the package folder.

For the mouse interaction to work, for now you will need to manually edit the AssemblyPath inside the DynaShape_ViewExtensionDefinition.xml (provided with the Visual Studio  solution) so that it points correctly to the DynaShape.dll within the package folder, and then place this xml into [DynamoCoreMainFolder]\viewExtensions, and restart Dynamo Studio and/or Revit


Once the package has been installed. You can start checking out these sample Dynamo scripts to see how DynaShape works:
https://drive.google.com/drive/folders/0B8GXDbjowDN_cng5RHVjU2kzeTg?usp=sharing

==========================================================

Contact Info:
- Email: longnguyen.gigabidea@gmail.com
- Twitter: @LongNguyenP

==========================================================

Acknowledgements:

I would like to acknowledge the following people:
- Ian Kenough and the Dynamo development team, for the great visual programming tool.
- The EPFL Computer Graphics Lab and Geometry Lab, for developing the essential mathematical and algorithm framework, which DynaShape is based on. http://lgg.epfl.ch/publications/2012/shapeup/index.php
- Daniel Piker, for playing a major role in popularizing physics and constraint-based digital form finding in the design community, mostly via his well-known plugin KangarooPhysics for Grasshopper. The highly positive response that KangarooPhysics receives from the community has inspired me to start DynaShape in hope to make similar computational design tools available to the Dynamo and BIM community.
http://www.grasshopper3d.com/group/kangaroo.
- Autodesk (particularly Phil Mueller and Matt Jezyk) for co-organizing and co-sponsoring AEC Hackathon Munich 2017, where DynaShape was born.
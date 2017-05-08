DynaShape is an open-sourced plugin for Dynamo the can be used for constraint-based form finding.

The source code is written entirely in Visual C#. All the dependent dll files can be found in the the Dynamo Core main folder (e.g. C:\Program Files\Dynamo\Dynamo Core\1.x), except mathnet.numerics.dll (which can be downloaded from NuGet, recommeded version 3.11 or above)

After you have successfully built DynaShape.dll from the source. You can assemble the Dynamo package folder according to the following structure:

DynaShapePackage
--pkg.json (provided with the Visual Studio solution)
--bin
----DynaShape.dll
----DynaShape_DynamoCustomization.xml (provided with the Visual Studio  solution)
----MathNet.Numerics.dll

For the mouse interaction to work, for now you will need to manually edit the AssemblyPath inside the DynaShape_ViewExtensionDefinition.xml (provided with the Visual Studio  solution) so that it points correctly to the DynaShape.dll within the package folder, and then place this xml into to [DynamoCoreMainFolder]\viewExtensions

If you have questions, feel free to contact me at longnguyen.gigabidea@gmail.com or on Twitter (@LongNguyenP)

Acknowledgements:
I would like to acknowledge the following people:
- The Computer Graphics Lab and EPFL for developing the essential mathematical framework, which DynaShape's core algorithms are largely based on. http://lgg.epfl.ch/publications/2012/shapeup/index.php
- Daniel Piker, for playing a major role in popularizing physics and constraint-based digital form finding within the design community, mostly via his famous plugin KangarooPhysics for Grasshopper. The success of KangarooPhysics inspired me to start DynaShape in hope to make similar computational design tools available to the Dynamo and BIM community.
http://www.grasshopper3d.com/group/kangaroo
- Autodesk (particularly Phil Mueller and Matt Jezyk) for co-organizing and co-sponsring AEC Hackathon Munich 2017, where DynaShape was born.
- My parents Thien, Van, and my younger sister Thu. They came from architecture backgrounds and have been significantly influencing me to bring my computer science background to the architecture field.

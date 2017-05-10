MIT License, Copyright 2017 Long Nguyen

DynaShape is a WIP open-source Dynamo plugin constraint-based form finding and optimization, as well as physics simulation.

DynaShape source code is written entirely in Visual C#. All the dependent dll files can be found in the the Dynamo Core main folder (e.g. C:\Program Files\Dynamo\Dynamo Core\1.x), except mathnet.numerics.dll (which can be downloaded from NuGet, recommeded version 3.11 or above)

After you have successfully built DynaShape.dll from the source, you can assemble the Dynamo package folder according to the following structure:


DynaShapem

-- pkg.json (provided with the Visual Studio solution)

-- bin

---- DynaShape.dll
 
---- DynaShape_DynamoCustomization.xml (provided with the Visual Studio  solution)
 
---- MathNet.Numerics.dll


Once the package folder has been assembled, you can "install" the package to Dynamo by to "Dynamo Main Menu > Settings > Manage Node and Package Paths...", and add a path to the package folder.

For the mouse interaction to work, for now you will need to manually edit the AssemblyPath inside the DynaShape_ViewExtensionDefinition.xml (provided with the Visual Studio  solution) so that it points correctly to the DynaShape.dll within the package folder, and then place this xml into [DynamoCoreMainFolder]\viewExtensions, and restart Dynamo Studio and/or Revit


Once the package has been installed. You can start to create goals in Dynamo and plug them to the Solver.Execute node. The solver needs to be reset once at the beginning of every scenerio, then set the "reset" to false and enable periodic update mode and see the solver iteratively move the nodes/vertices to their desired positions.

To manipulate the node/vertices directly in the background view, make sure that DynaShape_ViewExtensionDefinition.xml has been set up correctly as explained above, and set the mouseInteract input to the Solver.Execute node to True, and then you can switch to background view navigation and start manipulating the nodes. Enjoy! 

==========================================================

Contact Info:
- Email: longnguyen.gigabidea@gmail.com
- Twitter: @LongNguyenP

==========================================================

Acknowledgements:

I would like to acknowledge the following people:
- Ian Kenough and the Dynamo development team for the great visual programming tool.
- The EPFL Computer Graphics Lab for developing the essential mathematical and algorithm framework, which DynaShape is based on. http://lgg.epfl.ch/publications/2012/shapeup/index.php
- Daniel Piker, for playing quite a major role in popularizing physics-based and constraint-based digital form finding within the design community, mostly via his famous plugin KangarooPhysics for Grasshopper. The popularity of KangarooPhysics inspired me to start DynaShape in hope to make similar computational design tools available to the Dynamo and BIM community.
http://www.grasshopper3d.com/group/kangaroo.
- Autodesk (particularly Phil Mueller and Matt Jezyk) for co-organizing and co-sponsoring AEC Hackathon Munich 2017, where DynaShape was originally born.
- My parents Thien, Van, and my younger sister Thu. They came from architecture backgrounds and have been significantly influencing me to bring my computer science background to the architecture field.
- My colleauges and professor at ICD Stuttgart for giving me further exposure to the field of computational design and fabrication. http://icd.uni-stuttgart.de/
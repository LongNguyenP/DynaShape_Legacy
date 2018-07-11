<img src="https://forum.dynamobim.com/uploads/dynamobim/original/3X/7/4/74bf86e06c2827782a60f4fa54ce1dbd8136fc2a.png" width = "900">

###### If you want use DynaShape directly, visit the [DynaShape thread](https://forum.dynamobim.com/t/dynashape/11666) on the DynamoBIM forum for how-to-install and other information.

###### The information below is only for those who are interseted in working with DynaShape's source code

## How to build, package and install

#### Step 1: Clone source
Clone the source from GitHub and open in Visual Studio

#### Step 2: Deal with reference files
After open the source in Visual Studio, you may notice that some reference dll files are missing. Most of these can be found in the Dynamo Core main folder on your computer (e.g. *C:\Program Files\Dynamo\Dynamo Core\1.x*), except *MeshToolkit.dll*. To get this file you need to first install the MeshToolkit package using the Dynamo Package Manager, you can then find the *MeshToolkit.dll* typically at *%AppData%\Dynamo\Dynamo Core\x.x\packages\MeshToolkit\bin*

#### Step 3: Build and package
Build the solution and you will get *DynaShape.dll* (along with *DynaShape.xml*) in the build output folder.

You can now assemble the DynaShape package folder according to the following structure:

<pre>
_DynaShape
├── pkg.json (provided with the Visual Studio solution)
└── bin
    ├── DynaShape.dll
    ├── DynaShape.xml
    └── DynaShape_DynamoCustomization.xml (provided with the Visual Studio solution)
</pre>

***Very Important:*** Notice that there is an underscore character in the *"_DynaShape"* folder name as shown above. This is to ensure that Dynamo will load the DynaShape package alphabetically AFTER the MeshToolkit package. Otherwise, DynaShape will NOT be loaded correctly.

#### Step 4a: Install the package into Dynamo
Once the DynaShape package folder has been assembled, you can "install" the package to Dynamo by going to *Dynamo-Main-Menu > Settings > Manage Node and Package Paths...*, and add a path to the package folder.

#### Step 4b: Extra one-time setup for mouse manipulation to work
You will need to manually edit the *AssemblyPath* inside the *DynaShape_ViewExtensionDefinition.xml* file (provided with the Visual Studio solution) so that it points correctly to the *DynaShape.dll* file inside the package folder, and then place this xml into *[DynamoCoreMainFolder]\viewExtensions*, and restart Dynamo.

Once the package has been installed, you can start playing with these [Dynamo sample scripts](https://drive.google.com/drive/folders/0B8GXDbjowDN_ZHZ0ZWZaSWIwMzA?usp=sharing) to see how DynaShape works.



## Contact Info
* Email: longnguyen.connect@gmail.com
* Twitter: [@LongNguyenP](https://twitter.com/LongNguyenP)
* LinkedIn: https://www.linkedin.com/in/longnguyenp/


## Acknowledgement
I would like to acknowledge the following people:
* [Ian Keough](https://twitter.com/ikeough) and the [Dynamo](http://dynamobim.org/) development team, for the great visual programming tool.
* The [EPFL Computer Graphics Lab and Geometry Lab](http://lgg.epfl.ch/index.php), for developing the important [theoratical framework](http://lgg.epfl.ch/publications/2012/shapeup/paper.pdf), which DynaShape is based on. 
* [Daniel Piker](https://twitter.com/KangarooPhysics), for playing a major role in popularizing physics and constraint-based digital form finding in the design community, mostly via his well-known plugin [KangarooPhysics](http://www.grasshopper3d.com/group/kangaroo.) for Grasshopper. The highly positive response that KangarooPhysics receives from the community has inspired me to start DynaShape in hope to make similar computational design tools available to the Dynamo and BIM community.
* Autodesk (particularly Phil Mueller and Matt Jezyk) for co-organizing and co-sponsoring AEC Hackathon Munich 2017, where DynaShape was born.
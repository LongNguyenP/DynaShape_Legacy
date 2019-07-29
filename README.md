<img src="https://forum.dynamobim.com/uploads/dynamobim/original/3X/7/4/74bf86e06c2827782a60f4fa54ce1dbd8136fc2a.png" width = "900">

###

##### The information below is mainly intended for those who are interested in working with DynaShape's source code.

##### If you want to use DynaShape in Dynamo directly, visit the [DynaShape Thread on Dynamo forum](https://rebrand.ly/ds0) for how-to-install and other information.

## How to build, package and install

#### Step 1: Clone source
Clone the source from GitHub and open in Visual Studio

#### Step 2: Deal with reference files
After open the source in Visual Studio, Most of these will be automatically restored by the NuGet Package Manager when you first build the codes. However the two following dlls will have to be located manualy on your computer

* *MeshToolkit.dll*: To get this file you need to first install the MeshToolkit package using the Dynamo Package Manager, you can then find the *MeshToolkit.dll* typically at *%AppData%\Dynamo\Dynamo Core\2.x\packages\MeshToolkit\bin*
* *HelixToolkit.Wpf.SharpDX.dll*: This can be found in the main folder of *DynamoCoreRunTime* or *Dynamo Core* (typically located at *Program Files/Dynamo/Dynamo Core/2.x*)
#### Step 3: Build and package
Build the solution and you will get *DynaShape.dll* (along with *DynaShape.xml*) in the build output folder.

You can now assemble the DynaShape package folder according to the following structure:



<pre>
(The files marked with * are included in the visual studio solution, under the "ManifestFiles" folder)

_DynaShape
├── pkg.json *
└── bin
    ├── DynaShape.dll
    ├── DynaShape.xml
    └── DynaShape_DynamoCustomization.xml *
└── extra
    └── DynaShape_ViewExtensionDefinition.xml *
</pre>


***Very Important:*** Notice that there is an underscore character in the *"_DynaShape"* folder name as shown above. This is to ensure that Dynamo will load the DynaShape package alphabetically AFTER the MeshToolkit package, which is a MUST.

#### Step 4: Install the package into Dynamo
Once the DynaShape package folder has been assembled, you can "install" the package to Dynamo by to *Dynamo-Main-Menu > Settings > Manage Node and Package Paths...*, and add a path to the package folder.

Once the package has been installed, you can start playing with these [Dynamo sample scripts](https://drive.google.com/drive/folders/0B8GXDbjowDN_ZHZ0ZWZaSWIwMzA?usp=sharing) to see how DynaShape works.



## Contact Info
* Email: longnguyen.connect@gmail.com
* Twitter: [@LongNguyenP](https://twitter.com/LongNguyenP?lang=en)
* LinkedIn: https://www.linkedin.com/in/longnguyenp/


## Acknowledgement
I would like to acknowledge the following people:
* [Ian Keough](https://twitter.com/ikeough?lang=en) and the [Dynamo](http://dynamobim.org/) development team, for the great visual programming tool.
* The [EPFL Computer Graphics Lab and Geometry Lab](http://lgg.epfl.ch/index.php), for developing the important [theoratical framework](http://lgg.epfl.ch/publications/2012/shapeup/paper.pdf), which DynaShape is based on. 
* [Daniel Piker](https://twitter.com/KangarooPhysics?lang=en), for playing a major role in popularizing physics and constraint-based digital form finding in the design community, mostly via his well-known plugin [KangarooPhysics](http://www.grasshopper3d.com/group/kangaroo.) for Grasshopper. The highly positive response that KangarooPhysics receives from the community has inspired me to start DynaShape in hope to make similar computational design tools available to the Dynamo and BIM community.
* Autodesk (particularly Phil Mueller and Matt Jezyk) for co-organizing and co-sponsoring AEC Hackathon Munich 2017, where DynaShape was born.
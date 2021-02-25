<img src="https://aws1.discourse-cdn.com/business6/uploads/dynamobim/original/3X/e/4/e48d8015a42758dcbe5a8197a3d199060701b3c7.png" width = "900">

###

##### The information below is mainly intended for those who are interested in working with DynaShape's source code.

##### If you want to use DynaShape in Dynamo directly, visit the [DynaShape Thread on Dynamo forum](https://rebrand.ly/ds0) for how-to-install and other information.

## How to build, package and install

#### Step 1: Clone source
Clone the source from GitHub and open in Visual Studio

#### Step 2: Deal with missing references
After open the source in Visual Studio, You will notice that many of the referenced dll files seem to be missing. However, the NuGet package manager will automatically restore these files the first time you build the codes. So you don't have to worry about them. The only exception is MeshToolkit. To get this file you need to first install the MeshToolkit package using the Dynamo Package Manager, you can then find the *MeshToolkit.dll* typically at *%AppData%\Dynamo\Dynamo Core\2.x\packages\MeshToolkit\bin*

#### Step 3: Build and package
Build the solution and you will get *DynaShape.dll* (along with *DynaShape.xml*) in the build output folder.

You can now assemble the DynaShape package folder according to the following structure:



<pre>
(The files marked with * are included in the visual studio solution, under the "ManifestFiles" folder)

DynaShape
├── pkg.json *
└── bin
    ├── DynaShape.dll
    ├── DynaShape.xml
    └── DynaShape_DynamoCustomization.xml *
└── extra
    └── DynaShape_ViewExtensionDefinition.xml *
</pre>


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
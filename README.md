# Unity Pixelated Digging

A pixelated digging system with textures and runtime mesh generation in Unity.

This system utilizes the basic idea behind <strong>marching squares</strong> algorithm with slight modifications.

![demo](https://github.com/user-attachments/assets/d8a75ea0-47d3-4275-9ac9-5efa623179a5)

You can clone or download the project directly, or download the Unity package file from the releases section and import it into your project.

## How to Use

After adding the package into your Unity project, find the <strong>VoxelGrid prefab</strong> in the path <strong>Assets->PixelatedDigging->Prefabs->VoxelGrid</strong> and drag it into your scene.

![prefab](https://github.com/user-attachments/assets/51bde4fd-d8d8-41d1-9fdc-e39b47829b4b)

You may now see the gizmos of the <strong>VoxelGrid object</strong> in the scene view. It shows how the grid will look like once it gets initialized in play mode.

![gizmos](https://github.com/user-attachments/assets/5469fb01-380b-4337-87e0-6ca096d52995)

## Chunks

Once initialized, <em>Voxel Grid</em> consists of <strong>Chunks</strong>. This way, whenever digging occurs, only the relevant <em>chunks</em> update their meshes.\
Each <em>chunk</em> comes with 2 meshes, one for the surface and one for the extrusion parts.

##

Select the <em>Voxel Grid</em> object from the hierarchy to reveal the <strong>VoxelGrid</strong> component in the inspector.
Here you can see some fields of the <em>VoxelGrid</em> that you can change as you would like. Notice how the gizmos change as you tweak these values.

![inspector](https://github.com/user-attachments/assets/81d17b9f-d128-437a-b870-faa111aa8e8c)

- <strong>'Voxel Size'</strong> defines the size of each <strong>voxel(pixel)</strong> on both X and Y axis in units.
- <strong>'Extrusion Height'</strong> defines the size of each <em>voxel</em> on the Z axis in units.
- <strong>'Grid Resolution'</strong> defines the number of <em>chunks</em> on the grid for each axis.
- <strong>'Chunk Resolution'</strong> defines the number of <em>voxels</em> on each <em>chunk</em> for each axis.

In this example the grid will consist of 4 <em>chunks</em>(2x2), and will have 100 <em>chunks</em> on both X and Y axis(2x50). Each side of the grid will be 10 units long(100x0.1).

## About the Algorithm

As previously mentioned, this digging system is inpired by <em>marching squares</em> algorithm but has some fundamental differences.

In traditional <em>marching squares</em>, control nodes are placed in the corners of cells, whereas in this system, control nodes are the <em>voxels</em> themselves and they define a cell at the center of which they are located.

## References
- https://catlikecoding.com/unity/tutorials/marching-squares-series
- https://github.com/SebLague/Procedural-Cave-Generation

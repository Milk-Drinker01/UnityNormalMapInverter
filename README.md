# UnityNormalMapInverter
Convert normal maps from DirectX (-y) to OpenGL (+y)


This is a simple tool for converting normal maps from DirectX style (-y) to OpenGL (+y). why are there 2 standards that contain the same information but have slightly different configuration? beats me. But ive seen too many people use the wrong type in the wrong engine, so I made this in-editor tool to fix it, which is much easier to use than opening gimp, decomposing the colors into the RGB components, inverting the G channel, recomposing, and overwriting. 

A render engine will use either DirectX or OpenGL style normals. Some common examples include:
 OpenGL:
  - Unity
  - Blender
  - Houdini
  - Maya
  - Zbrush
  - IClone
  DirectX:
  - UE4/5
  - Godot
  - CryEngine
  - Source Engine
  - Substance Designer/painter

How do I know if my normal maps are fucked?
cause they look like this:
![FuckedOrNot](https://user-images.githubusercontent.com/59656122/162627338-a93b8efc-a28a-4a94-907a-1ec95cbeb385.png)



HOW TO USE:

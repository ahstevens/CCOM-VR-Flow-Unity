# CCOM Flow Field VR

Visualize and interact with 3D and 4D flow fields in virtual reality.

## Quickstart

- Open example scene in Assets/Scenes/VR MMS.unity
- Hit "Play" button and explore

## Usage

### Add Vector Field Data
  - Place binary file into Assets/Resources folder and change extension to .bytes
  - Add loading function to Assets/CCOM/Editor/FlowFileEditorWindow.cs (follow bgrid or egrid example within)
  - Unity menu CCOM -> Load Flow File (pops up new window)
  - Create new GameObject in scene and make sure it is selected
  - Click button corresponding to the data you want to load
  - Selected GameObject gets nested child GameObjects "Alignment Layer" and "Coordinate Frame" to which the "Vector Field" GameObject is added
  - Disable the Box Collider component on the "Vector Field" GameObject to be able to reach into data volume to grab slices within

### Add Slice
  - Select desired "Vector Field" GameObject to add a slice to
  - Unity menu CCOM -> Hairy Slices
  - Click "Add Hairy Slice to Selection" button on window that pops up
    - "Optional Settings" can be used when in play mode to jitter the regular grid and snap a screenshot with the in-game camera
  - New "Hairy Slice" GameObject gets added as sibling to "Vector Field" GameObject
  - May want to disable "Mesh Renderer" component on new "Hairy Slice" GameObject so slice surface is not rendered
  - "Hairy Slice" script component on new "Hairy Slice" GameObject needs parameter "Main Mat" set to "Stripes"
  - "Flow File" parameter can be left alone; script automatically finds sibling flow file
  - Other parameters control the flow glyph ("Hairs") properties. The glyphs/hairs will only be visible while in Play Mode. Some parameter changes require a manual rebuild of the glyph objects; click the "Rebuild" checkbox to request the script regenerate the geometry.

### Controls

  - You can use the touchpach on your VR controller to teleport around the scene.
  - Use the trigger to grab a slice and move it around within the Vector Field volume.
  - **Esc**: Quit the application
  - **L**: Create a point light at the tip of the right hand model's index finger. These point lights can be grabbed and repositioned as desired
  - **K**: Remove point light closest to the tip of the right hand model's index finger
  - **C**: Bring the camera to your right hand. The camera can be grabbed and posed as desired
  - **F7**: Isolate flow field visualization layer in camera view
  - **F8**: Change isolated visualization layer background to white
  - **F8**: Change isolated visualization layer background to black
  - **F10**: Enable all scene layers in camera view
  - **F11**: Capture a screenshot (will be stored in base project folder as "Screenshot_#" in PNG and JPEG)

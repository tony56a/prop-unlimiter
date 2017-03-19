# PropUnlimiter #

## Type Manager.PropContainer

 A container for Unlimited props, wraps around the actual prop instance itself, and a dict of string to floats for extra values ( more precise position, etc ) 



---
#### Field Manager.PropContainer.propInstance

 The prop instance itself 



---
#### Field Manager.PropContainer.extras

 The extras dictionary, holds values used by other mods 



---
#### Property Manager.PropUnlimiterManager.Props

 Dictionary of a "grid" of values on the map that correspond to the props in that grid 



---
#### Property Manager.PropUnlimiterManager.ExtraMapping

 Dictionary of PropContainer references/hashcodes to the extra mapping for the PropContainer 



---
#### Method Manager.PropUnlimiterManager.SetUnlimitedProp(System.Int32,UnityEngine.Vector3,System.Single,System.Boolean)

 Place a prop in alternate prop store, based on the propInfo ID, intended for use in serialization 

|Name | Description |
|-----|------|
|infoIndex: |ID of prop|
|position: | position of prop|
|angle: | angle of prop |
|single: | some kind of instance flag???|


---
#### Method Manager.PropUnlimiterManager.SetUnlimitedProp(PropInfo,UnityEngine.Vector3,System.Single,System.Boolean)

 Place a prop in alternate prop store, Note: Will set additional values in extras dict (accx, accz) for precision placement of props 

|Name | Description |
|-----|------|
|info: |Prop info(mesh, availability flags, etc)|
|position: | position of prop|
|angle: | angle of prop |
|single: | some kind of instance flag???|


---
#### Method Manager.PropUnlimiterManager.SetUnlimitedProp(PropUnlimiter.Utils.PropWrapper)

 Place a prop in prop store with prop info. Intended for deserialization only 

|Name | Description |
|-----|------|
|wrapper: |Prop info|


---
#### Method Manager.PropUnlimiterManager.DeleteProp(System.Int32,PropUnlimiter.Manager.PropContainer)

 Delete a prop, given the prop, and the grid it belongs to 

|Name | Description |
|-----|------|
|gridKey: |The grid value the prop is in|
|container: |The container to delete|


---
#### Method Manager.PropUnlimiterManager.UpdateProp(PropUnlimiter.Manager.PropContainer,UnityEngine.Vector3,System.Single)

 Update an existing PropContainer to a new position/angle 

|Name | Description |
|-----|------|
|container: |The PropContainer to update|
|newPosition: |The new prop position|
|newAngle: |The new prop angle|


---
#### Method Manager.PropUnlimiterManager.RaycastUnlimitedProps(UnityEngine.Ray,System.Int32@,PropUnlimiter.Manager.PropContainer@)

 Raycast for props, based on original propmanager raycast 

|Name | Description |
|-----|------|
|input: |Input ray for search|
|gridKey: |grid position the prop is in|
|prop: |the prop itself|
**Returns**: whether or not a prop was found



---
#### Method Manager.PropUnlimiterManager.GetPropsInGrid(System.Int32)

 Gets all props in the grid specified by gridKey, or null if nothing there 

|Name | Description |
|-----|------|
|gridKey: |The grid to get props from|
**Returns**: A list of props in the grid, or null if none available



---
#### Method Manager.PropUnlimiterManager.GetAllProps

 Gets all Prop Unlimiter props 

**Returns**: A list of all Prop Unlimiter props



---
#### Method Manager.PropUnlimiterManager.GetAllPropWrappers

 Get all prop infos. Intended for serialization only 

**Returns**: Array of all prop infos for serialization



---
#### Method Manager.PropUnlimiterManager.GetListPropWrappers(System.Int32)

 Get all prop info for a grid. Intended for serialization only 

|Name | Description |
|-----|------|
|gridKey: |The grid to get prop infos from|
**Returns**: A list of props in the specified grid, empty list if grid is not present



---
#### Method Manager.PropUnlimiterManager.LoadWrappers(System.Collections.Generic.List{PropUnlimiter.Utils.PropWrapper})

 Populate Prop Unlimiter store, given a list of prop infos 

|Name | Description |
|-----|------|
|propInfos: |A list of prop infos to load|


---
#### Method Patches.PropRenderingPatch.RenderProp(RenderManager.CameraInfo,PropUnlimiter.Manager.PropContainer)

 Intended for internal accurate rendering only. 

|Name | Description |
|-----|------|
|cameraInfo: ||
|container: ||


---



# eppz! `Utils`
> part of [**Unity.Library.eppz**](https://github.com/eppz/Unity.Library.eppz)

* [`ActivateOnAwake.cs`](ActivateOnAwake.cs)

	+ Sets each given `GameObject` active on awake.

* [`ExecutionOrder/ExecutionOrder.cs`](ExecutionOrder/ExecutionOrder.cs)

	+ Adds attributes like `[ExecutionOrder(500)]` to have your script execution order managed explicitly. `Editor/ExecutionOrders` is process every class when it is loaded.

* [`Editor/ClipSurgery.cs`](Editor/CodeStats.cs)

	+ Animation tool to fix broken paths, remap animation targets / properties, copy keyframe tangents, or even copy entire animation curves (work-in). Goes to `Window/eppz!/Clip Surgery`.

* [`Editor/CodeStats.cs`](Editor/CodeStats.cs)

	+ Basic code statistics (line number / statement count distribution). Goes to `Window/eppz!/Code Stats`.

* [`Editor/HandTool.cs`](Editor/HandTool.cs)

	+ Hold spacebar for hand (drag) tool. It is also published as [Hold Spacebar for Hand (drag) Tool on the **Unity Asset Store**](https://www.assetstore.unity3d.com/en/#!content/32803). All ⭐⭐⭐⭐🌟, I'm so proud.

* [`Editor/ScriptSurgery.cs`](Editor/ScriptSurgery.cs)

	+ Helper tool to fix missing script references. Goes to `Window/eppz!/Script Surgery`.

* [`Editor/SelectByLayer.cs`](Editor/SelectByLayer.cs)

	+ As title goes, script to select each `GameObject` on a given *Layer*. Goes to `Window/eppz!/Select by Layer`.

* [`Editor/SliceRenamer.cs`](Editor/SliceRenamer.cs)

	+ Convenient tool to rename slices in your *Sprite Sheet* assets. Goes to `Window/eppz!/Select by Layer`.
	+ Also contains one of my favourite invention: a very interesting class `EmbeddedImage`, that allows you to store `Texture2D` images right in the C# source code (encoded to Base64 string), and access / decode it at runtime. Really convenient to package *Editor Tool* textures into a single file.

* [`Editor/Snap.cs`](Editor/Snap.cs)

	+ Some helper to snap your `GameObject` transform positions in *Editor*. Menu items goes to `eppz!/Snap` (see keyboard shortcuts beside menu items).

## License

> Licensed under the [MIT license](http://en.wikipedia.org/wiki/MIT_License).


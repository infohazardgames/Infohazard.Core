# Infohazard.Core Documentation

## Table of Contents

- [Infohazard.Core Documentation](#infohazardcore-documentation)
  - [Table of Contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Documentation](#documentation)
  - [License](#license)
  - [Installation](#installation)
    - [Method 1 - Package Manager](#method-1---package-manager)
    - [Method 2 - Git Submodule](#method-2---git-submodule)
    - [Method 3 - Add To Assets](#method-3---add-to-assets)
    - [Method 4 - Asset Store](#method-4---asset-store)
  - [Setup](#setup)
    - [General Setup](#general-setup)
    - [SRP Setup](#srp-setup)
  - [Features Guide](#features-guide)
    - [Attributes](#attributes)
    - [Data Structures](#data-structures)
    - [Pooling](#pooling)
    - [Timing](#timing)
    - [Tags](#tags)
    - [Unique Names](#unique-names)
    - [Utility](#utility)
    - [Miscellaneous](#miscellaneous)

## Introduction

Infohazard.Core is a collection of systems and utilities that I’ve found super helpful in making many different kinds of games, so I hope you find it helpful too! This document will cover setup and basic usage of the code in Infohazard.Core.
You can find full API documentation here and in the code.

## Documentation

[API Docs](https://www.infohazardgames.com/docs/Infohazard.Core/html/)

[Tutorial Playlist](https://www.youtube.com/playlist?list=PLpnNr8QNHD90TDqamYqA95ENh4eIQVvhS)

## License

If Infohazard.Core is acquired from the Unity Asset Store, you must follow the Unity Asset Store license.
The open-source repository uses the [MIT license](https://opensource.org/licenses/MIT).
You are welcome to have your own packages or assets depend on this package.

## Installation

### Method 1 - Package Manager

Using the Package Manager is the easiest way to install HyperNav to your project. Simply install the project as a git URL. Note that if you go this route, you will not be able to make any edits to the package.

1. In Unity, open the Package Manager (Window > Package Manager).
2. Click the '+' button in the top right of the window.
3. Click "Add package from git URL...".
4. Paste in `https://github.com/infohazardgames/Infohazard.Core.git`.
5. Click Add.

### Method 2 - Git Submodule

Using a git submodule is an option if you are using git for your project source control. This method will enable you to make changes to the package, but those changes will need to be tracked in a separate git repository.

1. Close the Unity Editor.
2. Using your preferred git client or the command line, add `https://github.com/infohazardgames/Infohazard.Core.git` as a submodule in your project's Packages folder.
3. Re-open the Unity Editor.

If you wish to make changes when you use this method, you'll need to fork the HyperNav repo. Once you've made your changes, you can submit a pull request to get those changes merged back to this repository if you wish.

1. Fork this repository. Open your newly created fork, and copy the git URL.
2. In your project's Packages folder, open the HyperNav repository.
3. Change the `origin` remote to the copied URL.
4. Make your changes, commit, and push.
5. (Optional) Open your fork again, and create a pull request.

### Method 3 - Add To Assets

If you wish to make changes to the library without dealing with a git submodule (or you aren't using git), you can simply copy the files into your project's Assets folder.

1. In the main page for this repo, click on Code > Download Zip.
2. Extract the zip on your computer.
3. Make a HyperNav folder under your project's Assets folder.
4. Copy the `Editor` and `Runtime` folders from the extracted zip to the newly created HyperNav folder.

### Method 4 - Asset Store

If you’d rather use the asset store than the package manager, you can get the project at LINK TBD.
Simply add it to the project as you would any other asset.

## Setup

### General Setup

The only setup required beyond installation is to add references to the Infohazard.Core assembly if you are using an assembly definition. If you are using the default assemblies (such as Assembly-CSharp), nothing is needed here. You may also wish to have your editor assembly (if you have one) reference Infohazard.Core.Editor. In order to reference the generated GameTag file, you must also add a reference to Infohazard.Core.Data.

### SRP Setup

If you are using a scriptable render pipeline (URP, HDRP, etc) and wish to run the demos, you will need to upgrade the materials using your render pipeline's material upgrade system. The materials you'll need to upgrade are in `Assets/Plugins/Infohazard/Demos/Infohazard.Core/Materials` and `Assets/Plugins/Infohazard/Demos/Shared Demo Assets/Materials`.

## Features Guide

### Attributes

The package provides several PropertyAttributes that you can use in your scripts to customize how serialized fields are drawn in the inspector. To use any of these attributes, simply add [AttributeName] in front of a serialized field in a script. You can also check out the drawers for these attributes in the Editor/Attributes directory.

#### AssetDropdown

The [AssetDropdown] attribute is used to show a dropdown menu on a field whose type is a UnityEngine.Object reference. It will find all assets in your project that match this type and display them as options in the dropdown. The standard drag/drop interface still works as well.

#### ConditionalDraw

The [ConditionalDraw] attribute is used to conditionally hide a serialized field in the inspector depending on some other condition. The supplied condition should be the name of another serialized field in the same script. You can also optionally pass in a value to compare that condition field with, and whether they must be equal or unequal to display the field.
#### DrawSingleChildProperty
The [DrawSingleChildProperty] attribute is used to render a hierarchy of fields as just a single value. Say you have a struct called Data with a string field called _name. Adding [DrawSingleChildProperty(“_name”)] to a field of type Data would cause just the _name field to be drawn in the inspector.
#### EditNameOnlyAttribute
The [EditNameOnly] attribute displays a Unity Object reference field as a text entry, which is used to control the name of the referenced object. If the reference is set to null, the standard drag-drop box is used.
#### ExpandableAttribute
The [Expandable] attribute is used to optionally draw the child properties of a Unity Object reference field, such that the referenced object can be edited without changing the inspector context. If the type of the field is a ScriptableObject, new instances can also be created from the inspector.
#### TypeSelectAttribute
The [TypeSelect] attribute is used on a string field to show a dropdown where any valid C# type can be selected. The selected type is saved in the string as its full class name. This attribute is useful with the TypeUtility class to find the selected type.
### Data Structures
#### ListQueue
The ListQueue<T> class is an implementation of a queue data structure similar to C#’s Queue class. The main difference is that a ListQueue implements the IList interface and allows you to access any element of the queue whenever you want, while still maintaining O(1) performance for normal Enqueue and Dequeue operations (as long as there is capacity available).
### Pooling
The library provides a simple object pooling system under the Pooling directory. Object pooling means that instead of instantiating and destroying GameObjects as needed, we deactivate them and reactivate them to avoid constantly allocating and deallocating memory.
#### Spawnable
To start working with the pooling system, simply add the Spawnable script to your prefabs and then instantiate/destroy them using Spawnable.Spawn and Spawnable.Despawn. Note that Awake/OnDestroy will only be called when the objects are actually created and destroyed; if you want an event when the object is spawned, use the OnSpawned and OnDespawned messages. Note that if a prefab does not contain a Spawnable script, using Spawnable.Spawn and Spawnable.Despawn is the same as Instantiate and Destroy. This allows you to spawn and despawn objects without worrying about whether they're pooled or not. All of my libraries use this system, so they are compatible with pooling.
#### PoolManager
The pooling system needs a PoolManager to work. You can either place one PoolManager per scene, have a global instance that is never destroyed, or simply let the system create the manager itself. To create it manually, just add this script to an empty GameObject and you’re good to go. You can use the ClearInactiveObjects() method on PoolManager.Instance to destroy any inactive pooled objects, such as when you change scenes.
PooledTrail, PooledParticleEffect
Attach these scripts to TrailRenderer and ParticleSystem GameObjects to make them play nicely with the pooling system.
### Timing
The Timing directory contains some useful utilities to deal with in-game time.
#### PassiveTimer
PassiveTimer is a serializable data type used to create timers in your scripts. You can use it to create ability cooldowns and durations, weapon reloads, and other common game functionality. Simply call Initialize() when your script initializes, then use the timer's various methods. See the API docs for more info.
#### Pause
The Pause system is built to do exactly that - pause the game. Just set Pause.IsPaused to true and time will freeze, then set it to false to resume time at its previous speed. The game will automatically unpause if you change scenes. You should avoid running game logic if the game is paused.
#### TimeToLive
This script destroys or despawns an object some number of seconds after it is spawned. It is compatible with the pooling system.
### Tags
This system is meant to make working with GameObject tags in your scripts much easier and less error-prone.
#### Tag
This class provides string constants for all default tags. So instead of writing target.CompareTag(“Player”) you could write target.CompareTag(Tag.Player).
#### TagMask
Using a serialized field of type TagMask allows users to pick tags in the editor from a dropdown instead of typing them, just like the Unity-provided LayerMask.. It also allows selecting multiple tags without using an array. Extension methods are provided for common tag operations so that a TagMask can be used in place of a string tag. For example, you can say target.CompareTag(tagMask), which will return true if target’s tag is equal to any of the tags in tagMask.
#### GameTag
This file is generated based on your custom tags, and lives in your project rather than in the package. You should be prompted to generate it if the system detects you have custom tags, or you can use the Infohazard/Generate/Update GameTag.cs command. Once this file is generated, your game tags will automatically be available for selection in a TagMask, and you can refer to them as constants in code through the generated GameTag class (you will need to reference the Infohazard.Core.Data assembly if you are using assembly definitions).
### Unique Names
The UniqueName system enables you to assign names to objects that can be referenced across scenes and assets. You can then easily find the active object using that name (if one exists). This system uses ScriptableObjects to store the names so that you can easily see what names are available to reference, and can avoid having to type out the names and potentially make mistakes. Furthermore, these unique names can be changed without breaking references, since they are stored as object references.
#### UniqueNameList
A UniqueNameList asset is how you start creating a list of unique names. You can have multiple UniqueNameLists in your project, or you can use just one. This is purely for organizational purposes. You can create a UniqueNameList using Assets/Create/Infohazard/Unique Name List.
#### UniqueNameListEntry
A UniqueNameListEntry is the actual unique name, which is organized under a UniqueNameList and used both by objects with unique names and objects using the system to find named objects. UniqueNameListEntries should be created through the UniqueNameList inspector.
#### UniqueNamedObject
Attach this script to a GameObject to assign a unique name to it, and make it findable in the system. You can find one of these objects using the static method UniqueNamedObject.TryGetObject, passing in either a string or a UniqueNameListEntry.
### Utility
The Utility section contains a bunch of static methods to help with all kinds of common operations. See the API docs for each file for more info.
#### DebugUtility
Contains methods to draw a cube using Debug.DrawLine, and to pause the editor after a certain number of frames.
#### EnumerableUtility
Contains methods that combine common LINQ calls such as Select and Where into a single enumeration for better code optimization.
#### GameObjectUtility
Contains various methods for working with GameObjects and Transforms, such as destroying all the children of an object, setting an object’s layer recursively, and getting a path containing an object’s ancestor names.
#### MathUtility
Contains many useful math operations, such as constructing a Quaternion from any two axes, getting a vector with one component changed, solving polynomials up to degree 4 (quartic), and getting the point where two lines are closest to each other.
#### RandomUtility
Contains extension methods to System.Random such as generating 64-bit numbers.
StringUtility
String processing methods such as splitting a CamelCase string to have spaces between words.
#### TypeUtility
Provides methods to get a list of all loaded types using reflection, and to find a type based on its name.
### Miscellaneous
The remaining functionality provided by Infohazard.Core doesn’t fall nicely into one of the previous categories, but was still useful enough to include.
#### ProgressBar
Used to create health bars and other types of progress bars without using a Slider. It supports images that fill the bar using either the “filled” image type or by manipulating the RectTransform anchors.
#### SceneControl
Provides a static method to quit the game that works in a standalone build as well as in the editor. Also provides some methods to navigate to scenes. This is useful if you’re building a super quick main menu (such as in the last half hour of a game jam) and need to hook up your buttons as fast as possible.
#### SceneRef
A serializable type that allows you to have assignable scene references in your scripts without making the user type the scene name. Instead, they can simply drag in a scene asset. At runtime, you still access the scene by its name. Using a SceneRef also enables the reference to be maintained if a scene is renamed.
#### Singleton
You can inherit from this script in managers or other scripts that need to exist in the scene exactly once. A static Instance accessor is automatically provided, which will do a lazy search for the correct instance the first time it is used, or if the previous instance was destroyed. After that it will just return a cached instance.
#### SingletonAsset
Similar to Singleton, but for ScriptableObjects. You specify a path in your subclass where the instance should live (this must be under a Resources folder) and the editor will automatically handle loading and even creating this asset for you when needed.
#### TriggerVolume
A script that makes it easy to add events to a trigger collider. Provides both UnityEvents (assignable in the inspector) and normal C# events for when an object enters or leaves the trigger, and when all objects have left the trigger.

# Universal Unity Runtime Localization Mod

A lightweight, runtime localization tool for Unity games (Mono-based). This mod allows you to dynamically translate TextMeshPro (TMP) and legacy UGUI text at runtime without modifying the game's original assets, scenes, or prefabs. 

Originally developed to provide a Russian translation for the game *Family Time (Chapter 3)*, this tool is designed to be **game-agnostic** and **language-agnostic**. It can be injected into almost any Mono Unity game to provide localization for any language by simply swapping the dictionary file.

## Prerequisites
- [dnSpy](https://github.com/dnSpyEx/dnSpy) (or a similar .NET assembly editor).
- The target game must use the Unity **Mono** scripting backend (IL2CPP is not supported).

## Installation & Usage

1. **Inject the Code**
   Open the game's `Assembly-CSharp.dll` in dnSpy. Add the `LocalizationManager` class to the assembly.

2. **Initialize the Manager**
   Find a root initialization class in the game (for example, `GameSession` in *Family Time*, or `MainMenu` in other games) and add the initialization call inside its `Awake()` method:
   ```csharp
   private void Awake()
   {
       // Original game Awake code
       LocalizationManager.Init(); 
   }

3.**Create the Dictionary**
    Create a file named russian.txt (or any name you specify in the code) inside the game's [GameName]_Data folder. Add your translations line-by-line in the format Original=Translation.

4.**Compile & Play**
    Save the modified .dll in dnSpy and launch the game.

# IRBTModUtils
A collection of utility classes and helpers that support mods for the [HBS BattleTech](http://battletechgame.com/) game.


## Mech Move Modifiers

Various systems may want to patch the movement speeds of Mechs. Overlapping Harmony patches can be messy, so IRBTModUtils provides an API that allows systems to inject their own modifications. IRBTModUtils will scan all loaded assemblies for classes that extend `IRBTModUtils.Extension.MechMoveModifier` during the ModTek 'FinishedLoading' call. These implementations will be invoked to calculate the following properties of the Mech class:

* MaxWalkDistance
* MaxBackwardDistance
* MaxSprintDistance

Other mods may want to reference these values directly. To do so, simply import `IRBTModUtils.Extension` and use the `ModifiedWalkDistance` and `ModifiedRunDistance` extensions on your target Mech. These methods will fetch the base distance (mech.WalkSpeed and mech.RunSpeed respectively) then apply any configured modifiers to them.

This mod sets a minimum move distance, expressed as `Settings.MinimumMove` in `mod.json`. If the sum of all modifiers reduces the walk or run distance below this value, it will be returned instead.

This feature patches several methods. If you'd like to disable it entirely, set `Settings.Features.EnableMovementModifers` to false in `mod.json`.

## Custom Dialog

Loreum ipsum

## Custom Influence Map Factors

The AI logic in HBS BT uses a Decision Tree approach, a series of nodes that return a success or failure to determine what actions it should take. As part of choosing where to move, the AI calculates a discrete value for each position through an Influence Map. The InfluenceMap can be supplied with various factors which analyze the game state and return a multiplier and a weight. The combination of multiplier and weight determine how strongly the AI should emphasize that position, in conjunction with all the other factors. Decompile the `InfluenceMapEvaluator` constructor to see all of the vanilla factors that get applied.

If a mod wants to add their own custom InfluenceMap they can do so by implementing one of the following abstract classes. These factors will be injected to the InfluenceMap by the mod [CleverGirl](https://github.com/battletechmodders/clevergirl/), and applied as if they were normal factors.

By default HBS factors rely upon the `BehaviorVariable` enum. Factors return the behavior var name, and the InfluenceMap pulls the value using that name. This enum was never upgraded to a data-driven enum, so we cannot customize values or supply new ones. Instead each custom factor must return the weight directly through the `GetRegularMoveWeight` and `GetSprintMoveWeight` methods.

* CustomInfluenceMapAllyFactor: These factors should apply based upon the number of friendly actors (allies) around the target. Typically you should limit yourself to 2-4 to prevent the calculation time from being excessive.
* CustomInfluenceMapHostileFactor: These factors should apply based upon the presence of enemies.
* CustomInfluenceMapPositionFactor: These factors apply based on any factor relevant to the position itself.

In practice the distinction between these three values is heavily blurred.

### Removing Vanilla Values

You may find it desirable to remove some vanilla factors. To do so, implement a subclass of the `InfluenceMapFactorsToRemove` class. Return a static list of ally, hostile, and position factors that should be removed. These will be eliminated by CleverGirl at startup.

## "RESULT" mission objectives

IRBTModUtils extends the after action report with support for RESULTS, "objectives" that the player neither succeeded nor failed at. When adding results to the After Action Report, objectives with the status `ObjectiveStatus.Ignored` will be displayed as gold RESULTs rather than green SUCCESSFUL or red FAILED objectives.

## Deferring Logger

Loreum ipsum

## Redzen Ziggurat Guassian

Loreum ipsum

TBD:
* QuadTrees for neighbor calculations - https://en.wikipedia.org/wiki/Quadtree


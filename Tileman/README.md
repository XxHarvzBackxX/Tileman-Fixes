﻿///

This is SpicyKai's Tileman Challenge!

Most of the world is restricted behind tiles,
and if you want to use them--or even get to Pierre's you must buy my tiles!

This mod was inspired by Setlled's Runescape Tileman challenge on Youtube, 
Check him out here: https://www.youtube.com/c/Settledrs/featured

///

Press 'G' to turn the tile overlay on/off

Press 'H' to switch control mode. One for buying the tile in front of you, the other to use the mouse instead

///


Here is a short explanation for the values used in the config_tiles.json (There are seperate config_tiles.json files for each save for progression purposes)


ToPlaceTiles: 
Tells the mod whether or not to put down tiles in each location. true -> place tiles, false -> do not.

DoCollision:
Does the mod check if you collide with tiles?
true -> Stop player from crossing them, 
false -> Allows free-roaming

AllowPlayerPlacement:
lets the player do things in tile spots 
true -> allows furniture to be placed in a tile !! NOT RECOMENDED USER BEWARE!!
false -> no building or furniture placing allowed

ToggleOverlay: 
true -> See the tiles,
false -> Hide the tiles

TilePrice: 
The default price of a tile. Calculations will be done with this to calculate the true price per tile.

TilePriceRaise:
How much the price of tile incrases with each purchase,
The default raise is set to something very low so tile increases happen very infrequently

CavernsExtra:
How many extra levels of Skull Caverns do you want to have tiles in? 
The default goes down to floor 100

DifficultyMode
0 -> Tile prices increase as normal,
1 -> Tile price increase by 1 every 10x tiles purchased (10,100,1000...),
2 -> Every single tile purchased increases the price by 1

PurchaseCount:
int used to track tiles bought for progression purposes

///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;


namespace Tileman
{
    public class ModEntry : Mod
    {

        private const int LadderTileIndex = 115;
        public Stack<KaiTile> FloodFillKaiTiles;
        public bool do_loop = true;
        public bool do_collision = true;
        public bool allow_player_placement = false;
        public bool toggle_overlay = true;
        private bool tool_button_pushed = false;
        private bool location_changed = false;


        public double tile_price = 1.0;
        public double tile_price_raise = 0.0008;
        public double dynamic_tile_price;


        public int caverns_extra = 0;
        public int difficulty_mode = 0;
        public int purchase_count=0;
        public int overlay_mode = 0;

        int tile_count;

        public int amountLocations = 200;
        private int locationDelay = 0;

        private int collisionTick = 0;

        List<KaiTile> tileList = new();
        List<KaiTile> ThisLocationTiles = new();
        Dictionary<string, List<KaiTile>> tileDict = new();

        Texture2D tileTexture  = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);
        Texture2D tileTexture2 = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);
        Texture2D tileTexture3 = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);

        public ModBalanceConfig modBalanceConfig = new ModBalanceConfig();


       




        public override void Entry(IModHelper helper)
        {
            modBalanceConfig = Helper.ReadConfig<ModBalanceConfig>();
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.RenderedWorld += this.DrawUpdate;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Player.Warped += Player_Warped;


            helper.Events.GameLoop.Saved += this.SaveModData;
            helper.Events.GameLoop.SaveLoaded += this.LoadModData;
            helper.Events.GameLoop.DayStarted += this.DayStartedUpdate;
            helper.Events.GameLoop.ReturnedToTitle += this.TitleReturnUpdate;
      
            tileTexture = helper.ModContent.Load<Texture2D>("assets/tile.png");
            tileTexture2 = helper.ModContent.Load<Texture2D>("assets/tile_2.png");
            tileTexture3 = helper.ModContent.Load<Texture2D>("assets/tile_3.png");



            //CalculateTileSum();

        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if ((e.NewLocation.Name.Contains("UndergroundMine") || e.NewLocation.Name.Contains("VolcanoDungeon")) && Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{e.NewLocation.NameOrUniqueName}.json") == null)
            {
                FloodFillTilesOnMap(e.NewLocation);
                tileDict.Add(e.NewLocation.NameOrUniqueName, FloodFillKaiTiles.ToList());
                ThisLocationTiles = FloodFillKaiTiles.ToList();
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            for (int i = 0; i < ThisLocationTiles.Count; i++)
            {
                KaiTile t = ThisLocationTiles[i];
                //Prevent player from being pushed out of bounds
                if (do_collision) PlayerCollisionCheck(t);
            }
        }

        private void removeSpecificTile(int xTile, int yTile, string gameLocation)
        {

            var tileData = this.Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation}.json") ?? new MapData();
            var tempList = tileData.AllKaiTilesList;


            for (int i = 0; i < tileData.AllKaiTilesList.Count; i++)
            {
                KaiTile t = tileData.AllKaiTilesList[i];

                if (t.IsSpecifiedTile(xTile, yTile, gameLocation)) {
                    
                    tempList.Remove(t);
                    RemoveProperties(t, Game1.getLocationFromName(gameLocation));

                }

            }
            var mapData = new MapData
            {
                AllKaiTilesList = tempList,
            };



            Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation}.json", mapData);
            tileList = new();



        }

        private void RemoveProperties(KaiTile tile, GameLocation gameLocation)
        {
            gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "Buildable");
            if (gameLocation.doesTileHavePropertyNoNull(tile.tileX, tile.tileY, "Type", "Back") == "Dirt"
                || gameLocation.doesTileHavePropertyNoNull(tile.tileX, tile.tileY, "Type", "Back") == "Grass") gameLocation.setTileProperty(tile.tileX, tile.tileY, "Back", "Diggable", "true");

            gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "NoFurtniture");
            gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "NoSprinklers");

            gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "Passable");
            gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "Placeable");


            ThisLocationTiles.Remove(tile);
            tileList.Remove(tile);

        }
        public void RemoveTileExceptions()
        {

            this.Monitor.Log("Removing Unusual Tiles", LogLevel.Debug);

            removeSpecificTile(18,27,"Desert");

            removeSpecificTile(12, 9, "BusStop");




        }

        public void AddTileExceptions()
        {

            this.Monitor.Log("Placing Unusual Tiles", LogLevel.Debug);

            var tempName = "Town";

            //ADD UNUSAL TILES HERE
            tileDict[tempName].Add(new KaiTile(21, 42, tempName));
            tileDict[tempName].Add(new KaiTile(21, 43, tempName));
            tileDict[tempName].Add(new KaiTile(21, 44, tempName));
            tileDict[tempName].Add(new KaiTile(21, 45, tempName));
            tileDict[tempName].Add(new KaiTile(21, 46, tempName));

            tileDict[tempName].Add(new KaiTile(49, 42, tempName));
            tileDict[tempName].Add(new KaiTile(49, 43, tempName));
            tileDict[tempName].Add(new KaiTile(49, 44, tempName));
            tileDict[tempName].Add(new KaiTile(49, 45, tempName));
            tileDict[tempName].Add(new KaiTile(49, 46, tempName));
            tileDict[tempName].Add(new KaiTile(49, 47, tempName));
            tileDict[tempName].Add(new KaiTile(49, 48, tempName));
            tileDict[tempName].Add(new KaiTile(49, 49, tempName));
            tileDict[tempName].Add(new KaiTile(49, 50, tempName));
            tileDict[tempName].Add(new KaiTile(49, 51, tempName));

            tileDict[tempName].Add(new KaiTile(55, 102, tempName));
            tileDict[tempName].Add(new KaiTile(55, 103, tempName));
            tileDict[tempName].Add(new KaiTile(55, 104, tempName));
            tileDict[tempName].Add(new KaiTile(55, 105, tempName));
            tileDict[tempName].Add(new KaiTile(55, 106, tempName));
            tileDict[tempName].Add(new KaiTile(55, 107, tempName));





            //Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{tempName}.json", mapData);

            //
            //specific tiles to add in /// COPY ABOVE
            //Mountain 3X3 50,6 -> 52,8

           
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            

            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;


            if (!Context.IsPlayerFree) return;

            if (Game1.player.isFakeEventActor) return;
            

            if (e.Button == SButton.G)
            {
                toggle_overlay = !toggle_overlay;
                this.Monitor.Log($"Tileman Overlay set to:{toggle_overlay}", LogLevel.Debug);
                if(toggle_overlay) Game1.playSoundPitched("coin", 1000 );
                if(!toggle_overlay) Game1.playSoundPitched("coin", 600 );

            }
            if (e.Button == SButton.H)
            {
                overlay_mode++;
                var mode = "Mouse";
                if (overlay_mode > 1)
                {
                    mode = "Controller";
                    overlay_mode = 0;
                }

                Monitor.Log($"Tileman Overlay Mode set to:{mode}", LogLevel.Debug);
                Game1.playSoundPitched("coin", 1200);


            }



            if (!toggle_overlay) return;
            

            if (e.Button.IsUseToolButton()) tool_button_pushed = true;


            

            



        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button.IsUseToolButton()) tool_button_pushed = false;




        }

        private void DayStartedUpdate(object sender, DayStartedEventArgs e)
        {
            if (do_loop)
                PlaceInMaps();
            if (modBalanceConfig.EnableWeeklyReset)
                WeeklyWipe();
            GetLocationTiles(Game1.currentLocation);
            
            


            //Game1.timeOfDay = 900;
            //Game1.dayOfMonth = 14;
            //Monitor.Log($"TIME OF DAY SET TO:{Game1.timeOfDay}",LogLevel.Debug);







        }

        private void TitleReturnUpdate(object sender, ReturnedToTitleEventArgs e)
        {
            ResetValues();
            
        }

        private void DrawUpdate(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            //Makes sure to not draw while a cutscene is happening
            if (Game1.CurrentEvent != null) {
                if (!Game1.CurrentEvent.playerControlSequence)
                {
                    return;

                }
            }
            GroupIfLocationChange(); // i don't know why this is done in the draw update and not UpdateTicked?
                                     // to be honest with you, this function's purpose is defunct anyway - USE A WARPED EVENT
            for (int i = 0; i < ThisLocationTiles.Count; i++)
            {
                KaiTile t = ThisLocationTiles[i];
                if (t.tileIsWhere == Game1.currentLocation.Name || Game1.currentLocation.Name == "Temp")
                {
                    if (toggle_overlay)
                    {
                        var texture = tileTexture;
                        var stringColor = Color.Gold;
                        //Cursor
                        if (overlay_mode == 1)
                        {
                                
                            if (Game1.currentCursorTile == new Vector2(t.tileX,t.tileY))
                            {
                                texture = tileTexture2;

                                    

                                if (Game1.player.Money < (int)dynamic_tile_price) // removing Math.Floor might break the other difficulties but IDC because
                                                                                  // difficulty 1 is the only good option realistically.
                                {
                                    stringColor = Color.Red;
                                    texture = tileTexture3;
                                }

                                e.SpriteBatch.DrawString(Game1.dialogueFont, $"${dynamic_tile_price}",
                                    new Vector2(Game1.getMousePosition().X, Game1.getMousePosition().Y - Game1.tileSize), stringColor);

                            }
                        }
                        //Keyboard or Controller
                        else
                        {
                            if (Game1.player.nextPositionTile().X == t.tileX && Game1.player.nextPositionTile().Y == t.tileY)
                            {
                                texture = tileTexture2;
                                    
                                if (Game1.player.Money < (int)dynamic_tile_price)
                                {
                                    texture = tileTexture3;
                                    stringColor = Color.Red;

                                }


                                e.SpriteBatch.DrawString(Game1.dialogueFont, $"${dynamic_tile_price}",
                                    new Vector2((t.tileX) * 64 - Game1.viewport.X, (t.tileY) * 64 - 64 - Game1.viewport.Y), stringColor);




                            }
                        }
                        t.DrawTile(texture, e.SpriteBatch);
                    }
                    //Prevent player from being pushed out of bounds
                    //if (do_collision) PlayerCollisionCheck(t); /////////////////// << Removed because WHY would you ever check collision in a draw event??
                    //Monitor.Log($"{Game1.currentLocation.doesTileHavePropertyNoNull(t.tileX,t.tileY, "Diggable", "back")}", LogLevel.Debug);
                }
            }
                if (tool_button_pushed) PurchaseTilePreCheck();
        }


        private static IEnumerable<GameLocation> GetLocations()
        {
            var locations = Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
            return locations;
        }
        private void GetTilePrice()
        {
            if (modBalanceConfig.ScalePriceByPercentage)
            {
                double min = 0;
                if (purchase_count <= 10) dynamic_tile_price = tile_price;
                else if (purchase_count <= 100) dynamic_tile_price = tile_price * 2;
                else if (purchase_count <= 1000) dynamic_tile_price = tile_price * 3;
                else if (purchase_count <= 10000) dynamic_tile_price = tile_price * 4;
                else dynamic_tile_price = tile_price * 5;

                if (purchase_count <= 10) min = modBalanceConfig.PercentageBaseMinimum;
                else if (purchase_count <= 100) min = modBalanceConfig.PercentageBaseMinimum + Math.Floor(modBalanceConfig.PercentageBaseMinimum / 3);
                else if (purchase_count <= 1000) min = modBalanceConfig.PercentageBaseMinimum * 2;
                else if (purchase_count <= 10000) min = modBalanceConfig.PercentageBaseMinimum * 5;
                else min = modBalanceConfig.PercentageBaseMinimum * modBalanceConfig.PercentageBaseMinimum;

                tile_price = Math.Floor(Math.Clamp(Game1.player.Money * modBalanceConfig.PercentageScaler, min, double.MaxValue));
            }

            switch (difficulty_mode)
            {
                case 0:
                    //Slowly increase tile cost over time // Change 0 for initial buffer
                    if (purchase_count > 0) dynamic_tile_price += tile_price_raise;
                    break;

                case 1:
                    //Increase tile cost through milestones
                    if (purchase_count <= 10)      dynamic_tile_price = tile_price;
                    else if (purchase_count <= 100)     dynamic_tile_price = tile_price * 2;
                    else if (purchase_count <= 1000)    dynamic_tile_price = tile_price * 3;
                    else if (purchase_count <= 10000) dynamic_tile_price = tile_price * 4;
                    else dynamic_tile_price = tile_price * 5;

                    break;

                case 2:
                    //Increment tile price with each one purchased
                    dynamic_tile_price = purchase_count;
                    break;
            }
        }

        private void PurchaseTilePreCheck()
        {

            for (int i = 0; i < ThisLocationTiles.Count; i++)
            {
                KaiTile t = ThisLocationTiles[i];
                //Cursor 
                if (overlay_mode == 1)
                {
                    if (Game1.currentCursorTile == new Vector2(t.tileX,t.tileY) )
                    {
                        PurchaseTileCheck(t);
                    }
                }
                //Keyboard or Controller
                else 
                {
                    if (Game1.player.nextPositionTile().X == t.tileX && Game1.player.nextPositionTile().Y == t.tileY)
                    {
                        PurchaseTileCheck(t);
                    }
                }

            }

        }

        private void PurchaseTileCheck(KaiTile thisTile)
        {
            int floor_price = (int)Math.Floor(dynamic_tile_price);

            if (Game1.player.Money >= floor_price)
            {
                
                Game1.player.Money -= floor_price;

                GetTilePrice();
                

                tile_count--;
                purchase_count++;

                Game1.playSoundPitched("purchase", 700 + (100* new Random().Next(0, 7)) );

                

                var gameLocation = Game1.currentLocation;

                gameLocation.removeTileProperty(thisTile.tileX, thisTile.tileY, "Back", "Buildable");
                if (gameLocation.doesTileHavePropertyNoNull(thisTile.tileX, thisTile.tileY, "Type", "Back") == "Dirt"
                        || gameLocation.doesTileHavePropertyNoNull(thisTile.tileX, thisTile.tileY, "Type", "Back") == "Grass") gameLocation.setTileProperty(thisTile.tileX, thisTile.tileY, "Back", "Diggable", "true");

                gameLocation.removeTileProperty(thisTile.tileX, thisTile.tileY, "Back", "NoFurniture");
                gameLocation.removeTileProperty(thisTile.tileX, thisTile.tileY, "Back", "NoSprinklers");

                gameLocation.removeTileProperty(thisTile.tileX, thisTile.tileY, "Back", "Passable");
                gameLocation.removeTileProperty(thisTile.tileX, thisTile.tileY, "Back", "Placeable");

                ThisLocationTiles.Remove(thisTile);
                tileList.Remove(thisTile);

            }
            else Game1.playSoundPitched("grunt", 700 + (100 * new Random().Next(0, 7)));





        }
        private void WeeklyWipe()
        {
            bool isWeek = Game1.dayOfMonth % 7 == 0;
            if (!isWeek)
                return;
            FloodFillMaps(true);
            Monitor.Log("Weekly wipe of tiles complete.", LogLevel.Info);
            Game1.hudMessages.Add(new HUDMessage("Weekly wipe of tiles has occurred!"));
        }
        private void PlaceInMaps()
        {
            FloodFillMaps(false);
            Monitor.Log("Press 'G' to toggle Tileman Overlay", LogLevel.Info); // swapped from Debug to Info - useful to remind the player of the keybinds!
            Monitor.Log("Press 'H' to switch between Overlay Modes", LogLevel.Info);
        }

        private void FloodFillMaps(bool weeklyWipe) // the entrance to hell
        {
            FloodFillKaiTiles = new Stack<KaiTile>();
            string mineString = "UndergroundMine"; // mines
            GameLocation[] minesLocations = new GameLocation[220 + caverns_extra];
            for (int i = 1; i - 1 < minesLocations?.Length; i++)
            {
                minesLocations[i - 1] = Game1.getLocationFromName(mineString + i);
            }

            string volcanoString = "VolcanoDungeon"; // volcano
            GameLocation[] volcanoLocations = new GameLocation[10];
            for (int i = 1; i - 1 < volcanoLocations?.Length; i++)
            {
                volcanoLocations[i - 1] = Game1.getLocationFromName(volcanoString + i);
            }

            foreach (GameLocation location in GetLocations()?.Concat(minesLocations)?.Concat(volcanoLocations))
            {
                if (location == null || location is null)
                    continue;
                if (location.Name == Game1.getFarm().Name || location.Name == "FarmHouse" || location.Name.Contains(mineString) || location.Name.Contains(volcanoString))
                    if (weeklyWipe)
                        continue;
                FloodFillTilesOnMap(location);
            }
            do_loop = false; // should have no impact but just a failsafe incase other functions rely on this bool being false after resetting maps
        }
        private void FloodFillTilesOnMap(GameLocation gl) // i don't even want to know the time efficiency for this entire function with all of its calls
        {
            List<Vector2> points = new List<Vector2>();
            if (!gl.NameOrUniqueName.Contains("UndergroundMine"))
            {
                foreach (KeyValuePair<string, xTile.ObjectModel.PropertyValue> kvp in gl.Map.Properties)
                {
                    if (kvp.Key != "Warp")
                        continue;
                    string[] split = kvp.Value.ToString().Split(' ');
                    for (int i = 0; i < split.Length / 5; i++) // format::      x  y  NAME  destX  destY | x  y  NAME  destX  destY
                    {
                        Point startingPoint = new Point(int.Parse(split[0 + (5 * i)]), int.Parse(split[1 + (5 * i)])); // offset splits by 5i
                        if (startingPoint.X > -1 && startingPoint.Y > -1)
                        {
                            points.Add(new Vector2(startingPoint.X, startingPoint.Y));
                            break;
                        }
                    }
                }
            }
            else
            {
                Vector2 tile = Vector2.One * -1;
                if (gl is MineShaft)
                {
                    tile = Helper.Reflection.GetField<NetVector2>((MineShaft)gl, "netTileBeneathLadder", true).GetValue();
                    points.Add(tile);
                }
            }
            FloodFillKaiTiles.Clear();
            var listOfLists = new List<List<Vector2>>();
            foreach (Vector2 point in points)
            {
                listOfLists.Add(FloodFillIterative(gl, (int)point.X, (int)point.Y));
            }
            var list = new List<Vector2>();
            try
            {
                list = listOfLists?.OrderByDescending(m => m.Count())?.First() ?? new List<Vector2>(); // biggest list - biggest always means bestest
            }
            catch
            {
                list = new List<Vector2>();
            }
            foreach (var tile in list)
            {
                FloodFillKaiTiles.Push(new KaiTile((int)tile.X, (int)tile.Y, gl.NameOrUniqueName));
            }
            tileDict.Add(gl.NameOrUniqueName, FloodFillKaiTiles.ToList()); // add valid tiles to the tileDict
        }
        private List<Vector2> FloodFillIterative(GameLocation gl, int startX, int startY) // the only documented function in this godforsaken document
        {
            int width = gl.Map.GetLayer("Back").LayerWidth;
            int height = gl.Map.GetLayer("Back").LayerHeight;

            var flooded = new List<Vector2>(); // tiles confirmed to be accessible - gets assigned into tileDict later
            var tested = new HashSet<Vector2>(); // tiles that have been tested - could be accessible or inaccessible, but won't be checked again
            var queue = new Queue<Vector2>(); // tiles that are yet to be tested

            var start = new Vector2(startX, startY);
            queue.Enqueue(start); // queue up the origin
            while (queue.Count > 0) // while still left to check any
            {
                var tile = queue.Dequeue();
                if (tile == start)
                {
                    foreach (var neighbour in GetFourNeighbours(tile, width, height)) // get the origin's neighbours
                        queue.Enqueue(neighbour); // queue the origin's neighbour
                    tested.Add(tile);
                    continue;
                }

                if (tile.X < 0 || tile.Y < 0 || tile.X > width || tile.Y > height        // if OOB, already tested, or tile is collideable
                    || !tested.Add(tile) || DoesTileCollide(gl, (int)tile.X, (int)tile.Y))
                    continue;                                                              // goto next tile

                flooded.Add(tile); // if this tile made it past the checks, it means it's accessible, so add it to the flooded list
                foreach (var neighbour in GetFourNeighbours(tile, width, height)) // get this tile's 4 neighbours and iterate them
                {
                    if (!tested.Contains(neighbour)) // as long as the neighbour hasn't already been tested
                    {
                        queue.Enqueue(neighbour); // queue the neighbour
                    }
                }
            }
            return flooded;
        }
        private List<Vector2> GetFourNeighbours(Vector2 tile, int width, int height) // could definitely have been done using a yield return LOL
        {
            List<Vector2> list = new List<Vector2>();

            if (tile.X - 1 >= 0)
                list.Add(new Vector2(tile.X - 1, tile.Y)); // left
            if (tile.X + 1 < width)
                list.Add(new Vector2(tile.X + 1, tile.Y)); // right
            if (tile.Y - 1 >= 0)
                list.Add(new Vector2(tile.X, tile.Y - 1)); // up
            if (tile.Y + 1 < height)                       // it will never cease to annoy me how these two are flipped.
                list.Add(new Vector2(tile.X, tile.Y + 1)); // down
            return list;
        }
        private bool DoesTileCollide(GameLocation gl, int x, int y)
            => !gl.isTilePassable(new xTile.Dimensions.Location(x, y), Game1.viewport) || gl.GetFurnitureAt(new Vector2(x, y)) != null || gl.terrainFeatures.TryGetValue(new Vector2(x, y), out _) || gl.getLargeTerrainFeatureAt(x, y) != null || gl.Objects.TryGetValue(new Vector2(x, y), out _) ||
            gl.getTileIndexAt(x, y, "Back") == -1 || gl.warps.Any(w => w.X == x && w.Y == y);


        private void PlaceInTempArea(GameLocation gameLocation)
        {
            Monitor.Log($"Placing Tiles in Temporary Area: {Game1.whereIsTodaysFest}", LogLevel.Debug);
            FloodFillKaiTiles.Clear();
            FloodFillTilesOnMap(gameLocation);
            ThisLocationTiles = FloodFillKaiTiles.ToList();
            tileList = new();
        }


        private void GroupIfLocationChange()

        {
            if (Game1.locationRequest != null)
            {
                if (Game1.locationRequest.Location != Game1.currentLocation && !location_changed)
                {
                    locationDelay = 35;
                    location_changed = true;

                    if (Game1.currentLocation.Name == "Temp") {
                        SaveLocationTiles(Game1.currentLocation);
                    }

                }
                
            }
            else if (location_changed)
            {
                if (locationDelay <= 0)
                {
                    //First encounter with specific Temp area
                    if(Game1.currentLocation.Name == "Temp") 
                    { 
                        if (Helper.Data.ReadJsonFile<MapData>($"jsons/" +
                            $"{Constants.SaveFolderName}/" +
                            $"{Game1.currentLocation.Name + Game1.whereIsTodaysFest}.json") == null)
                        {
                            PlaceInTempArea(Game1.currentLocation);
                        }else
                        {
                            Monitor.Log($"Grouping Tiles At: {Game1.currentLocation.NameOrUniqueName}", LogLevel.Debug);
                            GetLocationTiles(Game1.currentLocation);
 
                        }
                        location_changed = false;
                    }
                    else
                    {

                        Monitor.Log($"Grouping Tiles At: {Game1.currentLocation.NameOrUniqueName}", LogLevel.Debug);
                        GetLocationTiles(Game1.currentLocation);

                        location_changed = false;
                    }
                    
                }

                locationDelay--;

            }


        }
        private void SaveLocationTiles(GameLocation gameLocation)
        {

            var locationName = gameLocation.NameOrUniqueName;

            if (locationName == "Temp") locationName += Game1.whereIsTodaysFest;
            Monitor.Log($"Saving in {locationName}", LogLevel.Debug);


            var tileData = Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{locationName}.json") ?? new MapData();



            if (gameLocation.Name == "Temp")
            { tileData.AllKaiTilesList = ThisLocationTiles; }
            else
            {
                tileData.AllKaiTilesList = tileDict[locationName];
            }
            Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{locationName}.json", tileData);
            
        }
        private void GetLocationTiles(GameLocation gameLocation)
        {
            var locationName = gameLocation.NameOrUniqueName;

            if (locationName == "Temp") locationName += Game1.whereIsTodaysFest;

            if (tileDict.ContainsKey(locationName))
            {
                ThisLocationTiles = tileDict[locationName];
            }
            else 
            { 
                var tileData = Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{locationName}.json") ?? new MapData();
                if(tileData.AllKaiTilesList.Count > 0) ThisLocationTiles = tileData.AllKaiTilesList;
                if(gameLocation.Name != "Temp")tileDict.Add(locationName, ThisLocationTiles);
            }

            if (gameLocation.Name != "Temp")
            {
                for (int i = 0; i < ThisLocationTiles.Count; i++)
                {
                    var t = ThisLocationTiles[i];

                    if (!allow_player_placement)
                    {
                        gameLocation.removeTileProperty(t.tileX, t.tileY, "Back", "Diggable");

                        gameLocation.setTileProperty(t.tileX, t.tileY, "Back", "Buildable", "false");
                        gameLocation.setTileProperty(t.tileX, t.tileY, "Back", "NoFurniture", "true");
                        gameLocation.setTileProperty(t.tileX, t.tileY, "Back", "NoSprinklers", "");
                        gameLocation.setTileProperty(t.tileX, t.tileY, "Back", "Placeable", "");
                    }
                    //if (do_collision) gameLocation.setTileProperty(t.tileX, t.tileY, "Back", "Passable", "");



                }
            }

        }

       
        private void ResetValues()
        {
            do_loop = true;
            toggle_overlay = true;
            do_collision = true;

            tile_price = 1.0;
            tile_price_raise = 0.20;
            purchase_count = 0;
            tile_count = 0;

            tileList.Clear();
            ThisLocationTiles.Clear();

            tileDict.Clear();




    }



        public int CalculateTileSum(int tileCount = 50000, double price = 1.0, double priceIncrease = 0.0008) // no clue, not even gonna touch this.
        {
            var totalCost = 0;
            switch (difficulty_mode) {
                case 0:
                                       

                    for (int i = 0; i < tileCount; i++)
                    {
                        totalCost += (int)Math.Floor(price);
                        price += priceIncrease;

                    }
                    break;

                case 1:
                    if (purchase_count <= 10) dynamic_tile_price = tile_price;
                    else if (purchase_count <= 100) dynamic_tile_price = tile_price * 2;
                    else if (purchase_count <= 1000) dynamic_tile_price = tile_price * 3;
                    else if (purchase_count <= 10000) dynamic_tile_price = tile_price * 4;
                    else dynamic_tile_price = tile_price * 5;
                    price = tile_price;


                    for (int i = 0; i < tileCount; i++) 
                    {
                        totalCost += (int)price;
                    }

                    break;

                case 2:
                    price = tile_price;


                    for (int i = 0; i < tileCount; i++)
                    {
                        totalCost += (int)price;
                        if (purchase_count > 10) price = 2.0;
                        if (purchase_count > 100) price = 3.0;
                        if (purchase_count > 1000) price = 4.0;
                        if (purchase_count > 10000) price = 5.0;

                    }

                    break;
            }
            this.Monitor.Log($"Cost of {tileCount} tiles by the end: {totalCost}", LogLevel.Debug);

            return totalCost;
        }

        /*public void BuyAllTilesInLocation(GameLocation gameLocation) ////////////////////////////////////////////////////////// why is this here if it's never used?????
        {
            var tileData = this.Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation}.json") ?? new MapData();
            tileList = tileData.AllKaiTilesList;

            if (CalculateTileSum(tileList.Count) <= Game1.player.Money)
            {
                for (int i = 0; i < tileList.Count; i++)
                {
                    PurchaseTileCheck(tileList[i]);
                }


                var mapData = new MapData
                {
                    AllKaiTilesList = tileList,
                };



                Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation}.json", mapData);
                tileList=new();
            }

        } */

        private void PlayerCollisionCheck(KaiTile tile) // why is this called in the draw loop? nobody knows. so i made it get called in uhhhhh update ticked instead
        {

            if (Game1.getLocationFromName(tile.tileIsWhere) == Game1.currentLocation || Game1.currentLocation.Name == "Temp")
            {
                

                Rectangle tileBox = new(tile.tileX * 64, tile.tileY * 64, tile.tileW, tile.tileH);
                Rectangle playerBox = Game1.player.GetBoundingBox();
                
                if (playerBox.Intersects(tileBox))
                {
                    if (collisionTick > 120)
                    {
                        collisionTick = 0;
                        PurchaseTileCheck(tile);

                    }

                    var xDist  = playerBox.Right  - tileBox.Left;
                    var xDist2 = tileBox.Right    - playerBox.Left;
                    var yDist  = playerBox.Bottom - tileBox.Top;
                    var yDist2 = tileBox.Bottom   - playerBox.Top;
                    var xOffset = 0;

                    if (Game1.player.movementDirections.Count > 1
                    && (Game1.player.movementDirections[0] == 3 || Game1.player.movementDirections[1] == 3)) xOffset = 20;

                    if (Math.Abs(xDist - xDist2) >= Math.Abs(yDist - yDist2) + xOffset)
                    {
                        //Collide from Left
                        if (xDist >= xDist2)
                        {

                            var newPos = new Vector2(Game1.player.Position.X + xDist2, Game1.player.Position.Y);
                            Game1.player.Position = newPos;

                        }
                        //Collide from Right
                        else
                        {

                            var newPos = new Vector2(Game1.player.Position.X - xDist, Game1.player.Position.Y);
                            Game1.player.Position = newPos;

                        }
                    }
                    else {
                        //Collide from Top
                        if (yDist >= yDist2)
                            {

                                var newPos = new Vector2(Game1.player.Position.X, Game1.player.Position.Y + yDist2);
                                Game1.player.Position = newPos;

                            }
                            //Collide from Bottom
                            else
                            {

                                var newPos = new Vector2(Game1.player.Position.X, Game1.player.Position.Y - yDist);
                                Game1.player.Position = newPos;

                            }
                        }
                                          
                        

                        
                            
                    collisionTick++;

                }
                if (playerBox.Center == tileBox.Center || playerBox.Intersects(tileBox) && locationDelay > 0)
                {
                    if (collisionTick > 120)
                    {
                        collisionTick = 0;
                        PurchaseTileCheck(tile);

                    }
                    Game1.player.Position = Game1.player.lastPosition;
                    collisionTick++;


                }
                

                

            }

        }

        

        private void SaveModData(object sender, SavedEventArgs e) {


            foreach (KeyValuePair<string, List<KaiTile>> entry in tileDict)
            { 
                SaveLocationTiles(Game1.getLocationFromName(entry.Key));
            }
            tileDict.Clear();

            var tileData = new ModData
            {
                ToPlaceTiles   = do_loop,
                DoCollision    = do_collision,
                ToggleOverlay  = toggle_overlay,
                TilePrice      = tile_price,
                TilePriceRaise = tile_price_raise,
                CavernsExtra   = caverns_extra,
                DifficultyMode = difficulty_mode,
                PurchaseCount  = purchase_count
            };

            Helper.Data.WriteJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config_tiles.json", tileData);
        }

        private void LoadModData(object sender, SaveLoadedEventArgs e)
        {


            var tileData = Helper.Data.ReadJsonFile<ModData>("config_tiles.json") ?? new ModData();
            
            //Load config Information
            if (Helper.Data.ReadJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config_tiles.json") != null)
            {
                tileData = Helper.Data.ReadJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config_tiles.json") ?? new ModData();

            }
            else
            {
                Helper.Data.WriteJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config_tiles.json", tileData);
            }


            do_loop = tileData.ToPlaceTiles;
            toggle_overlay = tileData.ToggleOverlay;
            do_collision = tileData.DoCollision;
            tile_price = tileData.TilePrice;
            tile_price_raise = tileData.TilePriceRaise;
            caverns_extra = tileData.CavernsExtra;
            difficulty_mode = tileData.DifficultyMode;
            purchase_count = tileData.PurchaseCount;

            //
            //Load Individual Location Information

            System.IO.DirectoryInfo root = new($"{Constants.GamePath}/Mods/Tileman/jsons/{Constants.SaveFolderName}");

            System.IO.FileInfo[] files = root.GetFiles();
            /*foreach (System.IO.FileInfo file in files)
            {
                //Note, file.Name has a '.json' at the end, which messes things up with dictionary keys
                var fileName = System.IO.Path.GetFileNameWithoutExtension($"{Constants.GamePath}/Mods/Tileman/jsons/{Constants.SaveFolderName}/{file.Name}");

                if (fileName != "config")
                {
                    
                    var fileData = Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{fileName}.json") ?? new MapData();
                    tileDict.Add(fileName, fileData.AllKaiTilesList);

                }

            }*/

            Monitor.Log("Mod Data Loaded", LogLevel.Debug);

        }

        public void createJson(string fileName)
        {
            Monitor.Log($"Creating {fileName}.json", LogLevel.Debug);
            System.IO.File.Create($"jsons/{fileName}.json");
        }
        public class ModBalanceConfig
        {
            public bool EnableWeeklyReset { get; set; } = true;
            public bool ScalePriceByPercentage { get; set; } = true;
            public double PercentageScaler { get; set; } = 0.025;
            public double PercentageBaseMinimum { get; set; } = 3;
            public double PercentageBaseMaximum { get; set; } = 500;
        }
    }


}
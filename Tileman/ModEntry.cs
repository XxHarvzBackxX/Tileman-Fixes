using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;


namespace Tileman
{
    public class ModEntry : Mod
    {



        public bool do_loop = true;
        public bool do_collision = true;
        public bool allow_player_placement = false;
        public bool toggle_overlay = true;
        private bool tool_button_pushed = false;


        public double tile_price = 1.0;
        public double tile_price_raise = 0.0008;


        public int caverns_extra = 0;
        public int difficulty_mode = 0;
        public int purchase_count=0;

        int tile_count;

        int amountLocations = 200;
        int locationDelay = 0;

        List<KaiTile> tileList = new();
        List<KaiTile> ThisLocationTiles = new();

        Texture2D tileTexture  = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);
        Texture2D tileTexture2 = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);
        Texture2D tileTexture3 = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);

        

       




        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.RenderedWorld += this.DrawUpdate;


            helper.Events.GameLoop.Saved += this.SaveModData;
            helper.Events.GameLoop.SaveLoaded += this.LoadModData;
            helper.Events.GameLoop.DayStarted += this.DayStartedUpdate;
            helper.Events.GameLoop.ReturnedToTitle += this.TitleReturnUpdate;
      
            tileTexture = helper.ModContent.Load<Texture2D>("assets/tile.png");
            tileTexture2 = helper.ModContent.Load<Texture2D>("assets/tile_2.png");
            tileTexture3 = helper.ModContent.Load<Texture2D>("assets/tile_3.png");



            //CalculateTileSum();

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
                    var location = Game1.getLocationFromName(gameLocation);
                    location.removeTileProperty(t.tileX, t.tileY, "Back", "Buildable");
                    if (location.doesTileHavePropertyNoNull(t.tileX, t.tileY, "Type", "Back") == "Dirt"
                        || location.doesTileHavePropertyNoNull(t.tileX, t.tileY, "Type", "Back") == "Grass") location.setTileProperty(t.tileX, t.tileY, "Back", "Diggable", "true");

                    location.removeTileProperty(t.tileX, t.tileY, "Back", "NoFurtniture");
                    location.removeTileProperty(t.tileX, t.tileY, "Back", "NoSprinklers");

                    location.removeTileProperty(t.tileX, t.tileY, "Back", "Passable");
                    location.removeTileProperty(t.tileX, t.tileY, "Back", "Placeable");

                }

            }
            var mapData = new MapData
            {
                AllKaiTilesList = tempList,
            };



            Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation}.json", mapData);
            tileList = new();



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

            var tileData = Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{tempName}.json") ?? new MapData();

            var tempTiles = tileData.AllKaiTilesList;

            //ADD UNUSAL TILES HERE
            tempTiles.Add(new KaiTile(21, 42, tempName));
            tempTiles.Add(new KaiTile(21, 43, tempName));
            tempTiles.Add(new KaiTile(21, 44, tempName));
            tempTiles.Add(new KaiTile(21, 45, tempName));
            tempTiles.Add(new KaiTile(21, 46, tempName));

            tempTiles.Add(new KaiTile(49, 42, tempName));
            tempTiles.Add(new KaiTile(49, 43, tempName));
            tempTiles.Add(new KaiTile(49, 44, tempName));
            tempTiles.Add(new KaiTile(49, 45, tempName));
            tempTiles.Add(new KaiTile(49, 46, tempName));
            tempTiles.Add(new KaiTile(49, 47, tempName));
            tempTiles.Add(new KaiTile(49, 48, tempName));
            tempTiles.Add(new KaiTile(49, 49, tempName));
            tempTiles.Add(new KaiTile(49, 50, tempName));
            tempTiles.Add(new KaiTile(49, 51, tempName));

            tempTiles.Add(new KaiTile(55, 102, tempName));
            tempTiles.Add(new KaiTile(55, 103, tempName));
            tempTiles.Add(new KaiTile(55, 104, tempName));
            tempTiles.Add(new KaiTile(55, 105, tempName));
            tempTiles.Add(new KaiTile(55, 106, tempName));
            tempTiles.Add(new KaiTile(55, 107, tempName));

            


            var mapData = new MapData
            {
                AllKaiTilesList = tempTiles,
            };

            //tileList.Add(tempTiles);
            tempTiles = new();



            Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{tempName}.json", mapData);

            //
            //specific tiles to add in /// COPY ABOVE
            //Mountain 3X3 50,6 -> 52,8

           
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            

            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;


            if (!Context.IsPlayerFree) return;

            if (Game1.CurrentEvent != null) return;
            

            if (e.Button == SButton.G)
            {
                toggle_overlay = !toggle_overlay;
                this.Monitor.Log($"Tileman Overlay set to:{toggle_overlay}", LogLevel.Debug);
                if(toggle_overlay) Game1.playSoundPitched("coin", 1000 );
                if(!toggle_overlay) Game1.playSoundPitched("coin", 600 );

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

            PlaceInMaps();
            GetLocationTiles(Game1.currentLocation);




        }

        private void TitleReturnUpdate(object sender, ReturnedToTitleEventArgs e)
        {
            ResetValues();
            
        }

        private void DrawUpdate(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            
            if (Game1.CurrentEvent != null )  return;

            GroupIfLocationChange();

            if (locationDelay > 0) locationDelay--;


            if (Game1.spriteBatch.Equals(null) == false && ThisLocationTiles != null)
            {

                for (int i = 0; i < ThisLocationTiles.Count; i++)
                {
                    KaiTile t = ThisLocationTiles[i];
                    if (t != null && Game1.getLocationFromName(t.tileIsWhere) == Game1.currentLocation)
                    {
                        if (toggle_overlay)
                        {
                            var texture = tileTexture;

                            /*if (Math.Floor((Double)((Game1.getMousePosition().X + Game1.viewport.X) / 64)) == t.tileX &&
                                Math.Floor((Double)((Game1.getMousePosition().Y + Game1.viewport.Y) / 64)) == t.tileY)
                            {
                                texture = tileTexture2;

                                e.SpriteBatch.DrawString(Game1.dialogueFont, $"${ (int)Math.Floor(tile_price)}",
                                 new Vector2(Game1.getMousePosition().X + Game1.viewport.X, Game1.getMousePosition().Y + Game1.viewport.Y), Color.Gold);

                                if (Game1.player.Money < (int)Math.Floor(tile_price)) { texture = tileTexture3; }

                            }*/
                            if (Game1.player.nextPositionTile().X == t.tileX && Game1.player.nextPositionTile().Y == t.tileY)
                            {
                                texture = tileTexture2;
                                var stringColor = Color.Gold;
                                if (Game1.player.Money < (int)Math.Floor(tile_price)) { 
                                    texture = tileTexture3;
                                    stringColor = Color.Red;
                                
                                }


                                e.SpriteBatch.DrawString(Game1.dialogueFont, $"${ (int)Math.Floor(tile_price)}",
                                 new Vector2((t.tileX ) * 64 - Game1.viewport.X, (t.tileY ) * 64 - 64 - Game1.viewport.Y), stringColor);




                            }

                            t.DrawTile(texture, e.SpriteBatch);
                        }
                         

                        //Prevent player from being pushed out of bounds
                        if(do_collision) PlayerCollisionCheck(t);
                        //Monitor.Log($"{Game1.currentLocation.doesTileHavePropertyNoNull(t.tileX,t.tileY, "Diggable", "back")}", LogLevel.Debug);

                    }




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
        


        private bool IsTileAt(int tileX, int tileY, GameLocation TileIsAt)
        {
            foreach (KaiTile t in tileList)
            {
                if (tileX == t.tileX && tileY == t.tileY && TileIsAt == Game1.getLocationFromName(t.tileIsWhere))
                {
                    return true;
                }

            }
            return false;


        }

        private void PurchaseTilePreCheck()
        {

            for (int i = 0; i < ThisLocationTiles.Count; i++)
            {

                KaiTile t = ThisLocationTiles[i];
                //controller or keyboard

                //if cursor on tile
                if (/*Game1.curso && e.Cursor.Tile.X == t.tileX && e.Cursor.Tile.Y == t.tileY ||*/
                Game1.player.nextPositionTile().X == t.tileX && Game1.player.nextPositionTile().Y == t.tileY)
                {
                    PurchaseTileCheck(t);
                }

                //mouse
                /*else
                {
                    if (e.Cursor.Tile.X == t.tileX && e.Cursor.Tile.Y == t.tileY )
                    {
                        PurchaseTileCheck(t);
                    }

                }*/

            }

        }

        private void PurchaseTileCheck(KaiTile thisTile)
        {
            int floor_price = (int)Math.Floor(tile_price);

            if (Game1.player.Money >= floor_price)
            {
                
                Game1.player.Money -= floor_price;

                switch(difficulty_mode)
                {
                    case 0: 
                        //Slowly increase tile cost over time // Change 0 for initial buffer
                        if (purchase_count > 0) tile_price += tile_price_raise;  
                        break;

                    case 1:
                        //Increase tile cost through milestones
                        if (purchase_count > 10) tile_price = 2;
                        if (purchase_count > 100) tile_price = 3;
                        if (purchase_count > 1000) tile_price = 4;
                        if (purchase_count > 10000) tile_price = 5;
                        break;

                    case 3:
                        //Increment tile price with each one purchased
                        tile_price++;
                        break;
                }

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

        private void PlaceInMaps()
        {
            if (Context.IsWorldReady)
            {



                if (do_loop == true)
                {


                    var locationCount = 0;
                    foreach (GameLocation location in GetLocations())
                    {
                        

                        Monitor.Log($"Placing Tiles in: {location.Name}", LogLevel.Debug);

                        locationCount++;

                        if (locationCount < amountLocations)
                        {
                            PlaceTiles(Game1.getLocationFromName(location.NameOrUniqueName));

                        }
                        else
                        {
                            break;
                        }


                        var mapData = new MapData
                        {
                            AllKaiTilesList = tileList,
                        };



                        Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{location.Name}.json", mapData);
                        tileList = new();
                    }

                    
                    //Place Tiles in the Mine // Mine 1-120 // Skull Caverns 121-???
                    for (int i = 1; i <= 220 + caverns_extra; i++)

                    {
                        var mineString = Game1.getLocationFromName("UndergroundMine" + i).Name;
                        if (Game1.getLocationFromName(mineString) != null)
                        {
                            PlaceTiles(Game1.getLocationFromName(mineString));
                            Monitor.Log($"Placing Tiles in: {mineString}", LogLevel.Debug);


                            var mapData = new MapData
                            {
                                AllKaiTilesList = tileList,
                            };



                            Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{mineString}.json", mapData);
                            tileList = new();

                        }
                    }

                    //VolcanoDungeon0 - 9
                    for (int i = 0; i <= 9; i++)

                    {
                        var mineString = Game1.getLocationFromName("VolcanoDungeon" + i).Name;
                        if (Game1.getLocationFromName(mineString) != null)
                        {
                            PlaceTiles(Game1.getLocationFromName(mineString));
                            Monitor.Log($"Placing Tiles in: {mineString}", LogLevel.Debug);

                            var mapData = new MapData
                            {
                                AllKaiTilesList = tileList,
                            };



                            Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{mineString}.json", mapData);
                            tileList = new();

                        }
                    }

                    AddTileExceptions();
                    RemoveTileExceptions();
                

                    do_loop = false;

                    Monitor.Log("Press 'G' to toggle Tileman Overlay", LogLevel.Debug);

                }
            }

        }

        private void PlaceTiles(GameLocation mapLocation)
        {

            int mapWidth = mapLocation.map.Layers[0].LayerWidth;
            int mapHeight = mapLocation.map.Layers[0].LayerHeight;


            for (int i = 1; i < mapWidth - 1; i++)
            {
                for (int j = 1; j < mapHeight - 1; j++)
                {
                    if (/*!IsTileAt(i, j, mapLocation)
                        &&*/ !mapLocation.isObjectAtTile(i, j) 
                        && !mapLocation.isOpenWater(i, j) 
                        && !mapLocation.isTerrainFeatureAt(i, j) 
                        && mapLocation.isTilePlaceable(new Vector2(i, j))
                        && mapLocation.isTileLocationTotallyClearAndPlaceable(new Vector2(i, j))
                        && mapLocation.Map.Layers[0].IsValidTileLocation(i,j) 

                        )
                    {
                        if (new Vector2(Game1.player.position.X, Game1.player.position.Y)!= new Vector2(i, j))
                        {
                            var t = new KaiTile(i, j, mapLocation.Name);
                            tileList.Add(t);


                            tile_count++;
                        }
                    }
                }
            }


            
            

        }

        

        private void GroupIfLocationChange()
        {
            if (Game1.locationRequest != null )
            {
                if (Game1.locationRequest.Location != Game1.currentLocation)
                {
                    SaveLocationTiles(Game1.currentLocation);

                    Monitor.Log($"Grouping Tiles At: {Game1.locationRequest.Location.NameOrUniqueName}",LogLevel.Debug);

                    GetLocationTiles(Game1.locationRequest.Location);

                    locationDelay = 20;

                }  
            }

        }
        private void SaveLocationTiles(GameLocation gameLocation)
        {
            var tileData = this.Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation.Name}.json") ?? new MapData();
            if (ThisLocationTiles.Count > 0
                && gameLocation.Name == ThisLocationTiles[0].tileIsWhere)
            {
                tileData.AllKaiTilesList = ThisLocationTiles;
                Helper.Data.WriteJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation.Name}.json", tileData);
            }
        }
        private void GetLocationTiles(GameLocation gameLocation)
        {   
            var tileData = this.Helper.Data.ReadJsonFile<MapData>($"jsons/{Constants.SaveFolderName}/{gameLocation.Name}.json") ?? new MapData();
            ThisLocationTiles = tileData.AllKaiTilesList;
            
            for(int i = 0; i < ThisLocationTiles.Count; i++)
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
                if (do_collision) gameLocation.setTileProperty(t.tileX, t.tileY, "Back", "Passable", "");
                
                

            }

        }

       
        private void ResetValues()
        {
            do_loop = true;
            toggle_overlay = true;

            tile_price = 1.0;
            tile_price_raise = 0.20;
            purchase_count = 0;
            tile_count = 0;

            tileList.Clear();
            ThisLocationTiles.Clear();




    }



        public int CalculateTileSum(int tileCount = 50000, double price = 1.0, double priceIncrease = 0.0008)
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

        public void BuyAllTilesInLocation(GameLocation gameLocation)
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

        }

        private void PlayerCollisionCheck(KaiTile tile)
        {

            if (Game1.getLocationFromName(tile.tileIsWhere) == Game1.currentLocation)
            {

                Rectangle tileBox = new(tile.tileX * 64, tile.tileY * 64, tile.tileW, tile.tileH);
                Rectangle playerBox = Game1.player.GetBoundingBox();

                if (playerBox.Center == tileBox.Center || playerBox.Intersects(tileBox) && locationDelay > 0)
                {
                    var gameLocation = Game1.currentLocation;
                    gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "Buildable");
                    if (gameLocation.doesTileHavePropertyNoNull(tile.tileX, tile.tileY, "Type", "Back") == "Dirt"
                        || gameLocation.doesTileHavePropertyNoNull(tile.tileX, tile.tileY, "Type", "Back") == "Grass")gameLocation.setTileProperty(tile.tileX, tile.tileY, "Back", "Diggable","true");

                    gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "NoFurtniture");
                    gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "NoSprinklers");

                    gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "Passable");
                    gameLocation.removeTileProperty(tile.tileX, tile.tileY, "Back", "Placeable");


                    ThisLocationTiles.Remove(tile);
                    tileList.Remove(tile);
                }

                

            }

        }

        

        private void SaveModData(object sender, SavedEventArgs e) {

            //if (System.IO.File.Exists("config")) createJson("config");
            SaveLocationTiles(Game1.currentLocation);

            var tileData = new ModData
            {
                ToPlaceTiles   = do_loop,
                ToggleOverlay  = toggle_overlay,
                TilePrice      = tile_price,
                TilePriceRaise = tile_price_raise,
                CavernsExtra   = caverns_extra,
                DifficultyMode = difficulty_mode,
                PurchaseCount  = purchase_count
            };

            //this.Helper.Data.WriteSaveData("example-key", tileData);
            Helper.Data.WriteJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config.json", tileData);
        }

        private void LoadModData(object sender, SaveLoadedEventArgs e) {

            Monitor.Log("Mod Data Loaded",LogLevel.Debug);

            var tileData = Helper.Data.ReadJsonFile<ModData>("config.json") ?? new ModData();

            if (Helper.Data.ReadJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config.json") != null)
            {
                tileData = Helper.Data.ReadJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config.json") ?? new ModData();

            }
            else
            {
                Helper.Data.WriteJsonFile<ModData>($"jsons/{Constants.SaveFolderName}/config.json", tileData);
            }
            

            do_loop          = tileData.ToPlaceTiles;
            toggle_overlay   = tileData.ToggleOverlay;
            do_collision     = tileData.DoCollision;
            tile_price       = tileData.TilePrice;
            tile_price_raise = tileData.TilePriceRaise;
            caverns_extra    = tileData.CavernsExtra;
            difficulty_mode  = tileData.DifficultyMode;
            purchase_count   = tileData.PurchaseCount;



        }

        public void createJson(string fileName)
        {
            Monitor.Log($"Creating {fileName}.json", LogLevel.Debug);
            System.IO.File.Create($"jsons/{fileName}.json");
        }






    }


}
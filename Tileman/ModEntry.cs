using System;
using System.Collections.Generic;
using System.Linq;
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



        bool do_loop = true;
        bool toggle_overlay = true;
        public double tile_price = 1.0;
        double tile_price_raise = 0.0008;
        int purchase_count=0;
        int tile_count;


        List<KaiTile> allTiles = new();
        List<KaiTile> ThisLocationTiles = new();

        Texture2D tileTexture  = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);
        Texture2D tileTexture2 = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);
        Texture2D tileTexture3 = new(Game1.game1.GraphicsDevice, Game1.tileSize, Game1.tileSize);

        public static readonly string dictPath = "Mods/SpicyKai.Tileman/tileData.json";




        public override void Entry(IModHelper helper)
        {
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





        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        /// 
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet

            if (!Context.IsWorldReady) return;
            

            if (!Context.IsPlayerFree) return;

            if (Game1.CurrentEvent != null)
            {
                return;
            }

            if (e.Button == SButton.G)
            {
                toggle_overlay = !toggle_overlay;
                this.Monitor.Log($"Tileman Overlay set to:{toggle_overlay}", LogLevel.Debug);
            }

            if (!toggle_overlay) return;
            

            if (e.Button.IsUseToolButton() /*|| e.IsDown(SButton.MouseRight*/)
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

            



        }

        private void DayStartedUpdate(object sender, DayStartedEventArgs e)
        {
            PlaceInMaps();



        }

        private void TitleReturnUpdate(object sender, ReturnedToTitleEventArgs e)
        {
            ResetValues();
            
        }

        private void DrawUpdate(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (Game1.CurrentEvent != null) {
                return;
            }

            
            
            GroupIfLocationChange();

            if (Game1.spriteBatch.Equals(null) == false && allTiles != null)
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
                                
                                e.SpriteBatch.DrawString(Game1.dialogueFont, $"${ (int)Math.Floor(tile_price)}",
                                 new Vector2((t.tileX ) * 64 - Game1.viewport.X, (t.tileY ) * 64 - 64 - Game1.viewport.Y), Color.Gold);

                                if (Game1.player.Money < (int)Math.Floor(tile_price)) { texture = tileTexture3; }



                            }

                            t.DrawTile(texture, e.SpriteBatch);

                        }
                        t.PlayerCollisionCheck();

                    }




                }




            }



        }

        private static IEnumerable<GameLocation> GetLocations()
        {


            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }

        private bool IsTileAt(int tileX, int tileY, GameLocation TileIsAt)
        {
            foreach (KaiTile t in allTiles)
            {
                if (tileX == t.tileX && tileY == t.tileY && TileIsAt == Game1.getLocationFromName(t.tileIsWhere))
                {
                    return true;
                }

            }
            return false;


        }

        private void PurchaseTileCheck(KaiTile thisTile)
        {
            int floor_price = (int)Math.Floor(tile_price);

            if (Game1.player.Money >= floor_price)
            {
                
                Game1.player.Money -= floor_price;
                Monitor.Log($"Bought Tile({thisTile.tileX},{thisTile.tileY}) for ${floor_price}", LogLevel.Debug);

                //RAISE THIS NUMBER TO ADD AN INITIAL BUFFER BEFORE PRICE INCREASES
                if (purchase_count > 0)
                {
                    tile_price += tile_price_raise;
                }
                tile_count--;
                purchase_count++;

                Game1.playSoundPitched("purchase", 700 + (100* new Random().Next(0, 7)) );
                


                allTiles.Remove(thisTile);
                ThisLocationTiles.Remove(thisTile);

            }
            else Game1.playSoundPitched("grunt", 700 + (100 * new Random().Next(0, 7)));





        }

        private void PlaceInMaps()
        {
            if (Context.IsWorldReady)
            {



                if (do_loop == true)
                {
                    Monitor.Log("Placing Tiles...", LogLevel.Debug);


                    var locationCount = 0;
                    foreach (GameLocation location in GetLocations())
                    {
                        locationCount++;

                        if (locationCount < 10)
                        {
                            PlaceTiles(location);

                        }
                        else
                        {
                            break;
                        }


                    }

                    //Locations not found from GetLocations
                    //PlaceTiles(Game1.getLocationFromName("Mineshaft"));

                    
                    RemoveTileExceptions();
                    AddTileExceptions();

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
                    if (!IsTileAt(i, j, mapLocation)
                        && !mapLocation.isObjectAtTile(i, j) 
                        && !mapLocation.isOpenWater(i, j) 
                        && !mapLocation.isTerrainFeatureAt(i, j) 
                        && mapLocation.isTilePlaceable(new Vector2(i, j))
                        && mapLocation.isTileLocationTotallyClearAndPlaceable(new Vector2(i, j))
                        && mapLocation.Map.Layers[0].IsValidTileLocation(i,j) 

                        )
                    {
                        //mapLocation.map.Layers[0].PickTile().Properties
                        if (new Vector2(Game1.player.position.X, Game1.player.position.Y)!= new Vector2(i, j))
                        {
                            var t = new KaiTile(i, j, mapLocation.Name);
                            allTiles.Add(t);

                            if(mapLocation.Name == t.tileIsWhere)
                            {
                                ThisLocationTiles.Add(t);
                            }




                            tile_count++;
                            ///this.Monitor.Log($"Tile #{tile_count} At {mapLocation.Name}", LogLevel.Debug);
                        }
                    }
                }
            }
            
            

        }

        private void RemoveTileExceptions() {

            this.Monitor.Log("removing unusual tiles", LogLevel.Debug);

            //
            //Specific tiles to take out
            //Mine (18,14)(18,13)
            //Desert (18,27)
            //Club (8,12)
            //CommunityCenter (32,23)(33,23)
            //IslandShrine (13,27)(13,28)(14,27)(14,28)
            //IslandEast (22,11)(33,30)
            //IslandHut (7,12)(7, 13)(7,14)
            //IslandFieldOffice (4,9)
            //IslandLocation (6,10)
            for (int i = 0; i < allTiles.Count; i++)
            {
                KaiTile t = allTiles[i];

                if (t.IsSpecifiedTile(6, 30, "SeedShop")) allTiles.Remove(t);
                if (t.IsSpecifiedTile(6, 29, "SeedShop")) allTiles.Remove(t);
                if (t.IsSpecifiedTile(6, 28, "SeedShop")) allTiles.Remove(t);

                if (t.IsSpecifiedTile(8, 11, "FarmCave")) allTiles.Remove(t);
                if (t.IsSpecifiedTile(8, 12, "FarmCave")) allTiles.Remove(t);

            }



        }

        private void AddTileExceptions() {

            this.Monitor.Log("Placing unusual tiles", LogLevel.Debug);

            //
            //specific tiles to add in
            //Town (21,42) -> (21,46)

            allTiles.Add(new KaiTile(21, 42, "Town"));
            allTiles.Add(new KaiTile(21, 43, "Town"));
            allTiles.Add(new KaiTile(21, 44, "Town"));
            allTiles.Add(new KaiTile(21, 45, "Town"));
            allTiles.Add(new KaiTile(21, 46, "Town"));

            allTiles.Add(new KaiTile(49, 42, "Town"));
            allTiles.Add(new KaiTile(49, 43, "Town"));
            allTiles.Add(new KaiTile(49, 44, "Town"));
            allTiles.Add(new KaiTile(49, 45, "Town"));
            allTiles.Add(new KaiTile(49, 46, "Town"));
            allTiles.Add(new KaiTile(49, 47, "Town"));
            allTiles.Add(new KaiTile(49, 48, "Town"));
            allTiles.Add(new KaiTile(49, 49, "Town"));
            allTiles.Add(new KaiTile(49, 50, "Town"));
            allTiles.Add(new KaiTile(49, 51, "Town"));

            allTiles.Add(new KaiTile(55, 102, "Town"));
            allTiles.Add(new KaiTile(55, 103, "Town"));
            allTiles.Add(new KaiTile(55, 104, "Town"));
            allTiles.Add(new KaiTile(55, 105, "Town"));
            allTiles.Add(new KaiTile(55, 106, "Town"));
            allTiles.Add(new KaiTile(55, 107, "Town"));













        }

        private void GroupIfLocationChange()
        {
            if (Game1.locationRequest != null)
            {
                if (Game1.locationRequest.Location != Game1.currentLocation)
                {
                    Game1.currentLocation.map.BringLayerForward(Game1.currentLocation.map.GetLayer("Buildings"));


                    this.ThisLocationTiles.Clear();
                    Monitor.Log($"Grouping Tiles At: {Game1.locationRequest.Location}",LogLevel.Debug);

                    foreach (KaiTile t in allTiles)
                    {
                        if (t.tileIsWhere == Game1.locationRequest.Location.Name)
                        {
                            ThisLocationTiles.Add(t);
                        }
                    }
                }
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

            allTiles.Clear();
            ThisLocationTiles.Clear();




    }


        
        private void CalculateTileSum()
        {
            var tileCount = 50000;
            var price = 1.0;
            var priceIncrease = 0.0008;
            var totalCost = 0;

            for(int i = 0; i < tileCount; i++)
            {
                totalCost += (int)Math.Floor(price);
                price += priceIncrease;

            }
            this.Monitor.Log($"Cost of {tileCount} tiles by the end: {totalCost}", LogLevel.Debug);
        }

        private void SaveModData(object sender, SavedEventArgs e) {

            var tileData = new ModData
            {
                ToPlaceTiles = this.do_loop,
                TilePrice = this.tile_price,
                TilePriceRaise = this.tile_price_raise
            };

            if (allTiles != null)
            {
             tileData.AllKaiTiles = allTiles;
                    
            }

            //this.Helper.Data.WriteSaveData("example-key", tileData);
            Helper.Data.WriteJsonFile<ModData>("jsons/tilemanData.json", tileData);
        }

        private void LoadModData(object sender, SaveLoadedEventArgs e) {

            Monitor.Log("Mod Data Loaded",LogLevel.Debug);


            var tileData = this.Helper.Data.ReadJsonFile<ModData>("jsons/tilemanData.json") ?? new ModData();
            //var tileData = this.Helper.Data.ReadSaveData<ModData>("example-key");

            do_loop = true;
            this.do_loop = tileData.ToPlaceTiles;
            this.tile_price = tileData.TilePrice;
            this.tile_price_raise = tileData.TilePriceRaise;

            this.allTiles = tileData.AllKaiTiles;



        }






    }


}
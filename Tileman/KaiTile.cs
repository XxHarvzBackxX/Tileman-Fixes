using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;


namespace Tileman
{
    public readonly struct KaiTile 
    {

        public readonly int tileX;
        public readonly int tileY;
        public readonly int tileW;
        public readonly int tileH;


        public readonly string tileIsWhere;
        public KaiTile(int TileX, int TileY, string TileIsWhere)
        {
            tileX = (TileX >= 0) ? TileX : 0;
            tileY = (TileY >= 0) ? TileY : 0;
            tileIsWhere = TileIsWhere;
            tileW = tileH = 64;
        }


        public void DrawTile(Texture2D texture, SpriteBatch spriteBatch) 
        {

                float offsetX = Game1.viewport.X;
                float offsetY = Game1.viewport.Y;


                spriteBatch.Draw(texture,
                    Utility.getRectangleCenteredAt(new Vector2((this.tileX + 1) * 64 - offsetX - 32, (this.tileY + 1) * 64 - offsetY - 32), 64),
                    Color.White);



        }

        

        public bool IsSpecifiedTile(int TileX, int TileY, string TileIsWhere) {
            if (TileX == tileX && TileY == tileY && TileIsWhere == tileIsWhere) return true;
            return false;
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tileman
{
    class ModData

    {
        public bool ToPlaceTiles { get; set; } = true;
        public double TilePrice { get; set; }

        public double TilePriceRaise { get; set; }


        public List<KaiTile> AllKaiTiles { get; set; } = new();//new KaiTile(30,20, StardewValley.Game1.getLocationFromName("Farm") ) );
    }
}

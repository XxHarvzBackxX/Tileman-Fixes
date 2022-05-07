using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tileman
{
    class MapData

    {
        public Dictionary<string, List<KaiTile> > AllKaiTilesDict { get; set; } = new();//new KaiTile(30,20, StardewValley.Game1.getLocationFromName("Farm") ) );
    }
}

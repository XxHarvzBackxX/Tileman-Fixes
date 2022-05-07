using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tileman
{
    class ModData

    {
        public bool ToPlaceTiles { get; set; }

        public bool DoCollision { get; set; }   
        public bool ToggleOverlay { get; set; }
        public double TilePrice { get; set; }
        public double TilePriceRaise { get; set; }

        public int CavernsExtra { get; set; }

    }
}

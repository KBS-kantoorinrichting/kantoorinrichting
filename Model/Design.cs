using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Design
    {
        public int DesignId { get; set; }
        
        public int SpaceId { get; set; }
        public Space Space { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<ProductPlacement> ProductPlacements { get; set; }
    }
}

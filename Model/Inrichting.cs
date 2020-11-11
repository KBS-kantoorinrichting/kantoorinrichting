using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Inrichting
    {
        public int InrichtingId { get; set; }
        
        public int RuimteId { get; set; }
        public Ruimte Ruimte { get; set; }

        public List<ProductPlaatsing> ProductPlaatsingen = new List<ProductPlaatsing>();
    }
}

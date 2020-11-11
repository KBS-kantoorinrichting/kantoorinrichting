using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Naam { get; set; }
        #nullable enable
        public double? Prijs { get; set; }
        public string? Foto { get; set; }
        #nullable disable
    }
}

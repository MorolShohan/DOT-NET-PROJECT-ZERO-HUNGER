using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zero_Hunger.EF;

namespace Zero_Hunger.Models
{
    public class ProductWithQuntity
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public string RestaurantName { get; set; }
    }
}
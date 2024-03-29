﻿using System;
using System.Collections.Generic;

namespace VideoStore.OrderingFunction.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Movies { get; set; }
        //public List<string> Movies { get; set; } = new List<string>(); // JSON serialization is not working currently with sql server trigger
    }
}

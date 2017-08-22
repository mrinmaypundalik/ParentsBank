using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParentsBank.Models
{
    public class WishList
    {

        public int id { get; set; }

        public WishList()
            {
            DateAdded = DateTime.Now;
            Purchased = false;
                }
            
        [Required]
        public int Cost { get; set; }

        [Required]
        public string Description { get; set; }

        [Url]
        [Required]
        public string Link { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }

        public bool Purchased { get; set; }

        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public virtual Account Account { get; set; }
    }
}
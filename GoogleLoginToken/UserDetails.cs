using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleLoginToken
{
    public class UserDetails
    {
        [Key]
        public int UserId { get; set; }
        public User User { get; set; }
        public string Detalles { get; set; }


    }
}

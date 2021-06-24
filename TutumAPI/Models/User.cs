using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TutumAPI.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        [Phone]
        public string Phone { get; set; }

        [JsonIgnore]
        public virtual Subscription Subscription { get; set; }

        [NotMapped]
        public bool HasSubscription { get => Subscription != null; }
    }
}

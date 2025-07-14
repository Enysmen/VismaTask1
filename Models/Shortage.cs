using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VismaTask.Models
{
    public class Shortage
    {
        // Uniquely identifies the application along with the Room
        [Required]
        public string Title { get; set; }

        [Required]
        // Name of the user who created the request
        public string Name { get; set; }

        [Required]
        // Room: one of the fixed values
        public Room Room { get; set; }

        [Required]
        // Category: one of the fixed values
        public Category Category { get; set; }

        [Required]
        // Priority from 1 (low) to 10 (high)
        public int Priority { get; set; }


        // Time of request creation, filled in automatically
        public DateTime CreatedOn { get; set; }
    }


    // Possible rooms for application
    public enum Room
    {
        MeetingRoom,
        Kitchen,
        Bathroom
    }

    // Possible categories for the application
    public enum Category
    {
        Electronics,
        Food,
        Other
    }
}


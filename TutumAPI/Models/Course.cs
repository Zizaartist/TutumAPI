using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable

namespace TutumAPI.Models
{
    public partial class Course
    {
        public Course()
        {
            Lessons = new HashSet<Lesson>();
        }

        public int CourseId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string PreviewPath { get; set; }

        public bool IsPremiumOnly { get; set; }

        [JsonIgnore]
        public virtual ICollection<Lesson> Lessons { get; set; }
    }
}

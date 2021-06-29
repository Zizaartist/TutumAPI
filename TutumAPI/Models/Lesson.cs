using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace TutumAPI.Models
{
    public partial class Lesson
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string VideoPath { get; set; }
        public string PreviewPath { get; set; }

        [JsonIgnore]
        public virtual Course Course { get; set; }

        public bool ShouldSerializeCourseId() => false;
        public bool ShouldSerializeText() => ShowAllData;
        public bool ShouldSerializeVideoPath() => ShowAllData;

        [JsonIgnore]
        [NotMapped]
        public bool ShowAllData = false;
    }
}

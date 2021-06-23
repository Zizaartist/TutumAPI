using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace TutumAPI.Models
{
    public partial class Lesson
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Название")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Текст")]
        [MaxLength(1000)]
        public string Text { get; set; }

        [Required]
        [Display(Name = "Видео файл")]
        [MaxLength(50)]
        public string VideoPath { get; set; }

        public string PreviewPath { get; set; }

        [JsonIgnore]
        public virtual Course Course { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        [MaxLength(100)]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Картинка")]
        public string PreviewPath { get; set; }

        [Required]
        [Display(Name = "Премиум курс")]
        public bool IsPremiumOnly { get; set; }

        [JsonIgnore]
        public virtual ICollection<Lesson> Lessons { get; set; }

        [Required]
        [Display(Name = "Изображение")]
        [NotMapped]
        public IFormFile PreviewFile { get; set; }

        //Только десериализация
        //public bool ShouldSerializePreviewFile() => false;
    }
}

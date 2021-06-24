using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TutumAPI.Models
{
    public class VideoViewModel
    {
        [Display(Name = "Имя файла")]
        public string FileName { get; set; }
        [Display(Name = "Превью")]
        public string PreviewPath { get; set; }
        [Display(Name = "Видео файл")]
        public string VideoPath { get; set; }
    }
}

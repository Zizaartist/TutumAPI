using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TutumAPI.Models;

namespace TutumAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class LessonsController : Controller
    {
        private readonly DatabaseContext _context;

        public LessonsController(DatabaseContext context) 
        {
            _context = context;
        }

        // GET: api/Lessons/ByCourse/3
        [Route("ByCourse/{id}")]
        [HttpGet]
        public ActionResult<IEnumerable<Lesson>> GetLessonsByCourse(int id)
        {
            var myId = int.Parse(User.Identity.Name);

            //Если модель подписки еще не удалена - премиум активен
            var doIHaveSubscription = _context.Users.Include(user => user.Subscription)
                                                    .FirstOrDefault(user => user.UserId == myId).Subscription != null;

            var lessons = _context.Lessons.Where(lesson => lesson.CourseId == id && (!lesson.Course.IsPremiumOnly || doIHaveSubscription));

            if (!lessons.Any()) 
            {
                return NotFound();
            }

            var result = lessons.ToList();

            return result;
        }

        // GET: api/Lessons/3
        [HttpGet("{id}")]
        public ActionResult<Lesson> GetLesson(int id)
        {
            var myId = int.Parse(User.Identity.Name);

            //Если модель подписки еще не удалена - премиум активен
            var doIHaveSubscription = _context.Users.Include(user => user.Subscription)
                                                    .FirstOrDefault(user => user.UserId == myId).Subscription != null;

            var lesson = _context.Lessons.FirstOrDefault(lesson => lesson.LessonId == id && (!lesson.Course.IsPremiumOnly || doIHaveSubscription));

            if (lesson == null)
            {
                return NotFound();
            }

            lesson.ShowAllData = true;

            return lesson;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class EFGroupRepository:IGroupsRepository
    {
        private ApplicationContext context;
        private UserManager<User> Manager;
        public EFGroupRepository(ApplicationContext cnt,UserManager<User> manager)
        {
            Manager = manager;
            context = cnt;
        }

        public IQueryable<Group> Groups => context.Groups;

        public IQueryable<Lecture> Lectures => context.Lectures.Include(x=>x.Visits);

        public IQueryable<Visit> Visits => context.Visits;

        public void DeleteGroup(Group group)
        {
            foreach (var item in context.Lectures.Where(x => x.GroupID == group.GroupID))
            {
                context.Visits.RemoveRange(context.Visits.Where(x => x.LectureID == item.LectureID));
            }
            context.RemoveRange(context.Lectures.Where(x => x.GroupID == group.GroupID));
            foreach (var page in context.Documentation.Where(x => x.GroupID == group.GroupID))
            {
                foreach (var task in context.Tasks.Where(x=>x.DocPageID==page.DocPageID))
                {
                    context.RemoveRange(context.Answers.Where(x=>x.StTaskID==task.StTaskID));
                    context.RemoveRange(context.TaskFiles.Where(x => x.StTaskId == task.StTaskID));
                    context.Remove(task);
                }
                context.Remove(page);
            }
           
            foreach (User user in context.Students.Where(x=>x.GroupID==group.GroupID).ToList())
            {
                Manager.DeleteAsync(user).Wait();
            }
            
            context.Groups.Remove(group);
            context.SaveChanges();
        }

        public Group GetGroupById(int id)
        {
            return Groups.FirstOrDefault(x=>x.GroupID==id);
        }

        public Group GetGroupByName(string Name)
        {
            return Groups.FirstOrDefault(xx=>xx.GroupName==Name);
        }

      public void AddLecture(Group group, Lecture lecture)
        {
            lecture.GroupID = group.GroupID;
            context.Add(lecture);
            context.SaveChanges();
        }
        public void DeleteLecture(Lecture lecture)
        {
           
                context.Visits.RemoveRange(context.Visits.Where(x=>x.LectureID==lecture.LectureID));
            context.Lectures.Remove(lecture);
            context.SaveChanges();
            
        }

        public void SaveGroup(Group group)
        {
            
            if (group.GroupID == 0)
            {
                context.Groups.Add(group);
            }
            
            context.SaveChanges();
        }

        public Lecture GetLectureById(int id)
        {
            return context.Lectures.FirstOrDefault(x=>x.LectureID==id);
        }

        public Lecture GetLectureByDate(DateTime date)
        {
            return context.Lectures.FirstOrDefault(x => x.Date.Year==date.Year&&x.Date.Month==date.Month&&x.Date.Day==date.Day);
        }

        public void AddVisitor(Lecture lect, string userId)
        {
            var visit = new Visit { LectureID = lect.LectureID, VisitorID = userId };
            context.Add(visit);
            context.SaveChanges();
        }

        public void DeletVisit(Visit visit)
        {
            context.Remove(visit);
            context.SaveChanges();
        }
    }
}

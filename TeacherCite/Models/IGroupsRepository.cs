using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public interface IGroupsRepository
    {
        IQueryable<Group> Groups { get; }
         void SaveGroup(Group group);
       void DeleteGroup(Group group);
        void AddLecture(Group group,Lecture lecture);
        void DeleteLecture(Lecture lecture);
        Group GetGroupById(int id);
        Group GetGroupByName(string Name);
        IQueryable<Lecture> Lectures { get; }
        Lecture GetLectureById(int id);
        Lecture GetLectureByDate(DateTime date);
        void AddVisitor(Lecture lect, string username);
        void DeletVisit(Visit visit);
        IQueryable<Visit> Visits { get; }
    }
  
}

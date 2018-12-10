---
description: >-
  У версії 5 демонструється реалізація компонентів бізнес логіки, а саме груп
  студентів та лекцій для груп з відмічанням студентів.
---

# Версія 5. Групи студентів, лекції.

## Бізнес-логіка

### Групи студентів

Група студентів - вибірка студентів які мають спільні лекції, сторінки з документацією та доступ до спільних ресурсів система\(студенти можуть переглядати акаунти один одного, завантажувати файли які прикріпленні до документації та бронювати собі завдання на сторінці документації\). Акаунт студента не може існувати без групи, тому для реєстрації студента вчитель переходить на сторінку потрібної групи і створює акаунт, після чого на вказану почту студента приходить лист з унікальним посиланням на реєстрацію.

Група зберігається в бд і має наступний вигляд:

```csharp
 public class Group
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int YearOfStudy { get; set; }
        public string Description { get; set; }
    }
```

### Лекції

Лекція - об'єкт в якому зберігається інформація про лекцію, а саме дату проведення, місце проведення, опис інформація про лекцію, primary key групи студентів для якої означено лекцію та колекцію відвідувань лекції студентами.

Код:

```csharp
public class Lecture
    {
        public int LectureID { get; set; }

        public DateTime Date { get; set; }

        public int GroupID { get; set; }

        public string Place { get; set; }

        public ICollection<Visit> Visits { get; set; }

        public string Description { get; set; }
    }
```

### Відвідування 

Для збереження відвідувань студентів означено наступний клас:

```csharp
public class Visit
    {
        public int VisitID { get; set; }
     
        public int LectureID { get; set; }

        public string VisitorID { get; set; }
       
    }
```

Тобто зберігаються лише айди лекції та студента який відвідав лекцію.

## Реалізація патерну Repository

Маємо наступний інтерфейс.

```csharp
 public interface IGroupsRepository
    {
        IQueryable<Group> Groups { get; }
        
        void SaveGroup(Group group);
         
        void DeleteGroup(Group group);
       
        IQueryable<Lecture> Lectures { get; }
       
        void AddLecture(Group group,Lecture lecture);
        
        void DeleteLecture(Lecture lecture);
        
        Group GetGroupById(int id);
        
        Group GetGroupByName(string Name);
        
        Lecture GetLectureById(int id);
        
        Lecture GetLectureByDate(DateTime date);
        
        IQueryable<Visit> Visits { get; }
        
        void AddVisitor(Lecture lect, string username);
        
        void DeletVisit(Visit visit);
        
        
    }
```

Даний інтерфейс імплементує колекції Groups, Lectures, Visits та основні методи для роботи з даними. 

Реалізація цього інтерфейсу з використанням Entity Framework core 2 наступна:

```csharp
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
```

Дана реалізація знімає з нас відповідальність за роботою з самим EF core та базою даних роблячи наш код, який відноситься до бізнес-логіки більш зрозумілим.

## Реалізація бізнес логіки

### GroupController

Даний контролер обробляє запити акаунтів з рол'ю вчитель за маршрутом Teacher/Group/{Action}, і має наступну реалізацію.

```csharp
  [Area("Teacher")]
    [Authorize(Roles ="Teacher")]
    public class GroupController : Controller
    {
        private IGroupsRepository repository;
        private ApplicationContext context;
        public GroupController(IGroupsRepository groups, ApplicationContext ctx)
        {
            context= ctx;
            repository = groups;
        }
        [HttpGet]
        public IActionResult Add(string returnUrl)
        {
            var model = new AddGroupViewModel() {returnUrl=returnUrl };
            return View(model);
        }
        [HttpPost]
        public IActionResult Add(AddGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool check = repository.Groups.Any(x=>x.GroupName==model.GroupName);
                if (check)
                {
                    ModelState.AddModelError("",$"Group with name {model.GroupName} is alredy exist.");
                }
                else
                {
                    Group group= new Group {GroupName=model.GroupName,Description=model.Description,YearOfStudy=model.YearOfStudy };

                    repository.SaveGroup(group);
                    return RedirectToAction(nameof(List));
                }

            }
            return View(model);
        }
        public IActionResult List()
        {
            return View(repository.Groups.ToList()??new List<Group>());
        }
      
        public IActionResult ShowGroup(int GroupID=1,string returnUrl="/")
        {
            var model = new ShowGroupViewModel { Users = context.Students.Where(x => x.GroupID == GroupID).ToList<IUser>()??new List<IUser>(),
                Lectures = repository.Lectures.Where(x => x.GroupID == GroupID).ToList()??new List<Lecture>(), returnUrl = returnUrl
                , Group = repository.GetGroupById(GroupID)
            };
            return View(model);
        }
        [HttpGet]
        public IActionResult AddLecture(string returnUrl,int GroupID)
        {
            ViewBag.GroupName = repository.Groups.FirstOrDefault(x => x.GroupID == GroupID).GroupName;
            var model = new AddLectureViewModel { GroupID = GroupID, returnUrl = returnUrl };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddLecture(AddLectureViewModel model)
        {
            var lecture = new Lecture() { Place=model.Place,Date=model.Date, Description=model.Description};
            repository.AddLecture(repository.GetGroupById(model.GroupID),lecture);
           return LocalRedirect(model.returnUrl);
        }
        public IActionResult RemoveLecture(int LectureID,string returnUrl)
        {
            repository.DeleteLecture(repository.GetLectureById(LectureID));
            return LocalRedirect(returnUrl);
        }
        public IActionResult ShowLecture(int LectureID,string returnUrl="/")
        {
            var groupid=repository.Lectures.First(x=>x.LectureID==LectureID).GroupID;
            List<StudentUser> users = context.Students.Where(x=>x.GroupID==groupid).ToList();
            List<ShowLectureViewModel> model = new List<ShowLectureViewModel>();
            foreach (var user in users)
            {
                ShowLectureViewModel showmodel = new ShowLectureViewModel
                {
                    Email = user.Email,
                    FirstName=user.FirstName,
                    LastName=user.LastName,
                    Username=user.UserName,
                    LectureID=LectureID,
                    Number=user.NumberOfStudentBook,
                    Was=repository.Visits.Any(x=>x.VisitorID==user.Id&&x.LectureID==LectureID)
                
                };
                model.Add(showmodel);
            }
            model.OrderBy(x=>x.Number);

            var ShowModel = new ShowLectureList() {list=model,returnUrl=returnUrl };


            return View(ShowModel);
        }

        public IActionResult RemoveGroup(int GroupId)
        {
            var group = repository.GetGroupById(GroupId);
           
            repository.DeleteGroup(group);
            return RedirectToAction("List");
        }
      
    }
```

Даний контролер бере на себе відповідальність за: 

* Додавання нових груп
* Видалення груп з усіма залежностями\( студенти, студенти документація, завдання студентів\)
* Додавання лекцій
* Видалення лекцій
* Список груп
* Демонстрацію конкретної групи студентів з усіма залежностями

Тобто GroupController надає можливість вчителю редагувати бд. Всі методи в результаті повертають html розмітку, про механізми її генерації йдеться в іншій частині звіту.

### API метод для фіксації відвідування студентом лекції

Так як у вимогах було вказано про можливе впровадження механізмів відмічання на лекції студентів асистентами вчителя або іншим програмним забезпеченням, була створена додаткова зона доступу до ресурсів додатку, а саме зона Admin в якій будуть створені механізми для взаємодії користувачів з рол'ю Admin.

Маємо наступний контролер:

```csharp
[Area("Admin")]
    [Authorize(Roles ="Teacher, Admin")]
    public class StudentsController : Controller
    {
        IGroupsRepository repository;
        UserManager<User> manager;
        public StudentsController(IGroupsRepository repo,UserManager<User> mng)
        {
            repository = repo;
            manager = mng;
        }
        
        [HttpPost]
        public async Task<IActionResult> ChekVisit(string UserName,int LectureID)
        {
            var user = await manager.FindByNameAsync(UserName);
            var visit = repository.Visits.FirstOrDefault(
                x => x.LectureID == LectureID && x.VisitorID == user.Id
                );
            var lection = repository.Lectures.FirstOrDefault(x => x.LectureID == LectureID);
            if (visit==null)
            {
                repository.AddVisitor(lection, user.Id);
            }
            else {
                repository.DeletVisit(visit);
            }
            return Ok();
        }
    }
```

CheckVisit є API методом до якого мають доступ користувачі з акаунтом вчителя, та адміністратора. Метод приймає інформацію про студента та лекцію і додає Visit у базу даних або видаляє його якщо Visit вже є у базі даних, в результаті коректної роботи метода повертається статус код 200.

## Підсумок 

У звіті було показано яким чином реалізовано логіку ведення груп студентів, лекцій та відвідувань. Введено нову зону доступу до ресурсів системи. Показано на прикладі методів GroupController з області Teacher, взаємодія з ресурсами системи на програмному рівні.


---
description: >-
  У версії 6 демонструється реалізація компонентів бізнес логіки, а саме
  сторінок документації та задач студентів.
---

# Версія 6. Документація, задачі студентів.

## Бізнес логіка

### Сторінка документації

Сторінки документації - редагуєма вчителем текстова інформація до якої можна прикріпити файли та завдання для студентів. Існує два види сторінок, а саме публічні та приватні для заданої групи студентів. В публічних сторінках не має можливості розмістити завдання студентів і такі сторінки відображаються усім студентам. Приватні сторінки містять завдання студентів і відображаються лише студентам заданої групи. На сторінці приватної документації студент може обрати собі одне завдання із списку незайнятих завдань. Також вчитель може дати всім студентам групи однакове завдання. Текстова інформація редагується вчителем з підтримкою мов розмітки html та markdown. Для безпечного генерування контенту сторінок використовується sanitizer, функцією якого є перевірка та редагування текстових даних html на заборонені елементи такі як теги html script style або атрибути. Для стилізації змісту дозволяється використовувати тег class, в який можна додати класи клієнтських бібліотек про які йдеться в іншій частині звіту.

Код класу сторінки документації.

```csharp
 public class DocPage
    {
        public string Data { get; set; }

        public int GroupID { get; set; }

        public string Name { get; set; }

        public int DocPageID { get; set; }

        public DateTime Date { get; set; }
    }
```

В такому вигляді сторінка зберігається у бд. Якщо GroupID == 0 , то сторінка вважається публічною.

### Задачі 

Задачі - об'єкти з інформацією про завдання, які прив'язані до студента \( \* задач - 1 студент\). Вчитель розміщує умови завдання на сторінках документації, де студенти обирають собі завдання\( Студент не може мати більше 1-го завдання від 1 сторінки документації\). Існує сценарій при якому вчитель одразу прив'язує копію однакової задачі до всіх студентів групи сторінки документації. Студент може прикріпити файл до свого завдання. Студент і вчитель може прикріпити відповідь на завдання.

Код задачі.

```csharp
public class StTask
    {
        public int StTaskID { get; set; }

        public int DocPageID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UserID { get; set; }

        public int MaxCost { get; set; }

        public bool IsChecked { get; set; }

        public DateTime DeadLine { get; set; }

        public int CurrentCost { get; set; }
    }
```

CurrentCost це поточний бал за завдання, а MaxCost - максимально можливий бал.

Код відповіді на завдання.

```csharp
  public class Answer
    {
        public int StTaskID { get; set; }

        public string UserID { get; set; }

        public string TextData { get; set; }

        public int AnswerID { get; set; }

        public DateTime AnswerDate { get; set; }
    }
```

Answer використовується для комунікації вчителя та студенту та збереження інформації про зміну стану завдання.

## Реалізація патерну Repository

Маємо наступний інтерфейс:

```csharp
 public interface IDocStorage
    {
        IQueryable<DocPage> Pages { get; }
        
        IQueryable<StTask> Tasks { get; }
        
        IQueryable<Answer> Answers { get; }
        
        void AddTask(StTask task);
        
        void RemoveTask(StTask task);
        
        void AddPage(DocPage page);
        
        void RemovePage(DocPage page);
        
        void AddAnswer(Answer answer);
        
        void RemoveAnswer(Answer answer);
        
        void UpdatePage(DocPage page);
        
        void UpdateTask(StTask task);
        
        IList<StTask> GetStudentTasks(string UserID);

        IList<DocPage> GetGroupDocs(int GroupID);
    }
```

Інтерфейс описує основну функціональність, яка нам знадобиться при роботі з даними.

Для роботи з базою даних інструментами Entity Framework Core 2 маємо наступну реалізацію інтерфейсу.

```csharp
public class DocPagesRepository:IDocStorage
    {
        ApplicationContext context;
        public DocPagesRepository(ApplicationContext ctx)
        {
            context = ctx;
        }

        public IQueryable<DocPage> Pages => context.Documentation;

        public IQueryable<StTask> Tasks => context.Tasks;

        public IQueryable<Answer> Answers => context.Answers;

        public void AddAnswer(Answer answer)
        {
            context.Add(answer);
            context.SaveChanges();
        }

        public void AddPage(DocPage page)
        {
            context.Add(page);
            context.SaveChanges();
        }

        public void AddTask(StTask task)
        {
            context.Add(task);
            context.SaveChanges();
        }

        public IList<DocPage> GetGroupDocs(int GroupID)
        {
            return context.Documentation.Where(x=>x.GroupID==GroupID).ToList();
        }

        public IList<StTask> GetStudentTasks(string UserID)
        {
            return context.Tasks.Where(x=>x.UserID==UserID).ToList();
        }

        public void RemoveAnswer(Answer answer)
        {
            context.Remove(answer);
            context.SaveChanges();
        }

        public void RemovePage(DocPage page)
        {
            
            context.Remove(page);
          context.SaveChanges();
        }

        public void RemoveTask(StTask task)
        {
            
            context.RemoveRange(context.Answers.Where(x => x.StTaskID == task.StTaskID));
            context.RemoveRange(context.TaskFiles.Where(x => x.StTaskId == task.StTaskID));
            context.Remove(task);
            context.SaveChanges();
        }

        public void UpdatePage(DocPage page)
        {

            if (page.DocPageID == 0)
            {
                context.Add(page);
            }
            context.SaveChanges(); ;
        }

        public void UpdateTask(StTask task)
        {

            if (task.StTaskID == 0)
            {
                context.Add(task);
            }
            context.SaveChanges();
        }
    }
```



## Реалізація бізнес логіки

### DocumentationController

Для адміністрування вчителем стану сторінок документації маємо наступний controller.

```csharp
[Area("Teacher")]
    [Authorize(Roles="Teacher")]
    public class DocumentationController : Controller
    {
        IDocStorage Documentation;
        IFileStorage Files;
        IGroupsRepository Repository;
        IUserRepository Users;
        IHostingEnvironment environment;
        public DocumentationController(IDocStorage docs,IFileStorage files,IGroupsRepository groups,IUserRepository users, IHostingEnvironment env)
        {
            Documentation = docs;
            Files = files;
            Repository = groups;
            Users = users;
            environment = env;
        }
        public IActionResult RemovePage(string returnUrl, int PageID)
        {
            Documentation.RemovePage(Documentation.Pages.FirstOrDefault(x=>x.DocPageID==PageID));
            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult AddMarkdownToPage(string returnUrl,int PageID)
        {
            var page = Documentation
                .Pages.FirstOrDefault(x => x.DocPageID == PageID);
            var model = new MarkdownViewModel {
                returnUrl =returnUrl,
                MarkdownData = page.Data,
                PageID=PageID,
                PageName=page.Name
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddMarkdownToPage(MarkdownViewModel model)
        {
            //if (model.MarkdownData?.Contains("<script>")==true)
            //{
            //    ModelState.AddModelError("","Unsafe tag <script>...</script>");
            //}
            if (ModelState.IsValid)
            {
              var page=Documentation.Pages.FirstOrDefault(x => x.DocPageID == model.PageID);
                page.Data = model.MarkdownData;
                page.Name = model.PageName;
                Documentation.UpdatePage(page);
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
        public IActionResult ShowPage(int PageID,string returnUrl)
        {
           
            var docpage = Documentation.Pages.FirstOrDefault(x => x.DocPageID == PageID);
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add( "class");
            if (docpage.GroupID==0)
            {
               
           
               
                var model = new ShowPublicPageViewModel
                {
                    Name=docpage.Name,
                    returnUrl=returnUrl,
                    Files =Files.DocumentationFiles.Where(x=>x.DocPageId==PageID).ToList(),
                    DocPageID = PageID,
                    HtmlDataText= sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(docpage.Data?? @"### No content ")),
                    Date=docpage.Date.ToShortDateString()
                };
                return View("ShowPublicDocPage", model);
            }
            else {
                var List = new List<TaskModel>();
                foreach (var item in Documentation.Tasks.Where(x => x.DocPageID == PageID).ToList())
                {
                    List.Add(new TaskModel() {Task=item,User=Users.UsersData.FirstOrDefault(x=>x.Id==item.UserID) });
                }
                var model = new ShowPrivatePageViewModel {
                    Name = docpage.Name,
                    returnUrl = returnUrl,
                    Files = Files.DocumentationFiles.Where(x => x.DocPageId == PageID).ToList(),
                    DocPageID = PageID,
                    HtmlDataText = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(docpage.Data ?? @"### No content ")),
                    Date = docpage.Date.ToShortDateString(),
                    GroupID = docpage.GroupID,
                    Tasks =List
            };
            return View("ShowPrivateDocPage",model);
            }
        }
        [HttpGet]
        public IActionResult AddDocPage(string returnUrl,int GroupID)
        {
            var model =new AddDocPageViewModel{returnUrl=returnUrl,GroupID=GroupID,GroupName=Repository.Groups.FirstOrDefault(x=>x.GroupID==GroupID)?.GroupName??"Public" };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddDocPage(AddDocPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                Documentation.AddPage(new DocPage {GroupID=model.GroupID,Name=model.Name,Date=model.Date });
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
        public IActionResult List()
        {
            var model = new DocumentationsListViewModel {

                Pages = (Documentation.Pages.ToList() ?? new List<DocPage>()).OrderBy(x => x.Date).Reverse().ToList(),
                Groups = Repository.Groups.ToList()??new List<Group>()
            };
           
            return View(model);
        }

        [HttpGet]
        public IActionResult AddDocFile(string returnUrl,int PageID)
        {
            var model = new AddDocFileViewModel {ReturnUrl=returnUrl,PageID=PageID };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddDocFile(AddDocFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Files.AllFiles.Any(x => x.FileName == model.File.FileName))
                {
                    ModelState.AddModelError("", "File with name " + model.File.FileName + " is alredy exist.");
                    return View(model);
                }

                var directory = Directory.GetFiles(environment.ContentRootPath + @"/wwwroot/FileExtansions");

                var ext = new String(model.File.FileName.TakeLast(model.File.FileName.Length - (model.File.FileName.LastIndexOf('.') == -1 ? model.File.FileName.Length : model.File.FileName.LastIndexOf('.')) - 1).ToArray());
                if (!directory.Any(x => x==("File"+ext+".png")))
                {
                    ext = "";
                }
                var File = new DocFile()
                {
                    FileName = model.File.FileName,
                    FileExtansion = ext,
                    DocPageId=model.PageID,
                    Description=model.Desription
                };
                using (var reader = new BinaryReader(model.File.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)model.File.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}", data.LongLength);
                    File.Value = data;
                }

                Files.AddFile(File);
                return LocalRedirect(model.ReturnUrl);
            }
            return View(model);
        } 
    }
```

Для відображення текстового змісту документації використовується наступні пакети NuGet:

* HtmlSanitizer \(4.0.187\)
* CommonMark.NET \(0.15.1\)

пакети необхідні для конвертації markdown-&gt;html та перевірки на безпечність розмітки.

### TaskController

Для адміністрування вчителем задач студентів використовується наступний controller:

```csharp
[Area("Teacher")]
    [Authorize(Roles = "Teacher")]
    public class TaskController : Controller
    {
        IDocStorage Documentation;
        IFileStorage Files;
        IGroupsRepository Repository;
        ApplicationContext Context;
        public TaskController(IDocStorage docs, IFileStorage files, IGroupsRepository groups, ApplicationContext ctx)
        {
            Documentation = docs;
            Files = files;
            Repository = groups;
            Context = ctx;
        }

        [HttpGet]
        public IActionResult AddTask(string returnUrl, int PageID)
        {
            var model = new AddTaskViewModel() { returnUrl = returnUrl, DocPageID = PageID };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddTask(AddTaskViewModel Model)
        {

            if (ModelState.IsValid)
            {
                if (Model.IsForAllStudents)
                {
                    var groupID = Documentation.Pages.FirstOrDefault(x => x.DocPageID == Model.DocPageID).GroupID;
                    var studentsFromThisGroup = Context.Students.Where(x => x.GroupID == groupID).ToList();
                    foreach (var student in studentsFromThisGroup)
                    {
                        var Task = new StTask
                        {
                            CurrentCost = 0,
                            DeadLine = Model.DeadLine,
                            Description = Model.Description,
                            IsChecked = true,
                            UserID = student.Id,
                            Name = Model.Name,
                            MaxCost = Model.MaxCost,
                            DocPageID = Model.DocPageID
                        };
                        Documentation.AddTask(Task);
                    }
                }
                else
                {
                    var Task = new StTask
                    {
                        CurrentCost = 0,
                        DeadLine = Model.DeadLine,
                        Description = Model.Description,
                        IsChecked = true,
                        Name = Model.Name,
                        MaxCost = Model.MaxCost,
                        DocPageID = Model.DocPageID
                    };
                    Documentation.AddTask(Task);
                }
                return LocalRedirect(Model.returnUrl);
            }
            return View(Model);
        }
        public IActionResult List()
        {
            return View(Documentation.Tasks);
        }
        public IActionResult UnChecked()
        {
            return Json(Documentation.Tasks.Where(x=>!x.IsChecked).Count());
        }
        public IActionResult RemoveTask(string returnUrl, int StTaskID)
        {
            Documentation.RemoveTask(Documentation.Tasks.FirstOrDefault(x => x.StTaskID == StTaskID));
            return LocalRedirect(returnUrl);
        }

        public IActionResult ShowTask(string returnUrl, int StTaskID)
        {
            var task = Context.Tasks.FirstOrDefault(x => x.StTaskID == StTaskID);
            if (task != null)
            {


                var answers = Documentation.Answers.Where(x => x.StTaskID == StTaskID);
                var AnswerViewModels = new List<AnswerViewModel>();
                var FileViewModels = new List<StTaskFileData>();
                foreach (var item in answers)
                {
                    var user = Context.Users.FirstOrDefault(x => x.Id == item.UserID);
                    AnswerViewModels.Add(new AnswerViewModel
                    {
                        Answer = item,
                        User = user,
                        IsTeacherAnswer = Context.UserRoles.Any(x => x.UserId == user.Id & Context.Roles.FirstOrDefault(y => x.RoleId == y.Id).Name == "Teacher")
                    });
                }
                foreach (var item in Files.StudentTaskFiles.Where(x => x.StTaskId == task.StTaskID).ToList())
                {
                    FileViewModels.Add(new StTaskFileData(item.FileName,item.FileExtansion,item.FileSize,item.Description));
                }
                var model = new ShowTaskViewModel()
                {
                    returnUrl = returnUrl,
                    Answers = AnswerViewModels.OrderBy(x=>x.Answer.AnswerDate).Reverse().ToList(),
                    User = Context.Users.FirstOrDefault(x => x.Id == task.UserID),
                    Task = task,
                    Files = FileViewModels
                };
                return View(model);
            }
            return LocalRedirect(returnUrl);
        }
        [HttpGet]
        public IActionResult Rate(string returnUrl,int TaskID)
        {
            var task = Documentation.Tasks.FirstOrDefault(x=>x.StTaskID==TaskID);
            if (task!=null)
            {

            var model = new RateTaskViewModel {
                returnUrl=returnUrl,
                TaskID=TaskID,
                CurrentRate=task.CurrentCost,
                MaxRate=task.MaxCost,
                
            };
            return View(model);

            }
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public IActionResult Rate(RateTaskViewModel model)
        {
            var task = Documentation.Tasks.FirstOrDefault(x => x.StTaskID == model.TaskID);

            if (task.MaxCost<model.CurrentRate)
            {
                ModelState.AddModelError("","Rate cannot be more than "+ model.MaxRate);
            }
            if (ModelState.IsValid)
            {
                 var oldrate = task.CurrentCost;
                task.CurrentCost = model.CurrentRate;
                task.IsChecked = true;
                Documentation.UpdateTask(task);
                var answer = new Answer() {
                    AnswerDate = DateTime.Now,
                    StTaskID = task.StTaskID,
                    TextData = model.Description,
                    UserID=Context.Users.FirstOrDefault(x=>x.UserName==User.Identity.Name).Id

                };
                Documentation.AddAnswer(answer);
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
    }
    
    
```

### Проблема з потоконебезпечністю.

Для студента реалізація бізнес логіки аналогічна і її можна подивитися на github. Слід зазначити лише обирання задач студентом. Так, як додаток працює асинхронно, а не в реальному часі, існує сценарій за яким студенти обирають один і той самий таск одночасно. Проблема в тому щоб в базу не поступали одночасно два запити на зміну завдання. 

Код:

```csharp
 static object TaskLocker = new object();
        public async Task<IActionResult> ChooseTask(int StTaskID, string returnUrl)
        {
            var user = await Manager.FindByNameAsync(User.Identity.Name);
            lock (TaskLocker)
            {
              var task=  Docs.Tasks.FirstOrDefault(x=>x.StTaskID==StTaskID&&x.UserID==null);
                
                if (task!=null)
                {
                    var isNoElseTasksForThisUser = Docs.Tasks
                        .FirstOrDefault(x => x.UserID == user.Id && x.DocPageID == task.DocPageID)==null;
                    var isDocPageFromThisGroup = Docs.Pages
                        .FirstOrDefault(x => x.DocPageID == task.DocPageID).GroupID == (user as StudentUser).GroupID;
                    if (isNoElseTasksForThisUser&&isDocPageFromThisGroup)
                    {
                        task.UserID = user.Id;
                        Docs.UpdateTask(task);
                    }
                    else
                    {
                        return View("TaskError", returnUrl);
                    }
                   
                }else
                {
                    return View("TaskError",returnUrl);
                }
            return LocalRedirect(returnUrl);
            }
```

Використовується блокування контексту для того щоб бути впевненим в тому що при одночасному запиту на вибір однакового таску не виникло помилок.

## Підсумок

Було показано як працює реалізація бізнес логіки документації вчителя та завдань студентів. Вирішена проблема потоконебезпечності контексту бази даних. Продемонстровані приклади роботи з пакетами NuGet для генерації html та перевірки його вмісту.


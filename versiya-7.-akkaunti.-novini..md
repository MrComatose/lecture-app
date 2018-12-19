---
description: >-
  В версії сім була реалізована можливість користувачами редагувати інформацію
  свого акаунту та публікація новин на головній сторінці вчителем.
---

# Версія 7. Аккаунти. Новини.

## Бізнес логіка

### Акаунти

Для реєстрації студентів вчитель повинен ввести пошту, им'я та прізвище, номер заліковки. Для надання доступу до акаунту генерується токен підтвердження пошти та генерується посилання для реєстрації, де студент проходить реєстрацію.

Для зміни паролю акаунту існує функція яка генерує посилання з токеном збросу пароля і відправляє його на пошту акаунту.

Студентам надається можливість редагувати всі поля акаунту окрім пошти, імені та прізвища та номера залікової книги. Вчителю надається можливість редагувати усі аканти і усі дані акаунтів, видаляти акаунти з бд.

Для оптимізації сайту аватари завантажені користувачем стискаються на стороні клієнта до розміру 256 на 256 пікселів максимум.

### Новини

Окрім документації користувача також існує інша можливість донесення інформації з боку вчителя до користувачів - новини. Новини зберігаються в бд і виводяться на головну сторінку, а саме останні шість новин.

Код новини:

```csharp
public class News
    {
        public int NewsID { get; set; }

        public string Name { get; set; }

        public string TextData { get; set; }

        public DateTime PublicationDate { get; set; }
    }
```

Також для новин існує реалізація патерну Repository.

```csharp
 public interface INewsStorage
    {
         void AddNews(News value);
        void DeleteNews(News value);
        IEnumerable<News> GetSixLatestNews();
        IQueryable<News> AllNews { get; }
    }
```

```csharp
 public class NewsRepository : INewsStorage
    {
        ApplicationContext Context;
        public NewsRepository(ApplicationContext ctx)
        {
            Context = ctx;
        }
        public IQueryable<News> AllNews => Context.News;

        public void AddNews(News value)
        {
            Context.Add(value);
            Context.SaveChanges();
        }

        public void DeleteNews(News value)
        {
            Context.Remove(value);
            Context.SaveChanges();
        }

        public IEnumerable<News> GetSixLatestNews()
        {
            return Context.News.AsNoTracking().OrderByDescending(x => x.PublicationDate).Take(6);
        }
    }
```

Також для новини існує можливість додати фото, яке буде також стиснуто на стороні клієнта та зберігатиметься у таблиці з файлами.

## Реалізація

### сервіс MailSender

Для реєстрації акаунту студента використовується сервіс для відправки пошти. Для того щоб динамічно генерувати html зміст листа використовується RazorEngine, а саме імітується робота фреймворку MVC для того щоб використати шаблони .cshtml. Для відправки листа я написав наступний сервіс:

```csharp
 public class AppMailSender
    {
        string smtpServer, login, password;
        public SmtpClient smtp { get; set; }
        IHostingEnvironment environment;
        RazorViewToStringRenderer Render;
        public AppMailSender(IConfiguration config, IHostingEnvironment env,RazorViewToStringRenderer renderer)
        {
            environment = env;
            smtpServer = config["Data:EmailSettings:SmtpServer"];
            login = config["Data:EmailSettings:Login"];
            password = config["Data:EmailSettings:Password"];
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
             smtp = new SmtpClient(smtpServer, 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential(login, password);
            smtp.EnableSsl = true;
            Render = renderer;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            MailAddress from = new MailAddress(login, "Support");
            // кому отправляем
            MailAddress to = new MailAddress(email);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
           
            // тема письма
            m.Subject = subject;
            // текст письма
            m.Body = message;
            // письмо представляет код html
            m.IsBodyHtml = true;
            
            
            smtp.Send(m);
         
        }
        public  string HtmlEmailMesage<TModel>(string templateFilePath, TModel model)
        {
            
            var result=Render.RenderViewToString(templateFilePath,model).GetAwaiter().GetResult();



            
          
            return result;
        }
    }
```

сервіс отримує в залежностях файл конфігурації з якого зчитуються дані smpt. Також клас потребую механізм генерації змісту листа метод HtmlEmailMesage&lt;&gt;. Використовується механізм Razor, суть його роботи дуже проста використовується шаблон C\#+Html = .cshtml дали інфраструктура приймає дані obj і генерує чистий html. 

Реалізація

```csharp
 public class RazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToString<TModel>(string name, TModel model)
        {
           
            var actionContext = GetActionContext();
            
            var viewEngineResult = _viewEngine.FindView(actionContext, name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}' \nSerched location: {1}", name, viewEngineResult.SearchedLocations.Aggregate((x,y)=>x+";\n"+y)));
            }

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
           
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
                
            };

            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

       
    }
    public class EmailTemplateExpender : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.ViewName.Contains("Email"))
            {
                
                    yield return "/EmailTemplates/{0}.cshtml";
                
            }
            foreach (var item in viewLocations)
            {
                yield return item ;
            }
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
         
        }
    }

```

клас  EmailTemplateExpander дозволяє розширити зону пошуку .cshtml файлив у директорії Views-&gt;EmailTemplates, та для використання цього механізму при публікації необхідно включити цю папку у зборку публікації. Я додаю всі шаблони пошти у папку Views-&gt;Shared.

Для використання цього механізму відправки пошти додамо AppMailServices у контейнер залежностей:

```csharp
 services.AddTransient<AppMailSender>();
```

### Створення акаунтів студентів

Як розповідалось раніше для створення акаунтів студентів вчитель повинен зайти на сторінку групи та ввести потрібні дані після чого надійде лист до студента. Реалізація:

```csharp
 [HttpGet]
        public IActionResult CreateStudentAccount(int GroupID, string returnUrl)
        {
            var model = new CreateStudentAccountViewModel { GroupID = GroupID, returnUrl = returnUrl };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudentAccount(CreateStudentAccountViewModel model)
        {
            if (context.Students.Any(x=>x.NumberOfStudentBook==model.NumberOfStudentBook))
            {
                ModelState.AddModelError("",$"Student with number {model.NumberOfStudentBook} is alredy exist.");
            }

            if (ModelState.IsValid)
            {
                StudentUser student = new StudentUser() {
                    UserName = model.FirstName + "_" + model.SecondName+"_"+model.NumberOfStudentBook,
                    Email = model.Email,
                    GroupID = model.GroupID,
                    FirstName = model.FirstName,
                    LastName = model.SecondName,
                    NumberOfStudentBook = model.NumberOfStudentBook };

               
                var result= await Manager.CreateAsync(student);
                if (result.Succeeded)
                {
                    var user = await Manager.FindByEmailAsync(student.Email) as StudentUser;

                    try
                    {
                        var token = await Manager.GenerateEmailConfirmationTokenAsync(user);

                        string link = $@"{(HttpContext.Request.IsHttps?"https":"http")}://{HttpContext.Request.Host}{Url.Action("Registration", "Account", new { area = "Student", Token = token, Magic = student.Id })}";
                        var mailmodel = new StRgsMailModel { user = user, link = link };
                        var msg = MailSender.HtmlEmailMesage("EmailRegistration", mailmodel);
                
                        await MailSender.SendEmailAsync(student.Email, "Registration", msg);


                        await Manager.AddToRoleAsync(student, "Student");

                        return Redirect(model.returnUrl);
                    }
                    catch (Exception e)
                    {
                        await Manager.DeleteAsync(user);
                        ModelState.AddModelError("",e.Message);
                    }
                   
                    
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("",item.Description);
                    }
                }
            }
            return View(model);
        }

```

### AccountController

Для всіх аутентифікованих користувачів реалізація наступна:

```csharp
public class AccountController : Controller
    {
        private SignInManager<User> SignInManager;
        private UserManager<User> Manager;
        private AppMailSender EmailSender;
        public AccountController(SignInManager<User> sim, UserManager<User> manag,AppMailSender mail)
        {
            EmailSender = mail;
            SignInManager = sim;
            Manager = manag;
        }
        public async Task<IActionResult> Logout()
        {
            
            await SignInManager.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet]
   
        public IActionResult Login(string returnUrl = "/")
        {


            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [Authorize]
        public async Task<IActionResult> Profile(string returnUrl,string username)
        {
            var user = await Manager.FindByNameAsync(username);
            var roles =await Manager.GetRolesAsync(user);
            var sanitizer = new HtmlSanitizer();
            var model = new ProfileViewModel()
            {
                UserData=user,
                returnUrl=returnUrl,
                Roles=roles,
                HtmlData= sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(user.Description??"### No description"))
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details)
        {
            if (ModelState.IsValid)
            {
                var User = await Manager.FindByEmailAsync(details.Email);
                if (User != null)
                {
                    await SignInManager.SignOutAsync();
                    var signinresult = await SignInManager.PasswordSignInAsync(User, details.Password, details.RememberMe, false);
                    if (signinresult.Succeeded)
                    {
                        return LocalRedirect(details.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(details);
        }
        public async Task<IActionResult> ResetPassword(string Email)
        {
            var user = await Manager.FindByEmailAsync(Email);
            if (user==null)
            {
                return StatusCode(404);
            }
            var token = await Manager.GeneratePasswordResetTokenAsync(user);
            string link = $@"{(HttpContext.Request.IsHttps ? "https" : "http")}://{HttpContext.Request.Host}{Url.Action("ChangePassword", "Account", new {  Token = token, Magic = user.Id })}";
            var mailmodel = new PasswordResetMailModel { user = user, link = link };
            var msg = EmailSender.HtmlEmailMesage("ResetPassword",mailmodel);
            await EmailSender.SendEmailAsync(user.Email,"Reset password",msg);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string Token,string Magic)
        {
            var user = await Manager.FindByIdAsync(Magic);
            if (user==null)
            {
                return StatusCode(404);
            }
            var model = new ChangePasswordViewModel() {
                Token = Token, UserID = Magic
            };
            return View(model);
        }
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await Manager.FindByIdAsync(model.UserID);
            if (user == null)
            {
                return StatusCode(404);
            }
            var result = await Manager.ResetPasswordAsync(user,model.Token,model.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("",item.Description);
                }
                return View(model);
            }

            return LocalRedirect("/Account/Login");
        }
    }
```

Як можна побачити Account контролер відповідає за функціїї які притаманні всім користувачам, а саме:

* Вхід
* Вихід
* Зміна паролю
* Профіль акаунту

Для вчителя додається функціонал редагування акаунтів та перегляду приватної інформації\(Оцінки, відсутність на лекціях студентів\)

```csharp
 [Area("Teacher")]
    [Authorize(Roles ="Teacher")]
    public class AccountController : Controller
    {
       
        private UserManager<User> Manager;
        private RoleManager<IdentityRole> Roles;
        private ApplicationContext context;
        private SignInManager<User> SignInManager;
        private AppMailSender MailSender;
        IDocStorage Docs;
        IGroupsRepository Groups;
        public AccountController( UserManager<User> manag, RoleManager<IdentityRole> roles
            ,ApplicationContext ctx, SignInManager<User> signInManager,
            AppMailSender Sender,
            IGroupsRepository groups,
            IDocStorage docs)
        {
            context = ctx;
            Manager = manag;
            Roles = roles;
            SignInManager = signInManager;
            MailSender = Sender;
            Docs = docs;
            Groups = groups;

        }
        public async Task<IActionResult> StudentProfile(string returnUrl,string username)
        {
            var sanitizer = new HtmlSanitizer();
            var user = await Manager.FindByNameAsync(username);
            if (user==null&&!(user is StudentUser))
            {
                return LocalRedirect(returnUrl);
            }
            var model = new StudentProgressViewModel();
            model.User = user;
            model.returnUrl = returnUrl;
            model.Tasks = Docs.GetStudentTasks(user.Id) ?? new List<StTask>();
            model.Group = Groups.GetGroupById(((StudentUser)user).GroupID);
            model.UnvisitedLectures = Groups.Lectures.Where(x => x.Date < DateTime.Now && !x.Visits.Any(y => y.VisitorID == user.Id) && x.GroupID == model.Group.GroupID).AsNoTracking().ToList() ?? new List<Lecture>();
            model.VisitedLectures = Groups.Lectures.Where(x => x.Date < DateTime.Now && x.Visits.Any(y => y.VisitorID == user.Id) && x.GroupID == model.Group.GroupID).AsNoTracking().ToList() ?? new List<Lecture>();
            model.HtmlData = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(user.Description ?? "### No description"));


            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string username,string returnUrl = "/")
        {
            var user = await Manager.FindByNameAsync(username);
            var roles = await Manager.GetRolesAsync(user);
            var studentData = new EditViewModel
            {
                Avatar = user.Avatar,
                ReturnUrl = returnUrl,
                UserName = user.UserName,
                OldUserName=username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Description = user.Description,
                PhoneNumber = user.PhoneNumber,
                Roles = roles
            };
            return View(studentData);
        }
        [HttpPost]
        public async Task<IActionResult> Avatar(IFormFile file,string username)
        {
            
            var user = await Manager.FindByNameAsync(username);
            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
            {
                user.Avatar = binaryReader.ReadBytes((int)file.Length);
            }
            var result = await Manager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(true);
            }
            return Json(false);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel edited)
        {

            var user = await Manager.FindByNameAsync(edited.OldUserName);

            if (ModelState.IsValid)
            {



                user.Email = edited.Email;
                user.Description = edited.Description;
                user.PhoneNumber = edited.PhoneNumber;
                user.UserName = edited.UserName;
                user.FirstName = edited.FirstName;
                user.LastName = edited.LastName;
              


                var result = await Manager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    if (User.Identity.Name==edited.OldUserName&&edited.OldUserName!=edited.UserName)
                    {
                       await SignInManager.RefreshSignInAsync(user);
    }
                    return LocalRedirect(edited.ReturnUrl);
                }
                else
                {

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }
            var roles = await Manager.GetRolesAsync(user);
            edited.Roles = roles;
            edited.Avatar = user.Avatar;
            return View(edited);
        }

        [HttpGet]
        public IActionResult CreateStudentAccount(int GroupID, string returnUrl)
        {
            var model = new CreateStudentAccountViewModel { GroupID = GroupID, returnUrl = returnUrl };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudentAccount(CreateStudentAccountViewModel model)
        {
            if (context.Students.Any(x=>x.NumberOfStudentBook==model.NumberOfStudentBook))
            {
                ModelState.AddModelError("",$"Student with number {model.NumberOfStudentBook} is alredy exist.");
            }

            if (ModelState.IsValid)
            {
                StudentUser student = new StudentUser() {
                    UserName = model.FirstName + "_" + model.SecondName+"_"+model.NumberOfStudentBook,
                    Email = model.Email,
                    GroupID = model.GroupID,
                    FirstName = model.FirstName,
                    LastName = model.SecondName,
                    NumberOfStudentBook = model.NumberOfStudentBook };

               
                var result= await Manager.CreateAsync(student);
                if (result.Succeeded)
                {
                    var user = await Manager.FindByEmailAsync(student.Email) as StudentUser;

                    try
                    {
                        var token = await Manager.GenerateEmailConfirmationTokenAsync(user);

                        string link = $@"{(HttpContext.Request.IsHttps?"https":"http")}://{HttpContext.Request.Host}{Url.Action("Registration", "Account", new { area = "Student", Token = token, Magic = student.Id })}";
                        var mailmodel = new StRgsMailModel { user = user, link = link };
                        var msg = MailSender.HtmlEmailMesage("EmailRegistration", mailmodel);
                
                        await MailSender.SendEmailAsync(student.Email, "Registration", msg);


                        await Manager.AddToRoleAsync(student, "Student");

                        return Redirect(model.returnUrl);
                    }
                    catch (Exception e)
                    {
                        await Manager.DeleteAsync(user);
                        ModelState.AddModelError("",e.Message);
                    }
                   
                    
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("",item.Description);
                    }
                }
            }
            return View(model);
        }

     
       [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccount(string username,string returnUrl)
        {
            var user =await Manager.FindByNameAsync(username);
            if (!(await Manager.IsInRoleAsync(user,"Teacher")))
            {
                foreach (var item in context.Tasks.Where(x => x.UserID == user.Id))
                {
                    context.RemoveRange(context.Answers.Where(x=>x.StTaskID==item.StTaskID));
                    context.RemoveRange(context.TaskFiles.Where(x=>x.StTaskId==item.StTaskID));
                }
                context.RemoveRange(context.Tasks.Where(x=>x.UserID==user.Id));
                context.RemoveRange(context.Visits.Where(x=>x.VisitorID==user.Id));
                context.SaveChanges();
                await Manager.DeleteAsync(user);
            }
            return LocalRedirect(returnUrl);
        }
    }
```

Для студентів реалізація аналогічна, але з урізаним функціоналом.

### API методи 

Для завантаження аватарів реалізовані апs методи. Наприклад для вчителя:

```csharp
[HttpPost]
        public async Task<IActionResult> Avatar(IFormFile file,string username)
        {
            
            var user = await Manager.FindByNameAsync(username);
            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
            {
                user.Avatar = binaryReader.ReadBytes((int)file.Length);
            }
            var result = await Manager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(true);
            }
            return Json(false);
        }
```

### NewsContrller

Для налаштування новин для вчителя реалізовано наступний контролер:

```csharp
[Authorize(Roles ="Teacher")]
  [Area("Teacher")]
    public class NewsController : Controller
    {
        INewsStorage News;
        IFileStorage Files;
        IHostingEnvironment environment;
        public NewsController(INewsStorage news,IFileStorage files, IHostingEnvironment env)
        {
            News = news;
            Files = files;
            environment = env;
        }

        [HttpPost]
        public IActionResult AddNews(string name,string txtdata,IFormFile file)
        {
           
            if (file.ContentType.Contains("image")&&name!=null&&txtdata!= null)
            {
                string ext = "png";
                var File = new AppFile()
                {
                    FileName = (name + '.' + ext),
                    FileExtansion = ext
                };
               
              
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)file.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}", data.LongLength);
                    File.Value = data;
                }

                Files.AddFile(File);
                News.AddNews(new News() { Name = name, TextData = txtdata, PublicationDate = DateTime.Now });
                return Ok();
            }
            return StatusCode(404);
        }

        public IActionResult List(string returnUrl)
        {
            ViewBag.Return = returnUrl;
            return View(News.AllNews.AsNoTracking().OrderByDescending(x=>x.PublicationDate).ToList() ?? new List<News>());
        }
        public IActionResult RemoveNews(string returnUrl,int NewsID)
        {
            var news = News.AllNews.First(x => x.NewsID == NewsID);
            var file = Files.AllFiles.FirstOrDefault(x => x.FileName == news.Name + ".png");
            if (file!=null)
            {
                Files.RemoveFile(file);
            }
            
            News.DeleteNews(news);
       
            return LocalRedirect(returnUrl);
        }

    }
```

## Підсумок

В поточній версії були релізовані можливості адміністрування акаунту користувачем. Написаний сервіс для генерації змісту листа та відправлення його за допомогою smtp сервера. На прикінці було показано як реалізовані можливості управліннями новинами.


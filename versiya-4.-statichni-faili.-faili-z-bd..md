---
description: >-
  В четвертій версії описується: реалізація бізнес логіки роботи з файлами,
  статичні користувацькі файли.
---

# Версія 4. Статичні Файли. Файли з БД.

## Каталог wwwroot

Як відомо з [версії 1](./) у структурі проекту для роботи з статичними файлами за замовченням створений каталог wwwroot. Для того щоб додати компонент middleware, який буде обслуговувати запити до \[кореневий\_каталог\]/wwwroot/..... достатньо додати наступний код.

```csharp
app.UseStaticFiles();
```

кореневий каталог за замовченням задається методом CreateDefaultBuilder\(args\) у  program.cs за бажанням його можна змінити.

Таким чином тепер ми можемо повертати статичні клієнтські файли з файлової системи. 

## Збереження файлів у базі даних.

Так як доступ до даних у каталозі wwroot ніяк не обмежується було прийнято рішення зберігати файли у SQL базі даних де файли зберігаються у вигляді байтів. 

На саму таблицю Files можна подивитися [тут](versiya-2.-struktura-danikh..md). В цій таблиці зберігаються наступні об'єкти.

### AppFile

```csharp
 public class AppFile
    {
        public string FileName { get; set; }

        public byte[] Value { get; set; }

        public string FileSize { get; set; }

        public string FileExtansion { get; set; }

        public int AppFileID { get; set; }


    }
```

Базові властивості файлу.

### DocFile

```csharp
 public class DocFile : AppFile
    {
        public int DocPageId { get; set; }
     
        public string Description { get; set; }
    }
```

Даний тип файлу буде використовуватися для додатків на сторінках документації.

### TaskFile

```csharp
  public class TaskFile:AppFile
    {
        public int StTaskId { get; set; }

        public string Description { get; set; }
    }
```

Файл який додає студент до свого завдання.

Для доступу до файлів було реалізовано [патерн Repository.](versiya-2.-struktura-danikh..md)

## User.Avatar

Аватари користувачів зберігаються разом з даними авторизації. Для того щоб відобразити цю картинку можна вставити її масив байтів перетворений у тип String64 безпосередньо у Html розмітку, але такий підхід робить розмітку нечитаною. Інший спосіб це реалізувати метод який буде повертати масив байтів по HTTP. Маємо:\

```csharp
   public IActionResult Photo(string id)
        {
            return File(Users
            .UsersData
            .FirstOrDefault(x=>x.UserName==id).Avatar, "image/jpeg");
        }
```

Даний метод ініціалізований у контролері FileController. Метод повертає аватар користувача за його id. Доступ до методів контролера мають всі аутентифіковані користувачі. Url має наступний вигляд /File/Photo?id=\*\*\*.

## Адміністрування файлів

Доступ до зміни всіх файлів можуть користувачі з ролью Teacher. Наступний код описує URL Teacher/File/{Action}.

```csharp
[Area("Teacher")]
    [Authorize(Roles ="Teacher")]
    public class FileController : Controller
    {
        IFileStorage repository;
        IHostingEnvironment environment;
        public FileController(IFileStorage repo, IHostingEnvironment env)
        {
            environment = env;
            repository = repo;
        }

        public IActionResult GetFile(string filename)
        {
            return File(repository.AllFiles.FirstOrDefault(x=>x.FileName==filename).Value, "application/octet-stream", filename);
        }

        [HttpGet]
        public IActionResult AddFile(string returnUrl)
        {
           var model= new AddFileViewModel(){ReturnUrl=returnUrl };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddFile(AddFileViewModel model)
        {



            
            if (ModelState.IsValid)
            {
                if (repository.AllFiles.Any(x => x.FileName == model.File.FileName))
                {
                    ModelState.AddModelError("", "File with name " + model.File.FileName + " is alredy exist.");
                    return View(model);
                }

                var directory = Directory.GetFiles(environment.ContentRootPath + @"/wwwroot/FileExtansions");

                var ext = new String(model.File.FileName.TakeLast(model.File.FileName.Length - (model.File.FileName.LastIndexOf('.') == -1 ? model.File.FileName.Length : model.File.FileName.LastIndexOf('.')) - 1).ToArray());
                if (!directory.Any(x => x ==( "File" + ext + ".png")))
                {
                    ext = "";
                }
                var File = new AppFile() {
                    FileName =model.File.FileName,
                    FileExtansion=ext
                };
                using (var reader = new BinaryReader(model.File.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)model.File.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}",data.LongLength );
                    File.Value = data;
                }
           
                repository.AddFile(File);
                return LocalRedirect(model.ReturnUrl);
            }
            return View(model);
        }

        public IActionResult RemoveFile(string filename,string returnUrl)
        {
            var file = repository.AllFiles.FirstOrDefault(x=>x.FileName==filename);
            if(file!=null)repository.RemoveFile(file);
            return LocalRedirect(returnUrl);
        }

        public IActionResult List()
        {
            var directory = Directory.GetFiles(environment.ContentRootPath+@"/wwwroot/FileExtansions");
           
            var Model = new List<FileNameData>();
            foreach (var item in repository.AllFiles)
            {
                var Size = item.FileSize;
                    var FileName = item.FileName;
                var ext = item.FileExtansion;
                Model.Add(new FileNameData(FileName,ext,Size));
            }
            return View(Model);
        }
    }
```

## Підсумок

В даній версії була описана роботи з статичними клієнтськими файлами по типу html, css, js, images. Також було показано як зберігаються файли які покривають бізнес-задачі, а саме класи AppFile, DocFile, TaskFile. В кінці звіту було продемонстровано як працює адміністрування зі сторони користувачів з рол'ю вчителя.


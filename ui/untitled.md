---
description: >-
  В даній версії описується інструмент генерації html змісту використовуючи
  інфраструктуру mvc.
---

# Версія 1. Razor.

## Що таке Razor?

{% hint style="info" %}
[https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-2.2](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-2.2) - офіційна документація.
{% endhint %}

У більшості випадків при зверненні до веб-додатку користувач очікує отримати веб-сторінку з якими-небудь даними. У MVC для цього, як правило, використовуються уявлення, які й формують зовнішній вигляд програми. У ASP.NET MVC Core уявлення - це файли з розширенням cshtml, які містять код призначеного для користувача інтерфейсу в основному на мові html, а також конструкції Razor - спеціального движка уявлень, який дозволяє переходити від коду html до коду на мові C \#.

В нашому проекті усі уявлення знаходяться у каталогах:

* Views
* Areas/Teacher/Views
* Areas/Student/Views

## Як це працює?

Коли в методі контролера викликається метод View\(\), інфраструктура MVC будує об'єкт IViewResult в якому викликається метод генерації html. Для генерації html використовуються класи шаблони, які компілюються з файлів .cshtml. При звернені до уявлення MVC буде шукати його за замовченням у наступних каталогах Views/Shared/{Action}.cshtml та Views/{Controller}/{Action}.cshtml. Авжеж всі ці етапи можна налаштувати під себе.

Razor розмітка має наступний вигляд:

```aspnet
@model KovalukApp.Models.User
<ul id="slide-out" class="sidenav ">
    <li>
        <div class="user-view ">
            <div class="background">
                <img  width="300" src="~/img/first.jpg">
            </div>
            <a href="#user">
                @if (Model.Avatar == null)
                {
                    <img class="circle" style="object-fit: cover;" src="~/img/img_avatar.png"/>
                }
                else
                {
                    <img class="circle" style="object-fit: cover;" src="/File/Photo/@Model.UserName"/>
                }
                   
                        <span class="badge white-text chip teal">@Model.FirstName @Model.LastName</span>
                        <span class="badge white-text chip teal">@Model.Email</span>
                     
                  
                
            </a>
            <br /><br />
        </div>
    </li>
    <li><a class="subheader">Teacher Options</a></li>
    <li><a href="/"><i class="material-icons">home</i>Home</a> </li>
    <li><a asp-area="Teacher" asp-controller="Users" asp-action="List"><i class="material-icons">list</i>User accounts </a></li>
    <li><a asp-area="Teacher" asp-controller="Group" asp-action="List"><i class="material-icons">group</i>Student Manager</a></li>
    <li><a asp-area="Teacher" asp-controller="File" asp-action="List"><i class="material-icons">folder</i>File Manager</a></li>
    <li><a asp-area="Teacher" asp-controller="Documentation" asp-action="List"><i class="material-icons">event</i>Documentation </a></li>
    <li><a asp-area="Teacher" asp-controller="News" asp-action="List"><i class="material-icons">settings</i>News Manager</a></li>
    <li><a asp-area="Teacher" asp-controller="Task" asp-action="List"><i class="material-icons">code</i>Tasks<span id="unchecked" class="badge chip red white-text"></span></a></li>
    <li><div class="divider"></div></li>
    <li><a class="subheader">Settings</a></li>


    <li><a class="waves-effect" asp-area="Teacher" asp-action="Edit" asp-controller="Account" asp-route-username="@User.Identity.Name" asp-route-returnUrl="@(Context.Request.Path+Context.Request.QueryString)"><i class="material-icons">account_box</i>Edit my account</a></li>
    <li><a class="waves-effect" href="/Account/Logout"><i class="material-icons">exit_to_app</i>log out</a></li>

    <li><br/><br /></li>
</ul>
<script>
    $.ajax('/Teacher/Task/UnChecked', {
        method: "get",
        processData: false,
        contentType: false,
        success(count) {
            if (count !== 0) {
                $("#unchecked").text(count);
            } else {
                $("#unchecked").remove();
            }
              
        },
        error() {

            alert('Upload error');
        },
    });
</script>
```

Це розмітка для генерації меню вчителя. Як можна побачити вона має елементи кода C\# та HTML. Більш детально про Razor розмітку можна знайти на сайті Microsoft.

## Структура Views

В каталозі Views знаходяться файли \__ViewsImports та_  \_ViewsStart, які відповідають за налаштування посилань на простори імен та стартову конфігурацію.

Views-&gt;{Controller} мають уявлення, які відповідають назвам методів контролерів.

Views-&gt;Shared вміщуються в собі уявлення, які використовуються в інших уявленнях для зменшення обсягу кода.

Наприклад шаблон головної сторінки:

```aspnet
<!DOCTYPE html>
<html lang="en">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1.0" />
    <title>Kovaluk - @ViewBag.Title</title>
    <!-- CSS  -->
    <link href="~/materialized/matelialized.min.css" rel="stylesheet" />
    <link href="~/materialized/style.css" rel="stylesheet" />
    <link rel="shortcut icon" href="~/favicon.ico" type="image/x-icon">
    <link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet">
    <script src="~/Colorize/Colorize.js"></script>
    <script type="text/javascript"
            src="https://code.jquery.com/jquery-2.1.1.min.js"></script>
    <script src="~/materialized/materialized.min.js"></script>
    <link href="https://fonts.googleapis.com/css?family=Montserrat" rel="stylesheet">
    <link href="https://fonts.googleapis.com/css?family=Nanum+Pen+Script" rel="stylesheet">
    <script asp-src-include="~/dist/js/Preloader.js" rel="stylesheet" ></script>
    <link asp-href-include="~/dist/Css/*.css" rel="stylesheet" />
</head>
<body>

   

    <header>
        <nav>

            <div class="container">
                <div class="nav-wrapper">
                    <a class="brand-logo " href="/"><img class="Logotype circle z-depth-5 centered" src="~/Logotype.png" /></a>

                    @if (!User.Identity.IsAuthenticated)
                    {
                        <a href="/Account/Login" class="sidenav-trigger"><i class="material-icons left ">account_circle</i></a>
                    }
                    else
                    {
                        <a href="#" data-target="slide-out" class="sidenav-trigger"><i class="material-icons">menu</i></a>
                    }
                    <ul class="right hide-on-med-and-down">




                        @if (User.Identity.IsAuthenticated)
                        {


                            <li>
                                <div data-target="slide-out" class="sidenav-trigger">
                                    <a href="#">@User.Identity.Name  <i class="material-icons  right">more_vert</i></a>
                                </div>
                            </li>
                        }
                        else
                        {
                            <li>

                                <a href="/Account/Login"><i class="material-icons left">account_circle</i>Log in</a>
                            </li>

                        }

                    </ul>

                </div>
            </div>
        </nav>
        @if (User.Identity.IsAuthenticated)
        {
            @await Component.InvokeAsync("Menu")
        }
    </header>
    <div id="preloader">
        <div class="progress">
            <div class="indeterminate"></div>
        </div>
    </div>


    

    @RenderBody()

    <footer   class="page-footer">
        <div class="container">
            <div class="row">
                <div class="col l6 s12">
                    <h5 class="white-text">KovalukApp</h5>

                    This is a student project. Any amount would help support and continue development on this project and is greatly appreciated.
                </div>
               
            </div>
        </div>
        <div class="footer-copyright">
            <div class="container">
                © 2018 Copyright KovalukApp

                <a class="grey-text text-lighten-4 right" href="mailto:parovenko.alexander@gmail.com">Created by Alexander Parovenko</a>
            </div>
        </div>
    </footer>

    <script>
 
        $(document).ready(function () {
            $('.sidenav').sidenav();
        });

    </script>
</body>
</html>

```

Метод RenderBody\(\) генерує іншу Razor сторінку яка посилається на цей шаблон.

## Підсумок

Для генерації відповідей клієнту використовується строго типізована розмітка Razor завдяки якій можна швидко і без копіювання коду оперувати HTML. Також її можна використовувати для рендерінгу на стороні серверу частин клієнтського додатку.


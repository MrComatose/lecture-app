
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Razor;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using System.Linq;
using System.Collections.Generic;

namespace KovalukApp.Models
{
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
    public class StRgsMailModel
    {
        public StudentUser user { get; set; }
        public string link { get; set; }

    }
    public class PasswordResetMailModel
    {
        public User user { get; set; }
        public string link { get; set; }
    }
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

}

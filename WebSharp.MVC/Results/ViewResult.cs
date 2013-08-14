using Griffin.Networking.Protocol.Http.Protocol;
using System.IO;
using Xipton.Razor;
using System.Reflection;
using WebSharp.Exceptions;

namespace WebSharp.MVC.Results
{
    public class ViewResult : ActionResult
    {
        static ViewResult()
        {
            // Set defaults //
            ViewBase = "Views";

            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("WebSharp.MVC.DefaultRazorConfig.xml")))
                Razor = new RazorMachine(stream.ReadToEnd());
        }

        public static void RegisterLayout(string path)
        {

        }

        public static string ViewBase { get; set; }
        protected static RazorMachine Razor { get; set; }

        public string View { get; set; }
        public object Model { get; set; }
        public Controller Controller { get; set; }

        private readonly bool _resolveExact;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewResult"/> class.
        /// </summary>
        /// <param name="view">The view to render.</param>
        /// <param name="controller">The controller that is handeling the result.</param>
        /// <param name="model">The model to give the view.</param>
        /// <param name="resolveExact">if set to <c>true</c>, path will be resolved by calling object; else it will be resolved automatically.</param>
        public ViewResult(string view, Controller controller, object model = null, bool resolveExact = false)
        {
            View = view;
            Model = model;
            Controller = controller;
            _resolveExact = resolveExact;
        }

        public override void HandleRequest(IRequest request, IResponse response)
        {
            response.ContentType = "text/html";
            var writer = new StreamWriter(response.Body);
            var path = ResolveView(View);
            if (path == null)
                throw new HttpNotFoundException(string.Format("Requested view not found. Looking for: <b>{0}</b>", Path.Combine(Controller.Name, View)));

            string result = (string)Razor.ExecuteUrl(path.Replace('\\', '/'), Model, Controller.ViewBag, false, true).ToString();

            writer.Write(result);
            writer.Flush();
        }

        private string ResolveView(string view)
        {
            // This is good for using exact paths //
            // EX: I could do 'Pages/Login.cshtml' and it would find 'ViewBase/Pages/Login.cshtml'

            if (_resolveExact) 
                return File.Exists(Path.Combine(".", ViewBase, view)) ? view : null;

            return File.Exists(Path.Combine(".", ViewBase, Controller.Name, view)) ?
                    Path.Combine(Controller.Name, view) :
                    null;
        }
    }
}
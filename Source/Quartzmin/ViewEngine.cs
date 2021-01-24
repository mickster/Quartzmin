using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Quartzmin
{
    public class ViewEngine
    {
        readonly Services _services;
        readonly Dictionary<string, HandlebarsDotNet.HandlebarsTemplate<object, object>> _compiledViews = new Dictionary<string, HandlebarsDotNet.HandlebarsTemplate<object, object>>(StringComparer.OrdinalIgnoreCase);

        public bool UseCache { get; set; }

        public ViewEngine(Services services)
        {
            _services = services;
            UseCache = string.IsNullOrEmpty(services.Options.ViewsRootDirectory);
        }

        HandlebarsDotNet.HandlebarsTemplate<object, object> GetRenderDelegate(string templatePath)
        {
            if (UseCache)
            {
                lock (_compiledViews)
                {
                    if (!_compiledViews.ContainsKey(templatePath))
                    {
                        _compiledViews[templatePath] = _services.Handlebars.CompileView(templatePath);
                    }

                    return _compiledViews[templatePath];
                }
            }
            else
            {
                return _services.Handlebars.CompileView(templatePath);
            }
        }

        public string Render(string templatePath, object model) {
	        string render = GetRenderDelegate(templatePath)(model);
	        return render;
        }

        public void Encode(object value, TextWriter target)
        {
            _services.Handlebars.Configuration.TextEncoder.Encode(string.Format(CultureInfo.InvariantCulture, "{0}", value), target);
        }

        public string ErrorPage(Exception ex)
        {
            return Render("Error.hbs", new
            {
                ex.GetBaseException().GetType().FullName,
                Exception = ex,
                BaseException = ex.GetBaseException(),
                Dump = ex.ToString()
            });
        }
    }
}

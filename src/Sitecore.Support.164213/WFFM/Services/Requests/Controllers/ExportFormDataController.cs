using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Diagnostics;
using Sitecore.Data.Items;
using Sitecore.Form.Core.Configuration;
using Sitecore.Jobs;
using Sitecore.Pipelines;
using Sitecore.WFFM.Analytics.Model;
using Sitecore.WFFM.Services.Pipelines;
using Sitecore.WFFM.Speak.ViewModel;
using Sitecore.WFFM.Core.Resources;
using Sitecore.Web.Http.Filters;
using Sitecore.Forms.Core.Data;
using Sitecore.WFFM.Analytics.Providers;
using Sitecore.Configuration;
using Sitecore.Data;

namespace Sitecore.Support.WFFM.Services.Requests.Controllers
{
    public class SupportExportFormDataController: Controller
    {
        public SupportExportFormDataController() : this((IWfmDataProvider)Factory.CreateObject("wffm/formsDataProvider", true))
        {
        }

        public SupportExportFormDataController(IWfmDataProvider formsDataProvider)
        {
            Assert.ArgumentNotNull(formsDataProvider, "formsDataProvider");
            this.FormsDataProvider = formsDataProvider;
        }
        public IWfmDataProvider FormsDataProvider
        {
            get;
            private set;
        }

        [ValidateHttpAntiForgeryToken]
        public ActionResult Export(Guid id, int format)
        {
            Assert.ArgumentNotNull(id, "id");
            Assert.ArgumentNotNull(format, "format");
            bool flag = format == 0;
            Item item = StaticSettings.MasterDatabase.GetItem(new ID(id));
            Assert.IsTrue(item != null, "Can't find the form.");
            Job job = Context.Job;
            if (job != null)
            {
                job.Status.LogInfo(ResourceManager.Localize("READING_DATA_FROM_DATABASE"));
            }
            IEnumerable<IFormData> formData = this.FormsDataProvider.GetFormData(id);
            FormExportArgs formExportArgs = new FormExportArgs(new FormItem(item), new FormPacket(formData), Form.Core.Utility.WebUtil.GetTempFileName(), flag ? "text /xml" : "application/vnd.ms-excel");
            formExportArgs.Parameters.Add("contextUser", Context.User.Name);
            CorePipeline.Run(flag ? "exportToXml" : "exportToExcel", formExportArgs);
            return base.Json(new
            {
                //Sitecore.Support.164213
                File = HttpContext.Server.UrlEncode(formExportArgs.FileName)
                //End of changes
            }, JsonRequestBehavior.AllowGet);
        }
        [ValidateHttpAntiForgeryToken]
        public ActionResult Download(string file, int format)
        {
            Assert.ArgumentNotNullOrEmpty(file, "fileName");
            string contentType = (format == 0) ? "text/xml" : "application/vnd.ms-excel";
            string fileDownloadName = (format == 0) ? "export.xml" : "export.xls";
            return this.File(file, contentType, fileDownloadName);
        }
    }
}

using DataConfig.Helper;
using DataConfig.Models;
using DataConfig.Resources;
using System.Web.Mvc;

namespace DataConfig.Controllers
{
    public class MainController : Controller
    {
        //
        // GET: /Main/Start

        public ActionResult Start()
        {
            return View(new Data());
        }


        //
        // POST: /Main/Data

        public ActionResult Data(Data model)
        {
            if (string.IsNullOrEmpty(model.ConfigStorageName))
            {
                model.Errors.Add(Messages.MissingStorageName);
            }

            if (string.IsNullOrEmpty(model.ConfigStorageKey))
            {
                model.Errors.Add(Messages.MissingStorageKey);
            }

            if (model.Errors.Count == 0 && !Azure.TestConnection(model.ConfigStorageName, model.ConfigStorageKey))
            {
                model.Errors.Add(Messages.CannotConnect);
            }

            if (model.Errors.Count > 0)
            {
                return View("Start", model);
            }

            return View(model);
        }

    }
}

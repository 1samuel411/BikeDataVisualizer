using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BikeDataVisualizer.Models;
using Microsoft.Extensions.Configuration;

namespace BikeDataVisualizer.Controllers
{
    public class HomeController : Controller
    {
        public readonly IConfiguration configuration;

        public HomeController(IConfiguration config)
        {
            configuration = config;
        }

        public IActionResult Index()
        {
            DataVisualizationModel model = DataController.GetData();
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UrlExtender.Objects;

namespace UrlExtender.Controllers
{
    [ApiController]
    [Route("api")]
    public class ExtenderController : ControllerBase
    {
        private readonly ILogger<ExtenderController> _logger;
        private readonly Config config;

        public ExtenderController(ILogger<ExtenderController> logger)
        {
            _logger = logger;
            config = new Config();
        }

        private string SanitizeUrl(string url) {
            if(!Regex.Match(url, "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$").Success) {
                throw new ArgumentException("Invalid URL");
            }
            
            if(url.Contains('?')) {
                int startOfParams = url.IndexOf('?'); // this part splits the URL in two and lowercaseify's the non-case sensitive half
                string beginningUrl = url.Substring(0, startOfParams);
                string paramsUrl = url.Substring(startOfParams);
                url = beginningUrl.ToLower() + paramsUrl;
            }
            else {
                url = url.ToLower();
            }

            if(url.StartsWith("http://") || url.StartsWith("https://")) {
                return url;
            }
            else {
                return "http://" + url;
            }
        }

        [HttpGet("create")]
        public ActionResult Create(string url, bool pretty)
        {
            if(url == null) {
                return BadRequest("You did not supply a URL");
            }

            try {
                string extended = Extender.Add(SanitizeUrl(url));
                if(pretty)
                    return Ok($"Your {config.magicWord}-extended URL is:\n{config.rootUrl}{extended}");
                else
                    return Ok(config.rootUrl + extended);
            }
            catch (ArgumentException e) {
                if(e.Message == "Invalid URL") 
                    return BadRequest(e.Message);
                else throw e;
            }
        }

        [HttpGet("{url}")]
        public ActionResult ExtenderDereferance(string url) {
            try {                
                return new RedirectResult(Extender.Dereferance(url));
            }
            catch(System.Collections.Generic.KeyNotFoundException) {
                return NotFound("Error 404: This URL does not exist");
            }
        }

        [HttpGet("hits")]
        public ActionResult HitCount(string url, bool pretty = false) {
            try {
                string clean = SanitizeUrl(url);
                int hits = Extender.GetHits(clean);
                if(pretty)
                    return Ok($"The {config.magicWord}-extended version of {url} has been visited {hits} time(s)");
                else
                    return Ok(Extender.GetHits(clean));
            }
            catch (KeyNotFoundException) {
                return NotFound($"The provided URL has not yet been {config.magicWord}-extended");
            }
            catch (ArgumentException e) {
                if(e.Message == "Invalid URL") 
                    return BadRequest(e.Message);
                else throw e;
            }
        }

        [HttpGet]
        public ActionResult RedirectToMain() {
            if(config.uiUrl == null) {
                return BadRequest("The administrator did not set an end-user webpage");
            }
            return new RedirectResult(config.uiUrl);
        }
    }
}

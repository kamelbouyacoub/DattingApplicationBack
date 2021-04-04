using DattingApplication.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 

namespace DattingApplication.Controllers
{
    public class BuggyController : BaseController
    {
        private readonly DataContext Context;
        public BuggyController(DataContext _context)
        {
            this.Context = _context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }


         [HttpGet("not-found")]
        public ActionResult<string> GetNotFound()
        {
            var thing = Context.Users.Find(-1);
            if (thing == null) return NotFound();
            return Ok(thing);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var thing = Context.Users.Find(-1);
            var thingToReturn = thing.ToString();
            return thingToReturn;
        }

         [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequestFound()
        {
            return BadRequest("This was a bad request"); 
        }
 
    }
}

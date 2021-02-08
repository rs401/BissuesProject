using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bissues.Data;
using Bissues.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Dynamic;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bissues.Controllers
{
    /// <summary>
    /// Bissues controller handles Bissue requests
    /// </summary>
    public class BissuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BissuesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bissues
        /// <summary>
        /// Bissues Index
        /// </summary>
        /// <returns>Returns Bissues Index view populated with a list of all Bissues</returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bissues.Include(b => b.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bissues/Details/5
        /// <summary>
        /// Bissues Details view lists the details of the Bissue as well as all 
        /// Messages related to the Bissue
        /// </summary>
        /// <param name="id">Bissue Id</param>
        /// <returns>Details view lists the details of the Bissue as well as all 
        /// Messages related to the Bissue</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bissue = await _context.Bissues
                .Include(b => b.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bissue == null)
            {
                return NotFound();
            }

            // Using ViewData to send Owner
            var user = await _userManager.FindByIdAsync(bissue.AppUserId);
            // string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var currentUsername = !string.IsNullOrEmpty((string)userId)
            //     ? userId
            //     : "Anonymous";

            // AppUser user = _context.AppUsers.Where(au => au.Id == currentUsername).Select();
            // AppUser theUser = _context.AppUsers.Find(currentUsername);
            if(user != null)
            {
                ViewData["Owner"] = bissue.AppUser;
            }
            /* Dynamic model to bundle the Bissue's Messages with it. */
            dynamic tmpmodel = new ExpandoObject();
            tmpmodel.Bissue = bissue;
            /* Get bissues if any */
            ICollection<Message> messages = _context.Messages.Where(m => m.BissueId == id).ToList();
            if(messages.Count <= 0)
            {
                tmpmodel.Messages = null;
            }
            else
            {
                tmpmodel.Messages = messages;
            }

            return View(tmpmodel);

            // return View(bissue);
        }

        // GET: Bissues/Create
        [Authorize]
        /// <summary>
        /// Bissues Create GET view displays a form to create a Bissue
        /// </summary>
        /// <returns>Bissues Create view displays a form to create a Bissue</returns>
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            return View();
        }

        // POST: Bissues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Bissues Create POST takes a Bissue and saves the Bissue to the db 
        /// and sets the created/modified date for the bissue as well as alters 
        /// the Modified date of the related Project.
        /// </summary>
        /// <param name="bissue"></param>
        /// <returns></returns>
        public async Task<IActionResult> Create([Bind("Id,Title,Description,IsOpen,AppUserId,ProjectId,CreatedDate,ModifiedDate")] Bissue bissue)
        {
            if (ModelState.IsValid)
            {
                bissue.CreatedDate = DateTime.UtcNow;
                bissue.ModifiedDate = DateTime.UtcNow;
                
                _context.Add(bissue);
                var project = await _context.Projects.FindAsync(bissue.ProjectId);
                project.ModifiedDate = DateTime.UtcNow;
                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", bissue.ProjectId);
            return View(bissue);
        }

        // GET: Bissues/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            /* Still need to figure out the issue with AppUsers not being set to 
             * owners of things and make sure the authenticated user is either 
             * the Bissue owner or an Admin to be able to edit or delete. */
            if (id == null)
            {
                return NotFound();
            }

            var bissue = await _context.Bissues.FindAsync(id);
            if (bissue == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", bissue.ProjectId);
            return View(bissue);
        }

        // POST: Bissues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IsOpen,AppUserId,ProjectId,CreatedDate,ModifiedDate")] Bissue bissue)
        {
            if (id != bissue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bissue.ModifiedDate = DateTime.UtcNow;
                    _context.Update(bissue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BissueExists(bissue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", bissue.ProjectId);
            return View(bissue);
        }

        // GET: Bissues/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bissue = await _context.Bissues
                .Include(b => b.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bissue == null)
            {
                return NotFound();
            }

            return View(bissue);
        }

        // POST: Bissues/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bissue = await _context.Bissues.FindAsync(id);
            _context.Bissues.Remove(bissue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BissueExists(int id)
        {
            return _context.Bissues.Any(e => e.Id == id);
        }
    }
}

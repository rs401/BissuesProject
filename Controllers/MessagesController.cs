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
using System.Security.Claims;

namespace Bissues.Controllers
{
    /// <summary>
    /// MessagesController handles Messages requests
    /// </summary>
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Messages
        /// <summary>
        /// Index returns the Messages Index view with a list of all messages. I should probably change this or remove it.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Messages.Include(m => m.Bissue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Messages/Details/5
        /// <summary>
        /// Details view I think I should also remove this
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Bissue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        [Authorize]
        /// <summary>
        /// Message Create GET view displays a form to create a message.
        /// </summary>
        /// <returns>Create view displays a form to create a message</returns>
        public IActionResult Create(int? bid)
        {
            if(bid != null)
            {
                ViewData["BissueId"] = new SelectList(_context.Bissues.Where(b => b.Id == bid).ToList(), "Id", "Description");
            }
            else
            {
                ViewData["BissueId"] = new SelectList(_context.Bissues, "Id", "Description");
            }
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Message Create POST takes a Message, sets the created/modified dates and saves to db. Need to add notification to Bissue owner
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<IActionResult> Create([Bind("Id,Body,AppUserId,AppUser,BissueId")] Message message)
        {
            if (ModelState.IsValid)
            {
                message.CreatedDate = DateTime.UtcNow;
                message.ModifiedDate = DateTime.UtcNow;
                message.AppUser = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                _context.Add(message);
                var bissue = await _context.Bissues.FindAsync(message.BissueId);
                bissue.ModifiedDate = DateTime.UtcNow;
                _context.Update(bissue);
                await _context.SaveChangesAsync();
                // return RedirectToAction(nameof(Index));
                //public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, string controllerName, object routeValues);
                return RedirectToAction("Details", "Bissues", new { id = message.BissueId});
            }
            ViewData["BissueId"] = new SelectList(_context.Bissues, "Id", "Description", message.BissueId);
            return View(message);
        }

        // GET: Messages/Edit/5
        [Authorize]
        /// <summary>
        /// Message Edit GET displays message form
        /// </summary>
        /// <param name="id">Message Id</param>
        /// <returns>Edit view</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = message.AppUserId;
            ViewData["CreatedDate"] = message.CreatedDate;
            ViewData["BissueId"] = new SelectList(_context.Bissues, "Id", "Description", message.BissueId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Message Edit POST 
        /// </summary>
        /// <param name="id">Message Id</param>
        /// <param name="message">Message Body</param>
        /// <returns>Message Edit view</returns>
        public async Task<IActionResult> Edit(int id, [Bind("Id,Body,AppUserId,AppUser,BissueId,CreatedDate,ModifiedDate")] Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    message.ModifiedDate = DateTime.UtcNow;
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "Bissues", new { id = message.BissueId });
            }
            ViewData["BissueId"] = new SelectList(_context.Bissues, "Id", "Description", message.BissueId);
            return View(message);
        }

        // GET: Messages/Delete/5
        [Authorize]
        /// <summary>
        /// Message Delete GET view
        /// </summary>
        /// <param name="id">Message Id</param>
        /// <returns>Message Delete view</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Bissue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Message DeleteConfirm POST
        /// </summary>
        /// <param name="id">Message Id</param>
        /// <returns>Message Index view</returns>
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}

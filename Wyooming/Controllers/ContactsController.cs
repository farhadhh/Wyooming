using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wyooming.Data;
using Wyooming.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Wyooming.Authorization;

namespace Wyooming.Controllers
{
    public class ContactsController : Controller
    {
        private readonly WyoomingContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactsController(WyoomingContext context, 
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Contact.ToListAsync());
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        public IActionResult Create()
        {

            return View();
        }



        // POST: Contacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
                
            }

            var contact = ViewModel_to_model(new Contact(), editModel);
            contact.OwnerID = _userManager.GetUserId(User);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact, ContactOperations.Create);

            if (!isAuthorized)
            {
                return new ChallengeResult();
            }

            _context.Add(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact.SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact, ContactOperations.Update);

            if (!isAuthorized)
            {
                return new ChallengeResult();
            }
                
            var editModel = Model_to_viewModel(contact);

            return View(editModel);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContactEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }
            var contact = await _context.Contact.SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact, ContactOperations.Update);

            if (!isAuthorized)
            {
                return new ChallengeResult();
            }

            contact = ViewModel_to_model(contact, editModel);
            if (contact.Status == ContactStatus.Approved)
            {
                var canApprove = await _authorizationService.AuthorizeAsync(User, contact, ContactOperations.Approve);

                if (!canApprove)
                {
                    contact.Status = ContactStatus.Submitted;
                }
            }

            _context.Update(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact,
                                ContactOperations.Delete);
            if (!isAuthorized)
            {
                return new ChallengeResult();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contact.SingleOrDefaultAsync(m => m.ContactId == id);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact,
                                        ContactOperations.Delete);
            if (!isAuthorized)
            {
                return new ChallengeResult();
            }

            _context.Contact.Remove(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private Contact ViewModel_to_model(Contact contact, ContactEditViewModel editModel)
        {
            contact.Address = editModel.Address;
            contact.City = editModel.City;
            contact.Email = editModel.Email;
            contact.Name = editModel.Name;
            contact.State = editModel.State;
            contact.Zip = editModel.Zip;

            return contact;
        }

        private ContactEditViewModel Model_to_viewModel(Contact contact)
        {
            var editModel = new ContactEditViewModel();

            editModel.ContactId = contact.ContactId;
            editModel.Address = contact.Address;
            editModel.City = contact.City;
            editModel.Email = contact.Email;
            editModel.Name = contact.Name;
            editModel.State = contact.State;
            editModel.Zip = contact.Zip;

            return editModel;
        }

        private bool ContactExists(int id)
        {
            return _context.Contact.Any(e => e.ContactId == id);
        }
    }
}

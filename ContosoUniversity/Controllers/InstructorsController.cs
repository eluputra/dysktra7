using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels; //added the school view model but i dont think it matter it should already been included in using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = await _context.Instructors // get instructor and added to view model
                  .Include(i => i.OfficeAssignment) // i am not really sure what is i mean is i == number of item?
                  .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Enrollments) // i am not really sure what this one do maybe adding endrollment? 
                            .ThenInclude(i => i.Student)
                  .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                  .AsNoTracking()
                  .OrderBy(i => i.LastName)
                  .ToListAsync();

            if (id != null) // if id not null
            {
                ViewData["InstructorID"] = id.Value; // add the value into instructor id
                Instructor instructor = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single(); //You use the Single method on a collection when you know the collection will have only one item. The Single method throws an exception if the collection passed 
                                                     //to it's empty or if there's more than one item
                viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }
            
            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;// add the value into course id
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                }
                viewModel.Enrollments = selectedCourse.Enrollments; // maybe this is me new with C# but i have some doubt about this line of code functionality need to ask teacher about this particular area
            }

            return View(viewModel);

            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                }
                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,LastName,FirstMidName,HireDate")] Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorToUpdate = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (await TryUpdateModelAsync<Instructor>(
                instructorToUpdate,
                "",
                i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
            {// Updates the retrieved Instructor entity with values from the model binder. The TryUpdateModel overload enables you to whitelist 
                if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))// If the office location is blank, sets the Instructor.OfficeAssignment property to null so that the related row in the OfficeAssignment table will be deleted.
                {
                    instructorToUpdate.OfficeAssignment = null;
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(instructorToUpdate);
        }

        
        

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.ID == id);
        }
    }
}

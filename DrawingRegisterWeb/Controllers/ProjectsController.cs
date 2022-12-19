﻿using DrawingRegisterWeb.Data;
using DrawingRegisterWeb.Models;
using DrawingRegisterWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DrawingRegisterWeb.Controllers
{
	public class ProjectsController : Controller
	{
		private readonly DrawingRegisterContext _context;

		public ProjectsController(DrawingRegisterContext context)
		{
			_context = context;
		}

		//GET: Projects
		public async Task<IActionResult> Index(string search, string states)
		{
			IQueryable<string> statesQuery = from s in _context.ProjectState orderby s.Name select s.Name;

			var project = from p in  _context.Project.Include(s => s.ProjectState) select p;

			if (search != null) 
			{ 
				project = project.Where(p => 
				p.Name.Contains(search) || 
				p.ProjectNubmer.Contains(search));
			}

			if (states != null)
			{
				project = project.Where(p => p.ProjectState!.Name == states);
			}

			var projectVM = new ProjectVM
			{
				ProjectStates = new SelectList(await statesQuery.Distinct().ToListAsync()),
				Projects = await project.OrderBy(p => p.ProjectNubmer).ToListAsync()
			};

			return View(projectVM);
		}

		// GET: Project/Details
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Project == null)
			{
				return NotFound();
			}

			var project = await _context.Project
				.Include(p => p.ProjectState)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (project == null)
			{
				return NotFound();
			}

			var drawings = from d in _context.Drawing where d.ProjectId == id select d;
			var documentations = from d in _context.Documentation where d.ProjectId == id select d;
			var layouts = from l in _context.Layout where l.ProjectId == id select l;

			var ProjectVM = new ProjectVM
			{
				Project = project,
				Drawings = await drawings.OrderBy(d => d.DrawingNumber).ToListAsync(),
				Documentations = await documentations.OrderBy(d => d.FileName).ToListAsync(),
				Layouts = await layouts.OrderBy(l => l.FileName).ToListAsync()
			};

			return View(ProjectVM);
		}

		// GET: Project/Create
		public IActionResult Create()
		{
			ViewData["ProjectStateId"] = new SelectList(_context.ProjectState, "Id", "Name");
			return View();
		}

		// POST: Project/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,ProjectNubmer,Name,Description,DeadlineDate,ProjectStateId")] Project project)
		{
			if (ModelState.IsValid)
			{
				_context.Add(project);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["ProjectStateId"] = new SelectList(_context.ProjectState, "Id", "Name", project.ProjectStateId);
			return View(project);
		}

		// GET: Project/Edit
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Project == null)
			{
				return NotFound();
			}

			var project = await _context.Project.FindAsync(id);
			if (project == null)
			{
				return NotFound();
			}
			ViewData["ProjectStateId"] = new SelectList(_context.ProjectState, "Id", "Name", project.ProjectStateId);
			return View(project);
		}

		// POST: Project/Edit
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectNubmer,Name,Description,CreateDate,DeadlineDate,ProjectStateId")] Project project)
		{
			if (id != project.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(project);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProjectExists(project.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Details), new {id});
			}
			ViewData["ProjectStateId"] = new SelectList(_context.ProjectState, "Id", "Name", project.ProjectStateId);
			return View(project);
		}

		// GET: Project/Delete
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Project == null)
			{
				return NotFound();
			}

			var project = await _context.Project
				.Include(p => p.ProjectState)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		// POST: Project/Delete
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Project == null)
			{
				return Problem("Entity set 'DrawingRegisterContext.Project'  is null.");
			}
			var project = await _context.Project.FindAsync(id);
			if (project != null)
			{
				_context.Project.Remove(project);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ProjectExists(int id)
		{
			return _context.Project.Any(e => e.Id == id);
		}
	}
}

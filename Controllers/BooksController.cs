using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SesliKitapWeb.Data;
using SesliKitapWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;

namespace SesliKitapWeb.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? search)
        {
            var booksQuery = _context.Books.Include(b => b.Reviews).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.ToLower(new System.Globalization.CultureInfo("tr-TR"));
                booksQuery = booksQuery.Where(b =>
                    b.Title.ToLower().Contains(normalized) ||
                    b.Author.ToLower().Contains(normalized)
                );
            }

            var books = await booksQuery.ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
                .Include(b => b.UserBooks)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book, IFormFile? pdfFile)
        {
            if (ModelState.IsValid)
            {
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    book.PdfFilePath = $"/pdfs/{uniqueFileName}";
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // POST: Books/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddReview(int bookId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                BookId = bookId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        // POST: Books/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userBook = await _context.UserBooks
                .FirstOrDefaultAsync(ub => ub.BookId == bookId && ub.UserId == user.Id);

            if (userBook == null)
            {
                userBook = new UserBook
                {
                    BookId = bookId,
                    UserId = user.Id,
                    IsCompleted = true,
                    LastReadAt = DateTime.Now
                };
                _context.UserBooks.Add(userBook);
            }
            else
            {
                userBook.IsCompleted = true;
                userBook.LastReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Categories(string? category)
        {
            var categories = await _context.Books
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var books = string.IsNullOrEmpty(category)
                ? new List<Book>()
                : await _context.Books.Where(b => b.Category == category).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var userBook = await _context.UserBooks
                .FirstOrDefaultAsync(ub => ub.BookId == bookId && ub.UserId == user.Id);

            if (userBook == null)
            {
                userBook = new UserBook
                {
                    BookId = bookId,
                    UserId = user.Id,
                    IsFavorite = true,
                    LastReadAt = DateTime.Now
                };
                _context.UserBooks.Add(userBook);
            }
            else
            {
                userBook.IsFavorite = !userBook.IsFavorite;
                userBook.LastReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = bookId });
        }

        private string ExtractTextFromPdf(string pdfFilePath)
        {
            using var reader = new PdfReader(pdfFilePath);
            using var pdfDoc = new PdfDocument(reader);
            var text = new StringBuilder();

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                text.Append(PdfTextExtractor.GetTextFromPage(page));
            }

            return text.ToString();
        }
    }
}
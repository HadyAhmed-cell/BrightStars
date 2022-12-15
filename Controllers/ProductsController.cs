using BrightStars.Data;
using BrightStars.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStars.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        [HttpGet]

        public ActionResult<IEnumerable<Product>> GetProducts(string? search, string? sortType, string? sortOrder, int pageSize = 2, int pageNumber = 1)
        {

            IQueryable<Product> prods = _context.Products.AsQueryable();

            if (string.IsNullOrWhiteSpace(search) == false)
            {
                search = search.Trim();
                prods = _context.Products.Where(e => e.Name.Contains(search));

            }

            if (!string.IsNullOrWhiteSpace(sortType) && !string.IsNullOrWhiteSpace(sortOrder))
            {
                if (sortType == "Name")
                {
                    if (sortOrder == "asc")
                    {
                        prods = prods.OrderBy(e => e.Name);
                    }
                    else if (sortOrder == "desc")
                    {
                        prods.OrderByDescending(e => e.Name);
                    }
                }

                else if (sortType == "Description")
                {
                    if (sortOrder == "asc")
                    {
                        prods = prods.OrderBy(e => e.Description);
                    }
                    else if (sortOrder == "desc")
                    {
                        prods.OrderByDescending(e => e.Description);
                    }
                }


            }


            prods = prods.Skip(pageSize * (pageNumber - 1)).Take(pageSize);

            return Ok(prods);
        }

        /// <summary>
        /// Get a Product From Database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>


        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> GetProduct(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            Product product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);


            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }




        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult PostProduct([FromForm] Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            if (product.ImageFile == null)
            {
                product.ImageUrl = "\\images\\No_Image.png";
            }
            else
            {
                string imgExtension = Path.GetExtension(product.ImageFile.FileName);

                //if (imgExtension != ".png" && imgExtension != ".jpg")
                //{
                //    return BadRequest("Only .png and .jpg images are allowed");
                //}
                List<string> allowedImageExtensions = new List<string>() { ".jpg", ".png" };

                if (allowedImageExtensions.Contains(imgExtension) == false)
                {
                    return BadRequest("Not Allowed Image Extension");

                }

                if (product.ImageFile.Length > 2000000)
                {
                    return BadRequest("Allowed image Maximum size is 2 MB");
                }

                Guid imgGuid = Guid.NewGuid();
                string imgName = imgGuid + imgExtension;
                product.ImageUrl = "\\images\\" + imgName;

                string imgPath = _environment.WebRootPath + product.ImageUrl;

                FileStream imgStream = new FileStream(imgPath, FileMode.CreateNew);
                product.ImageFile.CopyTo(imgStream);
                imgStream.Dispose();

            }

            if (product.Name.Trim() == product.Description.Trim())
            {
                ModelState.AddModelError("NameAndDescriptionMatch", "Name and Description must not match");
                return BadRequest(ModelState);
            }

            if (_context.Products.Any(p => p.Name == product.Name))
            {
                ModelState.AddModelError("DuplicateName", "The product name already exists");
                return BadRequest(ModelState);
            }

            product.CreatedAt = DateTime.Now;
            _context.Products.Add(product);
            _context.SaveChanges();


            return CreatedAtRoute("GetById", new { id = product.Id }, product);


        }
        /// <summary>
        /// Delete a Product Permanently From Database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public ActionResult DeleteProduct(int? id)
        {
            if (id == null || id == 0)
            {
                return BadRequest();
            }

            Product product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.ImageUrl != "\\images\\No_Image.png")
            {
                string imgPath = _environment.WebRootPath + product.ImageUrl;

                if (System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                }
            }
            _context.Products.Remove(product);
            _context.SaveChanges();

            return NoContent();
        }

        /// <summary>
        /// Update/Edit the current product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns></returns>

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public ActionResult PutProduct(int id, [FromForm] Product product)
        {
            if (product == null || id == 0 || product.Id != id)
            {
                return BadRequest();
            }

            if (product.ImageFile != null)
            {

                string imgExtension = Path.GetExtension(product.ImageFile.FileName);

                List<string> allowedImageExtensions = new List<string>() { ".jpg", ".png" };

                if (allowedImageExtensions.Contains(imgExtension) == false)
                {
                    return BadRequest("Not Allowed Image Extension");

                }

                if (product.ImageFile.Length > 2000000)
                {
                    return BadRequest("Allowed image Maximum size is 2 MB");
                }



                if (product.ImageUrl != "\\images\\No_Image.png")
                {
                    string oldImgPath = _environment.WebRootPath + product.ImageUrl;
                    if (System.IO.File.Exists(oldImgPath))
                    {
                        System.IO.File.Delete(oldImgPath);
                    }
                }

                Guid imgGuid = Guid.NewGuid();
                string imgName = imgGuid + imgExtension;
                string imgUrl = "\\images\\" + imgName;
                product.ImageUrl = imgUrl;

                string imgPath = _environment.WebRootPath + imgUrl;
                FileStream imgStream = new FileStream(imgPath, FileMode.Create);
                product.ImageFile.CopyTo(imgStream);
                imgStream.Dispose();



            }








            if (product.Name.Trim() == product.Description.Trim())
            {
                ModelState.AddModelError("nameanddescriptionmatch", "name and description must not match");
                return BadRequest(ModelState);
            }

            if (_context.Products.Any(p => p.Name == product.Name && p.Id != product.Id))
            {
                ModelState.AddModelError("duplicatename", "the product name already exists");
                return BadRequest(ModelState);
            }

            product.LastUpdatedAt = DateTime.Now;
            _context.Products.Update(product);
            _context.SaveChanges();
            return NoContent();

        }
    }
}

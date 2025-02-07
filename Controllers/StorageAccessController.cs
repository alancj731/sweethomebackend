using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;




namespace sweetbackend.Controllers
{



    public class TargetPath
    {
        
        public string? type { get; set; }
        public string? path { get; set; }
        public string? newpath { get; set; }
    }


    [Route("api/storage")]
    [ApiController]
    public class StorageAccessController(AppDbContext context, IStorageAccessService storageAccessService) : ControllerBase
    {

        private readonly AppDbContext _context = context;
        private readonly IStorageAccessService _storageAccessService = storageAccessService;

        [HttpGet("content")]
        [Authorize]
        public async Task<IActionResult> GetStorage([FromQuery] TargetPath? folderPath)
        {
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            var path = folderPath?.path ?? string.Empty;

            var folderContent = await this._storageAccessService.GetStorage(email, path);

            var jsonContent = JsonSerializer.Serialize(folderContent);

            return Ok(jsonContent);
        }

        [HttpGet("download")]
        [Authorize]
        public IActionResult Download([FromQuery] TargetPath targetPath)
        {
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            var path = targetPath.path;
            if (path == null)
            {
                return BadRequest("Path is required.");
            }

            if (!System.IO.File.Exists(path))
            {
                return NotFound("File not found.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(path);
            var fileName = Path.GetFileName(path);
            var contentType = "application/octet-stream";

            return File(fileBytes, contentType, fileName);
        }

        [HttpPut("rename")]
        [Authorize]
        public async Task<IActionResult> RemameTarget([FromQuery] TargetPath targetPath)
        {   
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            Console.WriteLine("targetPath", targetPath);

            var type = targetPath.type;
            var path = targetPath.path;
            var newPath = targetPath.newpath;


            if (type == null || (type !="file" && type != "folder") || path == null || newPath == null)
            {
                return BadRequest("Type, path and new path are required. Type must be 'file' or 'folder'.");
            }

             try
            {
                var result = await this._storageAccessService.Rename(email, type, path, newPath);


                if (!result)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to rename target.");
                }

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpPost("createfolder")]
        [Authorize]
        public async Task<IActionResult> CreateFolder([FromQuery] TargetPath targetPath)
        {
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            var path = targetPath.path;
            if (path == null)
            {
                return BadRequest("Path is required.");
            }

            try
            {
                var result = await this._storageAccessService.CreateFolder(email, path);


                if (!result)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to create folder.");
                }

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }


        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] TargetPath targetPath)
        {
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            var path = targetPath.path;
            if (path == null)
            {
                return BadRequest("Path is required.");
            }


            if (file == null)
            {
                return BadRequest("File is required.");
            }

            if (System.IO.File.Exists(path))
            {
                return BadRequest("File already exists.");
            }

            try
            {
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }

            return Ok();
        }

        [HttpDelete("delete")]
        [Authorize]
        public IActionResult Delete([FromQuery] TargetPath targetPath)
        {
            Console.WriteLine("Delete touched!");

            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            var path = targetPath.path;
            if (path == null)
            {
                return BadRequest("Path is required.");
            }

            if (!System.IO.File.Exists(path) && !Directory.Exists(path))
            {
                return NotFound("Target not found.");
            }

            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                else
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }

            return Ok();
        }

    }
}
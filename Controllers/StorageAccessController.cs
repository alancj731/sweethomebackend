using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;




namespace sweetbackend.Controllers
{



    public class TargetPath
    {
        public string? path { get; set; }
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
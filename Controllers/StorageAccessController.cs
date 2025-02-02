using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;




namespace sweetbackend.Controllers
{
    


    public class FolderPath
    {
        public string path { get; set; }
    }


    [Route("api/storage")]
    [ApiController]
    public class StorageAccessController(AppDbContext context, IStorageAccessService storageAccessService) : ControllerBase
    {

        private readonly AppDbContext _context = context;
        private readonly IStorageAccessService _storageAccessService = storageAccessService;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetStorage([FromBody] FolderPath folderPath)
        {
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value; 
            var path = folderPath.path;

            var folderContent = await this._storageAccessService.GetStorage(email, path);

            var jsonContent = JsonSerializer.Serialize(folderContent);
            // var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return Ok(jsonContent);
        }

        private async Task<string?> GetRootFolderPath(string email)
        {
            // Implement the logic to get the root folder based on the email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                return user.StorageFolder;
            }

            return null;
        }

        private StorageFolder GetFolder(string path, string pathName)
        {
            // Implement the logic to get the root folder based on the email
            var folder = new StorageFolder
            {
                id = path,
                name = pathName,
            };
            folder.children = this.GetChildren(folder.id);
            return folder;
        }

        public List<object> GetChildren(string folderPath)
        {
            var children = new List<object>();

            List<string> directories = new List<string>();
            List<string> files = new List<string>();

            try
            {
                // Get subdirectories in the given folder
                directories.AddRange(Directory.GetDirectories(folderPath));

                foreach (var directory in directories)
                {
                    var folder = new StorageFolder
                    {
                        id = directory,
                        name = Path.GetFileName(directory),
                    };
                    children.Add(folder);
                }

                // Get files in the given folder
                files.AddRange(Directory.GetFiles(folderPath));

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var fileObject = new StorageFile
                    {
                        id = file,
                        name = fileInfo.Name,
                    };
                    children.Add(fileObject);
                }
                return children;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return [];
            }

        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;




namespace sweetbackend.Controllers
{
    public class File
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; } = "file";
    };

    public class Folder
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; } = "folder";
        public List<object> children { get; set; } = [];  // List of Folder or File
    };


    public class FolderPath
    {
        public string path { get; set; }
    }


    [Route("api/storage")]
    [ApiController]
    public class StorageAccess(AppDbContext context): ControllerBase
    {

        private AppDbContext _context = context;

        public static Folder d = new Folder
        {
            id = "root",
            name = "My Drive",
            children = new List<object>
            {
                new Folder
                {
                    id = "folder1",
                    name = "Documents",
                    children = new List<object>
                    {
                        new File { id = "file1", name = "Resume.pdf", },
                        new File { id = "file2", name = "Report.docx", }
                    }
                },
                new Folder
                {
                    id = "folder2",
                    name = "Images",
                    children = new List<object>
                    {
                        new File { id = "file3", name = "Vacation.jpg" }
                    }
                },
                new File
                {
                    id = "file1",
                    name = "Videoss",
                }
            }
        };

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetStorage([FromBody] FolderPath folderPath)
        {
            var path = folderPath.path;
            var name = "My Drive";

            if (path == "")
            {
                var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
                path = await this.GetRootFolderPath(email);
                if (path == null)
                {
                    return NotFound("User not found.");
                }
            }
            else{
                name = Path.GetFileName(path);
            }

            var folderContent = this.GetFolder(path, name);
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

        private Folder GetFolder(string path, string pathName)
        {
            // Implement the logic to get the root folder based on the email
            var folder = new Folder
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
                    var folder = new Folder
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
                    var fileObject = new File
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
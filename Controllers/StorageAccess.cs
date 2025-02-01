using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System;
using System.IO;
using System.Collections.Generic;




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
    public class StorageAccess : ControllerBase
    {

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
        public IActionResult GetStorage([FromBody] FolderPath folderPath)
        {
            var path = folderPath.path;
            var name = "My Drive";
            Console.WriteLine($"GetStorage path: {path}");

            if (path == "")
            {
                var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
                Console.WriteLine($"GetStorage email: {email}");
                path = this.GetRootFolderPath(email);
            }
            else{
                name = Path.GetFileName(path);
            }

            var folderContent = this.GetFolder(path, name);
            var jsonContent = JsonSerializer.Serialize(folderContent);
            // var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            Console.WriteLine($"{jsonContent}");

            return Ok(jsonContent);
        }

        private string GetRootFolderPath(string email)
        {
            // Implement the logic to get the root folder based on the email
            return "/home/jian/Documents/Work/github/dotnet/sweetbackend";
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
            Console.WriteLine($"GetChildren folderPath: {folderPath}");
            var children = new List<object>();

            List<string> directories = new List<string>();
            List<string> files = new List<string>();

            try
            {
                // Get subdirectories in the given folder
                directories.AddRange(Directory.GetDirectories(folderPath));
                Console.WriteLine($"GetChildren directories: {directories}");

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
                Console.WriteLine($"GetChildren files: {files}");

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
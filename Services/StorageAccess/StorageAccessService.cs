using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sweetbackend.Controllers;




public class StorageFile
{
    public string id { get; set; }
    public string name { get; set; }
    public string type { get; } = "file";
};

public class StorageFolder
{
    public string id { get; set; }
    public string name { get; set; }
    public string type { get; } = "folder";
    public List<object> children { get; set; } = [];  // List of Folder or File
};

public interface IStorageAccessService
{
    Task<StorageFolder> GetStorage(string email, string path);
    Task<bool> CreateFolder(string email, string path);

    Task<bool> Rename(string email, string type, string path, string newPath);
}

public class StorageAccessService(AppDbContext context) : IStorageAccessService
{
    private readonly AppDbContext _context = context;


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

    public async Task<StorageFolder> GetStorage(string email, string path)
    {
        var rootPath = await this.GetRootFolderPath(email);
        var name = "My Drive";

        if (rootPath == null)
        {
            throw new InvalidOperationException("Root folder path not found.");
        }

        if (path != "")
        {
            // prevent access to folders outside the root folder
            if (!path.StartsWith(rootPath))
            {
                throw new InvalidOperationException("Invalid path.");
            }
            name = Path.GetFileName(path);
        }
        else
        {
            path = rootPath;
        }

        return this.GetFolder(path, name);
    }

    public async Task<bool> Rename(string email, string type, string path, string newPath)
    {

        try
        {
            var rootPath = await this.GetRootFolderPath(email);

            if (rootPath == null)
            {
                throw new InvalidOperationException("Root folder path not found.");
            }

            if (!path.StartsWith(rootPath) || !newPath.StartsWith(rootPath))
            {
                throw new InvalidOperationException("Invalid path.");
            }


            if (type == "folder")
            {

                if (Directory.Exists(newPath) || !Directory.Exists(path))
                {
                    throw new InvalidOperationException("Orignal folder does not exist or target folder already exists.");
                }
                else
                {
                    Directory.Move(path, newPath);
                }
            }
            else
            {   
                if (File.Exists(newPath) || !File.Exists(path))
                {
                    throw new InvalidOperationException("Orignal file does not exist or target file already exists.");
                }
                else
                {
                    File.Move(path, newPath);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateFolder(string email, string path)
    {
        if (Directory.Exists(path))
        {
            throw new InvalidOperationException("Folder already exists.");
        }

        var rootPath = await this.GetRootFolderPath(email) ?? throw new InvalidOperationException("Root folder path not found.");

        // prevent access to folders outside the root folder
        if (!path.StartsWith(rootPath))
        {
            throw new InvalidOperationException("Invalid path.");
        }

        // Create the folder on the file system
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
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

    private List<object> GetChildren(string folderPath)
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

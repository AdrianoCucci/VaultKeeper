using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Models;

namespace VaultKeeper.AvaloniaApplication.Services;

public static class TodoListFileService
{
    // This is a hard coded path to the file. It may not be available on every platform. In your real world App you may
    // want to make this configurable
    private static readonly string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Avalonia.SimpleToDoList", "MyToDoList.txt");

    /// <summary>
    /// Stores the given items into a file on disc
    /// </summary>
    /// <param name="items">The items to save</param>
    public static async Task SaveToFileAsync(IEnumerable<TodoItem> items)
    {
        // Ensure all directories exists
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        // We use a FileStream to write all items to disc
        using var fs = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(fs, items);
    }

    /// <summary>
    /// Loads the file from disc and returns the items stored inside
    /// </summary>
    /// <returns>An IEnumerable of items loaded or null in case the file was not found</returns>
    public static async Task<IEnumerable<TodoItem>?> LoadFromFileAsync()
    {
        try
        {
            // We try to read the saved file and return the ToDoItemsList if successful
            using var fs = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<IEnumerable<TodoItem>>(fs);
        }
        catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
        {
            // In case the file was not found, we simply return null
            return null;
        }
    }
}

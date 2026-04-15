using FFMpegCore;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace PhotoManager.Common;

public static class VideoHelper
{
    //.3g2 - Mobile video
    //.3gp - Mobile video
    //.asf - Advanced Systems Format
    //.av1 - Video coding format for videos transmissions
    //.avi - Audio Video Interleave
    //.flv - Flash video
    //.m4v - MP4 video
    //.mkv - Matroska video
    //.mov - QuickTime movie
    //.mp4 - MP4 video
    //.mpeg - Moving Picture Experts Group
    //.mpg - Moving Picture Experts Group
    //.ogv - Ogg Vorbis video
    //.webm / av1 - WebM video
    //.wmv - Windows Media Video
    public static bool IsVideoFile(string fileName)
    {
        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

        if (extension.IsEmpty)
        {
            return false;
        }

        return extension.Equals(".3g2", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".3gp", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".asf", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".av1", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".avi", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".flv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".m4v", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mov", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mpeg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mpg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".ogv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".webm", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".wmv", StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetFirstFramePath(string directoryName, string fileName, string destinationPath,
        ILogger logger)
    {
        string videoPath = Path.Combine(directoryName, fileName);

        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(destinationPath);

        // Set the output file name based on the input video file name
        string firstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";

        try
        {
            string firstFrameVideoPath = Path.Combine(destinationPath, firstFrameVideoName);

            // Set the path to ffmpeg/ffprobe binaries (cross-platform)
            ConfigureFFmpegPath();

            // Use FFMpegCore to extract the first frame
            FFMpegArguments
                .FromFileInput(videoPath)
                .OutputToFile(firstFrameVideoPath, false, options => options
                    .Seek(TimeSpan.FromSeconds(1))
                    .WithFrameOutputCount(1))
                .ProcessSynchronously();

            if (!File.Exists(firstFrameVideoPath))
            {
                throw new FileFormatException(
                    "FFmpeg failed to generate the first frame file due to its format or content.");
            }

            logger.LogInformation("First frame extracted successfully for: {videoPath}", videoPath);
            logger.LogInformation("First frame saved at: {firstFrameVideoPath}", firstFrameVideoPath);

            return firstFrameVideoPath;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to extract the first frame for: {videoPath}, Message: {ex.Message}",
                videoPath,
                ex.Message);

            return null;
        }
    }

    /// <summary>
    /// Configures the ffmpeg binary path based on the current OS.
    /// Uses system ffmpeg if available, otherwise falls back to bundled binaries.
    /// </summary>
    private static void ConfigureFFmpegPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string commonProjectPath = FindProjectDirectory(baseDirectory, "PhotoManager.Common");
            string ffmpegBinPath = Path.Combine(commonProjectPath, "Ffmpeg", "Bin");
            GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegBinPath);
        }
        // On Linux and macOS, rely on system-installed ffmpeg (found via PATH)
    }

    private static string FindProjectDirectory(string startPath, string projectFolderName)
    {
        DirectoryInfo directoryInfo = new(startPath);

        // Traverse up the directory structure and return as soon as the project folder is found
        while (directoryInfo.GetDirectories(projectFolderName).Length == 0)
        {
            directoryInfo = directoryInfo.Parent!;
        }

        // Since the project structure is fixed, we can assume this point will always find the directory
        return Path.Combine(directoryInfo.FullName, projectFolderName);
    }
}

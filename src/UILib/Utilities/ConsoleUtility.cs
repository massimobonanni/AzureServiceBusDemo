using Figgle;

namespace UILib.Utilities;

/// <summary>
/// Provides helper methods for writing formatted and colorized text to the console.
/// </summary>
public static class ConsoleUtility
{
    /// <summary>
    /// Writes a message to the console followed by a line terminator, temporarily applying the specified foreground color.
    /// </summary>
    /// <param name="message">The message to write. The default is an empty string.</param>
    /// <param name="foregroundColor">The foreground color to apply while writing the message. The default is <see cref="ConsoleColor.White"/>.</param>
    public static void WriteLine(string message = "", ConsoleColor foregroundColor = ConsoleColor.White)
    {
        var currentForegroundColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = foregroundColor;
        System.Console.WriteLine(message);
        System.Console.ForegroundColor = currentForegroundColor;
    }

    /// <summary>
    /// Writes a message to the console without a line terminator, temporarily applying the specified foreground color.
    /// </summary>
    /// <param name="message">The message to write. The default is an empty string.</param>
    /// <param name="foregroundColor">The foreground color to apply while writing the message. The default is <see cref="ConsoleColor.White"/>.</param>
    public static void Write(string message = "", ConsoleColor foregroundColor = ConsoleColor.White)
    {
        var currentForegroundColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = foregroundColor;
        System.Console.Write(message);
        System.Console.ForegroundColor = currentForegroundColor;
    }

    /// <summary>
    /// Writes a timestamped message to the console followed by a line terminator, temporarily applying the specified foreground color.
    /// The timestamp uses the local time and the format <c>[HH:mm:ss.fff] - message</c>.
    /// </summary>
    /// <param name="message">The message to write after the timestamp. The default is an empty string.</param>
    /// <param name="foregroundColor">The foreground color to apply while writing the message. The default is <see cref="ConsoleColor.White"/>.</param>
    public static void WriteLineWithTimestamp(string message = "", ConsoleColor foregroundColor = ConsoleColor.White)
    {
        WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] - {message}", foregroundColor);
    }

    /// <summary>
    /// Writes a timestamped message to the console without a line terminator, temporarily applying the specified foreground color.
    /// The timestamp uses the local time and the format <c>[HH:mm:ss.fff] - message</c>.
    /// </summary>
    /// <param name="message">The message to write after the timestamp. The default is an empty string.</param>
    /// <param name="foregroundColor">The foreground color to apply while writing the message. The default is <see cref="ConsoleColor.White"/>.</param>
    public static void WriteWithTimestamp(string message = "", ConsoleColor foregroundColor = ConsoleColor.White)
    {
        Write($"[{DateTime.Now:HH:mm:ss.fff}] - {message}", foregroundColor);
    }

    /// <summary>
    /// Writes an application name to the console as a Figgle ASCII-art banner, with blank lines before and after it.
    /// </summary>
    /// <param name="applicationName">The application name to render in the banner.</param>
    /// <param name="color">The foreground color to apply to the banner. The default is <see cref="ConsoleColor.Green"/>.</param>
    public static void WriteApplicationBanner(string applicationName,ConsoleColor color = ConsoleColor.Green)
    {
        WriteLine();
        WriteLine(Figgle.Fonts.FiggleFonts.Standard.Render(applicationName), color);
        WriteLine();
    }
}


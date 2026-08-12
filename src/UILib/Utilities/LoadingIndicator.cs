using System.Diagnostics;

namespace UILib.Utilities;

/// <summary>
/// Displays an animated loading indicator in the console while an operation is in progress.
/// </summary>
public class LoadingIndicator : IDisposable
{
    /// <summary>
    /// Defines the frames used by the spinner style.
    /// </summary>
    private static readonly char[] SpinnerChars = { '|', '/', '-', '\\' };
    
    /// <summary>
    /// Defines the frames used by the dots style.
    /// </summary>
    private static readonly char[] DotsChars = { '.', 'o', 'O', 'o' };
    
    /// <summary>
    /// Defines the frames used by the arrow style.
    /// </summary>
    private static readonly char[] ArrowChars = { '←', '↖', '↑', '↗', '→', '↘', '↓', '↙' };
    
    /// <summary>
    /// Defines the frames used by the Braille style.
    /// </summary>
    private static readonly char[] BrailleChars = { '⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏' };

    /// <summary>
    /// Stores the frames selected for the current animation style.
    /// </summary>
    private readonly char[] _characters;
    
    /// <summary>
    /// Stores the message displayed beside the loading indicator.
    /// </summary>
    private readonly string _message;
    
    /// <summary>
    /// Stores the delay, in milliseconds, between animation frames.
    /// </summary>
    private readonly int _interval;
    
    /// <summary>
    /// Controls cancellation of the animation task.
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    /// <summary>
    /// Represents the task that renders the loading animation.
    /// </summary>
    private readonly Task _animationTask;
    
    /// <summary>
    /// Stores the index of the next animation frame to render.
    /// </summary>
    private int _currentIndex;
    
    /// <summary>
    /// Tracks whether this instance has already been disposed.
    /// </summary>
    private bool _disposed;
    
    /// <summary>
    /// Stores the elapsed animation time, in milliseconds, when the indicator stops.
    /// </summary>
    private double _elapsedMillisecondsSecond;

    /// <summary>
    /// Specifies the available loading indicator animation styles.
    /// </summary>
    public enum Style
    {
        /// <summary>
        /// Uses the rotating frame sequence <c>|</c>, <c>/</c>, <c>-</c>, and <c>\\</c>.
        /// </summary>
        Spinner,

        /// <summary>
        /// Uses the pulsing frame sequence <c>.</c>, <c>o</c>, <c>O</c>, and <c>o</c>.
        /// </summary>
        Dots,

        /// <summary>
        /// Uses arrows that cycle through the eight compass directions.
        /// </summary>
        Arrow,

        /// <summary>
        /// Uses a rotating sequence of Braille patterns.
        /// </summary>
        Braille
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingIndicator"/> class and starts its animation.
    /// </summary>
    /// <param name="message">The message to display beside the indicator. The default is <c>Loading</c>.</param>
    /// <param name="style">The animation style to use. The default is <see cref="Style.Spinner"/>.</param>
    /// <param name="intervalMs">The delay between animation frames, in milliseconds. The default is <c>100</c>.</param>
    public LoadingIndicator(string message = "Loading", Style style = Style.Spinner, int intervalMs = 100)
    {
        _message = message;
        _interval = intervalMs;
        _characters = style switch
        {
            Style.Dots => DotsChars,
            Style.Arrow => ArrowChars,
            Style.Braille => BrailleChars,
            _ => SpinnerChars
        };

        _cancellationTokenSource = new CancellationTokenSource();
        _animationTask = Task.Run(AnimateAsync, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Renders animation frames until cancellation is requested.
    /// </summary>
    /// <returns>A task that represents the animation operation.</returns>
    private async Task AnimateAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Hide cursor
            System.Console.CursorVisible = false;
            var originalLeft = System.Console.CursorLeft;
            var originalTop = System.Console.CursorTop;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Reset cursor position
                System.Console.SetCursorPosition(originalLeft, originalTop);
                
                // Display current frame
                var elapsed = stopwatch.Elapsed;
                var character = _characters[_currentIndex % _characters.Length];
                var timeDisplay = $" ({elapsed.TotalSeconds:F1}s)";
                
                System.Console.Write($"{character} {_message}{timeDisplay}");
                
                // Move to next character
                _currentIndex++;
                
                await Task.Delay(_interval, _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping the indicator
        }
        finally
        {
            _elapsedMillisecondsSecond = stopwatch.Elapsed.TotalMilliseconds;
            // Show cursor
            System.Console.CursorVisible = true;
        }
    }

    /// <summary>
    /// Stops the animation and clears the current loading-indicator line.
    /// </summary>
    public void Stop()
    {
        if (_disposed) return;

        _cancellationTokenSource.Cancel();
        
        try
        {
            _animationTask.Wait(1000); // Wait up to 1 second for animation to stop
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
        {
            // Expected cancellation exceptions
        }

        // Clear the loading indicator line
        var currentLeft = System.Console.CursorLeft;
        var currentTop = System.Console.CursorTop;
        
        System.Console.SetCursorPosition(0, currentTop);
        System.Console.Write(new string(' ', System.Console.WindowWidth - 1));
        System.Console.SetCursorPosition(0, currentTop);
        
        System.Console.CursorVisible = true;
    }

    /// <summary>
    /// Stops the animation and writes a completion message to the console.
    /// </summary>
    /// <param name="completionMessage">The message to write after the animation stops.</param>
    /// <param name="showTimeTaken">Whether to append the elapsed time to the completion message.</param>
    /// <param name="color">The foreground color for the completion message. The default is <see cref="ConsoleColor.Green"/>.</param>
    public void Complete(string completionMessage, bool showTimeTaken, ConsoleColor color = ConsoleColor.Green)
    {
        Stop();
        if (showTimeTaken)
        {
            completionMessage += $" (Completed in {_elapsedMillisecondsSecond / 1000:F1}s)";
        }
        ConsoleUtility.WriteLine(completionMessage, color);
    }

    /// <summary>
    /// Stops the animation and releases the resources used by the loading indicator.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        _cancellationTokenSource.Dispose();
        _disposed = true;
    }
}
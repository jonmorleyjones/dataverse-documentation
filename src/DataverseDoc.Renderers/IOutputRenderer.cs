namespace DataverseDoc.Renderers;

/// <summary>
/// Interface for rendering output in various formats.
/// </summary>
public interface IOutputRenderer
{
    /// <summary>
    /// Renders the data to the specified output.
    /// </summary>
    /// <typeparam name="T">The type of data to render.</typeparam>
    /// <param name="data">The data to render.</param>
    /// <param name="output">The output writer.</param>
    void Render<T>(IEnumerable<T> data, TextWriter output);
}

/// <summary>
/// Interface for rendering single items.
/// </summary>
public interface ISingleOutputRenderer
{
    /// <summary>
    /// Renders a single item to the specified output.
    /// </summary>
    /// <typeparam name="T">The type of data to render.</typeparam>
    /// <param name="data">The data to render.</param>
    /// <param name="output">The output writer.</param>
    void Render<T>(T data, TextWriter output);
}

using System.Net.Http.Json;
using System.Text.Json;

namespace Volt.Testing.Http;

/// <summary>
/// Convenience wrapper around <see cref="HttpClient"/> for integration tests.
/// Provides shorthand methods for common HTTP operations with automatic
/// base URL resolution and content serialization.
/// </summary>
public sealed class TestHttpClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new <see cref="TestHttpClient"/> wrapping the provided <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">The underlying HTTP client, typically created by <c>WebApplicationFactory</c>.</param>
    public TestHttpClient(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified path.
    /// </summary>
    /// <param name="path">The relative path to request.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> GetAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _client.GetAsync(path);
    }

    /// <summary>
    /// Sends an HTTP POST request with form-encoded data to the specified path.
    /// </summary>
    /// <param name="path">The relative path to post to.</param>
    /// <param name="formData">The form field key-value pairs to include in the request body.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> PostFormAsync(string path, Dictionary<string, string> formData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(formData);

        var content = new FormUrlEncodedContent(formData);
        return _client.PostAsync(path, content);
    }

    /// <summary>
    /// Sends an HTTP PUT request with a JSON-serialized body to the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the request body to serialize.</typeparam>
    /// <param name="path">The relative path to send the request to.</param>
    /// <param name="data">The data to serialize as JSON in the request body.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> PutJsonAsync<T>(string path, T data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(data);

        return _client.PutAsJsonAsync(path, data, JsonOptions);
    }

    /// <summary>
    /// Sends an HTTP DELETE request to the specified path.
    /// </summary>
    /// <param name="path">The relative path to send the delete request to.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> DeleteAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _client.DeleteAsync(path);
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON-serialized body to the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the request body to serialize.</typeparam>
    /// <param name="path">The relative path to send the request to.</param>
    /// <param name="data">The data to serialize as JSON in the request body.</param>
    /// <returns>The HTTP response message.</returns>
    public Task<HttpResponseMessage> PostJsonAsync<T>(string path, T data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(data);

        return _client.PostAsJsonAsync(path, data, JsonOptions);
    }

    /// <summary>
    /// Releases the underlying <see cref="HttpClient"/> resources.
    /// </summary>
    public void Dispose() => _client.Dispose();
}

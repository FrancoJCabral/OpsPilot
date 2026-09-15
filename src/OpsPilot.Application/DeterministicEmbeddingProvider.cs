using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpsPilot.Application;

public sealed partial class DeterministicEmbeddingProvider : IEmbeddingProvider
{
    public int Dimensions => 256;

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        cancellationToken.ThrowIfCancellationRequested();

        var vector = new float[Dimensions];
        foreach (Match match in TokenPattern().Matches(text.ToLowerInvariant()))
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(match.Value));
            var index = BitConverter.ToUInt32(hash, 0) % (uint)Dimensions;
            vector[index] += (hash[4] & 1) == 0 ? 1 : -1;
        }

        var norm = Math.Sqrt(vector.Sum(value => value * value));
        if (norm > 0)
            for (var index = 0; index < vector.Length; index++)
                vector[index] = (float)(vector[index] / norm);

        return Task.FromResult(vector);
    }

    [GeneratedRegex("[A-Za-z0-9]+")]
    private static partial Regex TokenPattern();
}

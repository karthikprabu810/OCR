namespace ocrApplication;

/// <summary>
/// Enum representing different types of text similarity algorithms.
/// </summary>
public enum SimilarityType
{
    Cosine,         // Cosine similarity: A metric used to measure how similar two vectors are in an n-dimensional space.
    Levenshtein,    // Levenshtein distance: A string metric for measuring the difference between two sequences.
    JaroWinkler,    // Jaro-Winkler similarity: A string comparison algorithm that measures similarity based on character matching.
    Jaccard         // Jaccard similarity: A statistic used for comparing the similarity and diversity of sample sets.
}
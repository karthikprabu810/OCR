using ocrApplication;

namespace unitTestProject
{
    /// <summary>
    /// Test suite for the OcrComparison class, which provides text similarity comparison functionality.
    /// Tests cover both Levenshtein distance-based and cosine similarity-based text comparisons,
    /// as well as word vector generation for text analysis.
    /// </summary>
    [TestClass]
    public class TextSimilarityTests
    {
        private OcrComparison _textSimilarity;

        /// <summary>
        /// Initializes a new instance of OcrComparison before each test.
        /// This ensures each test starts with a fresh instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _textSimilarity = new OcrComparison();
        }

        /// <summary>
        /// Tests that identical strings return 100% Levenshtein similarity.
        /// This is a baseline test for perfect matches.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_IdenticalStrings_Returns100Percent()
        {
            // Arrange
            string text1 = "Sample text for OCR comparison";
            string text2 = "Sample text for OCR comparison";
            
            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2);
            
            // Assert
            Assert.AreEqual(100.0, similarity, 0.001, "Similarity between identical strings should be 100%");
        }

        /// <summary>
        /// Tests that completely different strings return a low Levenshtein similarity score.
        /// Verifies that the algorithm correctly identifies significant differences between texts.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_CompletelyDifferentStrings_ReturnsLowPercentage()
        {
            // Arrange
            string text1 = "Sample text for OCR comparison";
            string text2 = "Completely different text content";
            
            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2);
            
            // Assert
            Assert.IsTrue(similarity < 50.0, "Similarity between completely different strings should be low");
        }

        /// <summary>
        /// Tests that strings with minor differences return a high Levenshtein similarity score.
        /// Verifies that small variations (e.g., one character difference) are properly handled.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_SimilarStrings_ReturnsMediumToHighPercentage()
        {
            // Arrange
            string text1 = "Sample text for OCR comparison";
            string text2 = "Sample text for OCR comporison"; // One character different
            
            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;
            
            // Assert
            Assert.IsTrue(similarity > 90.0, "Similarity between strings with one character difference should be high");
        }

        /// <summary>
        /// Tests handling of empty strings in Levenshtein similarity calculation.
        /// Verifies that comparing two empty strings returns 100% similarity
        /// as they are identical (both contain zero characters).
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_EmptyStrings_HandledAppropriately()
        {
            // Arrange
            string text1 = "";
            string text2 = "";
            
            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;
            
            // Assert
            Assert.AreEqual(100.0, similarity, 0.001, "Two empty strings should be 100% similar as they are identical");
        }

        /// <summary>
        /// Tests Levenshtein similarity between an empty string and a non-empty string.
        /// Verifies that the comparison correctly returns 0% similarity in this case.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_EmptyNonEmptyStrings_HandledAppropriately()
        {
            // Arrange
            string text1 = "Sample text";
            string text2 = "";
            
            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;
            
            // Assert - Should be 0% similar as all chars need to be inserted/deleted
            Assert.AreEqual(0.0, similarity, 0.001, "Similarity between string and empty string should be 0%");
        }

        /// <summary>
        /// Tests that identical strings return 100% cosine similarity.
        /// Verifies the baseline case for perfect matches using word-based comparison.
        /// </summary>
        [TestMethod]
        public void CalculateCosineSimilarity_IdenticalStrings_ReturnsHighValue()
        {
            // Arrange
            string text1 = "Sample text for OCR comparison with multiple words";
            string text2 = "Sample text for OCR comparison with multiple words";
            
            // Act
            double similarity = _textSimilarity.CalculateCosineSimilarity(text1, text2);
            
            // Assert
            Assert.AreEqual(100.0, similarity, 0.001, "Cosine similarity between identical strings should be 100%");
        }

        /// <summary>
        /// Tests that completely different texts return a low cosine similarity score.
        /// Verifies that texts with different word compositions are properly distinguished.
        /// </summary>
        [TestMethod]
        public void CalculateCosineSimilarity_CompletelyDifferentStrings_ReturnsLowValue()
        {
            // Arrange
            string text1 = "Sample text for OCR comparison";
            string text2 = "Completely unrelated content with different words";
            
            // Act
            double similarity = _textSimilarity.CalculateCosineSimilarity(text1, text2) * 100.0;
            
            // Assert
            Assert.IsTrue(similarity < 50.0, "Cosine similarity between different strings should be low");
        }

        /// <summary>
        /// Tests cosine similarity for texts that share most words but have some differences.
        /// Verifies that the algorithm properly handles partial matches and maintains high similarity
        /// when most words are identical.
        /// </summary>
        [TestMethod]
        public void CalculateCosineSimilarity_SimilarStrings_ReturnsHighValue()
        {
            // Arrange - Use strings with many similar words
            string text1 = "The quick brown fox jumps over the lazy dog";
            string text2 = "The quick brown fox jumps over the sleepy dog";
            
            // Act
            double similarity = _textSimilarity.CalculateCosineSimilarity(text1, text2) * 100.0;
            
            // Assert - With 8 out of 9 words the same, similarity should be high
            Assert.IsTrue(similarity > 80.0, "Cosine similarity with most words the same should be high");
        }

        /// <summary>
        /// Tests word vector generation for an empty string.
        /// Verifies that the method returns an empty dictionary when no words are present.
        /// </summary>
        [TestMethod]
        public void GetWordVector_EmptyString_ReturnsEmptyDictionary()
        {
            // Arrange
            string text = "";
            
            // Act
            var wordVector = _textSimilarity.GetWordVector(text);
            
            // Assert
            Assert.AreEqual(0, wordVector.Count, "Word vector for empty string should be empty");
        }

        /// <summary>
        /// Tests word vector generation for text with repeated words.
        /// Verifies that the method correctly counts word frequencies and handles multiple occurrences.
        /// </summary>
        [TestMethod]
        public void GetWordVector_MultipleWords_ReturnsCorrectFrequencies()
        {
            // Arrange
            string text = "sample sample test ocr ocr ocr";
            
            // Act
            var wordVector = _textSimilarity.GetWordVector(text);
            
            // Assert
            Assert.AreEqual(3, wordVector.Count, "Word vector should have 3 unique words");
            Assert.AreEqual(2.0, wordVector["sample"], "Word 'sample' should have frequency 2");
            Assert.AreEqual(1.0, wordVector["test"], "Word 'test' should have frequency 1");
            Assert.AreEqual(3.0, wordVector["ocr"], "Word 'ocr' should have frequency 3");
        }

        /// <summary>
        /// Tests Levenshtein similarity with mixed case variations.
        /// Verifies that the method correctly handles case differences
        /// when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_MixedCase_HandlesCorrectly()
        {
            // Arrange
            string text1 = "This is a Test";
            string text2 = "THIS IS A test";

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 90.0, "Case differences should have minimal impact on similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with punctuation variations.
        /// Verifies that the method correctly handles different punctuation
        /// when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_PunctuationVariations_HandlesCorrectly()
        {
            // Arrange
            string text1 = "Hello, world!";
            string text2 = "Hello world";

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 80.0, "Punctuation differences should have minimal impact on similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with whitespace variations.
        /// Verifies that the method correctly handles different whitespace
        /// patterns when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_WhitespaceVariations_HandlesCorrectly()
        {
            // Arrange
            string text1 = "Hello world";
            string text2 = "Hello  world";  // Extra space

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 90.0, "Whitespace differences should have minimal impact on similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with character transpositions.
        /// Verifies that the method correctly handles swapped characters
        /// when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_CharacterTranspositions_HandlesCorrectly()
        {
            // Arrange
            string text1 = "Hello world";
            string text2 = "Hello wolrd";  // Transposed 'r' and 'l'

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 80.0, "Character transpositions should result in high similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with common OCR errors.
        /// Verifies that the method correctly handles typical OCR mistakes
        /// when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_CommonOcrErrors_HandlesCorrectly()
        {
            // Arrange
            string text1 = "optical character recognition";
            string text2 = "0ptical charact3r rec0gnition";  // Common OCR mistakes: 'o' -> '0', 'e' -> '3'

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 80.0, "Common OCR errors should result in reasonable similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with repeated words.
        /// Verifies that the method correctly handles text with
        /// repeated words when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_RepeatedWords_HandlesCorrectly()
        {
            // Arrange
            string text1 = "Hello world";
            string text2 = "Hello Hello world";  // Repeated word

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 50.0, "Word repetition should result in reasonable similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with missing words.
        /// Verifies that the method correctly handles text with
        /// missing words when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_MissingWords_HandlesCorrectly()
        {
            // Arrange
            string text1 = "The quick brown fox jumps over the lazy dog";
            string text2 = "The brown fox jumps over lazy dog";  // Missing words

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 50.0, "Missing words should result in moderate similarity");
        }

        /// <summary>
        /// Tests Levenshtein similarity with special characters.
        /// Verifies that the method correctly handles text containing
        /// special characters when calculating similarity.
        /// </summary>
        [TestMethod]
        public void CalculateLevenshteinSimilarity_SpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            string text1 = "Hello @world! #test";
            string text2 = "Hello world! test";

            // Act
            double similarity = _textSimilarity.CalculateLevenshteinSimilarity(text1, text2) * 100.0;

            // Assert
            Assert.IsTrue(similarity > 80.0, "Special characters should have minimal impact on similarity");
        }
    }
} 
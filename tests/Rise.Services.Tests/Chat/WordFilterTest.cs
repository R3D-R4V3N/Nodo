namespace Rise.Services.Tests
{
    public class WordFilterTests
    {
        private readonly WordFilter _filter = new();

        [Fact]
        public void Censor_ShouldReplaceBlacklistedWord_WithHashes()
        {
            // Arrange
            var input = "Dit is echt shit";
            
            // Act
            var result = _filter.Censor(input);
            
            // Assert
            Assert.Equal("Dit is echt ####", result);
        }

        [Fact]
        public void Censor_ShouldIgnoreNormalText()
        {
            var input = "Wat een mooie dag";
            var result = _filter.Censor(input);

            Assert.Equal(input, result);
        }

        [Fact]
        public void Censor_ShouldBeCaseInsensitive()
        {
            var input = "Dat is SHIT en dat weet je.";
            var result = _filter.Censor(input);

            Assert.Equal("Dat is #### en dat weet je.", result);
        }

        [Fact]
        public void Censor_ShouldHandleEmptyString()
        {
            var input = "   ";
            var result = _filter.Censor(input);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Censor_ShouldCensorMultipleBlacklistedWords()
        {
            var input = "Wat een klootzak en een hoer";
            var result = _filter.Censor(input);

            Assert.Equal("Wat een ######## en een ####", result);
        }
    }
}
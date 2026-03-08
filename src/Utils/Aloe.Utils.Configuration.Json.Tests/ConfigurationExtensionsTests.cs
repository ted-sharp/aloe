using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace Aloe.Utils.Configuration.Json.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact(DisplayName = "builderがnullの場合ArgumentNullExceptionをスローすること")]
        public void AddJsonFiles_NullBuilder_ThrowsArgumentNullException()
        {
            IConfigurationBuilder builder = null!;
            var files = new[] { "appsettings.json" };
            Assert.Throws<ArgumentNullException>(() =>
                ConfigurationExtensions.AddJsonFiles(builder, files));
        }

        [Fact(DisplayName = "filesがnullの場合ArgumentNullExceptionをスローすること")]
        public void AddJsonFiles_NullFiles_ThrowsArgumentNullException()
        {
            var builder = new ConfigurationBuilder();
            IEnumerable<string> files = null!;
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddJsonFiles(files));
        }

        [Theory(DisplayName = "空白パスが含まれる場合ArgumentExceptionをスローすること")]
        [InlineData("")]
        [InlineData("   ")]
        public void AddJsonFiles_WhitespacePath_ThrowsArgumentException(string file)
        {
            var builder = new ConfigurationBuilder();
            var files = new[] { file };
            Assert.Throws<ArgumentException>(() =>
                builder.AddJsonFiles(files));
        }

        [Fact(DisplayName = "nullパスが含まれる場合ArgumentNullExceptionをスローすること")]
        public void AddJsonFiles_NullPath_ThrowsArgumentNullException()
        {
            var builder = new ConfigurationBuilder();
            string? file = null;
            var files = new[] { file! };
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddJsonFiles(files));
        }

        [Fact(DisplayName = "有効なファイルリストを追加するとJsonConfigurationSourceが追加されること")]
        public void AddJsonFiles_ValidFiles_AddsJsonConfigurationSources()
        {
            var builder = new ConfigurationBuilder();
            var files = new[] { "appsettings.json", "appsettings.Development.json" };
            var result = builder.AddJsonFiles(files);
            var sources = builder.Sources.OfType<JsonConfigurationSource>().ToList();
            Assert.Equal(files.Length, sources.Count);
            for (int i = 0; i < files.Length; i++)
            {
                Assert.Equal(files[i], sources[i].Path);
                Assert.False(sources[i].Optional);
                Assert.False(sources[i].ReloadOnChange);
            }
            Assert.Same(builder, result);
        }

        [Fact(DisplayName = "optionalおよびreloadOnChangeフラグが正しく設定されること")]
        public void AddJsonFiles_WithFlags_SetsOptionalAndReloadOnChange()
        {
            var builder = new ConfigurationBuilder();
            var files = new[] { "config.json" };
            const bool optional = true;
            const bool reloadOnChange = true;
            var result = builder.AddJsonFiles(files, optional, reloadOnChange);
            var source = builder.Sources.OfType<JsonConfigurationSource>().Single();
            Assert.Equal("config.json", source.Path);
            Assert.True(source.Optional);
            Assert.True(source.ReloadOnChange);
            Assert.Same(builder, result);
        }

        [Fact(DisplayName = "conditionがtrueの場合PathオーバーロードでJsonConfigurationSourceが追加されること")]
        public void AddJsonFileIf_TrueCondition_PathOverload_AddsSource()
        {
            var builder = new ConfigurationBuilder();
            bool condition = true;
            string path = "settings.json";
            const bool optional = true;
            const bool reloadOnChange = true;

            var result = builder.AddJsonFileIf(condition, path, optional, reloadOnChange);
            var sources = builder.Sources.OfType<JsonConfigurationSource>().ToList();

            Assert.Single(sources);
            var source = sources.Single();
            Assert.Equal(path, source.Path);
            Assert.Equal(optional, source.Optional);
            Assert.Equal(reloadOnChange, source.ReloadOnChange);
            Assert.Same(builder, result);
        }

        [Fact(DisplayName = "conditionがfalseの場合Pathオーバーロードで何も追加されないこと")]
        public void AddJsonFileIf_FalseCondition_PathOverload_NoSourceAdded()
        {
            var builder = new ConfigurationBuilder();
            bool condition = false;
            string path = "settings.json";

            var result = builder.AddJsonFileIf(condition, path, optional: true, reloadOnChange: true);

            Assert.Empty(builder.Sources.OfType<JsonConfigurationSource>());
            Assert.Same(builder, result);
        }

        [Fact(DisplayName = "conditionがtrueの場合ProviderオーバーロードでJsonConfigurationSourceが追加されること")]
        public void AddJsonFileIf_TrueCondition_ProviderOverload_AddsSource()
        {
            var builder = new ConfigurationBuilder();
            bool condition = true;
            var provider = new NullFileProvider();
            string path = "config.json";
            const bool optional = false;
            const bool reloadOnChange = false;

            var result = builder.AddJsonFileIf(condition, provider, path, optional, reloadOnChange);
            var sources = builder.Sources.OfType<JsonConfigurationSource>().ToList();

            Assert.Single(sources);
            var source = sources.Single();
            Assert.Equal(path, source.Path);
            Assert.Equal(optional, source.Optional);
            Assert.Equal(reloadOnChange, source.ReloadOnChange);
            Assert.Same(provider, source.FileProvider);
            Assert.Same(builder, result);
        }

        [Fact(DisplayName = "conditionがfalseの場合Providerオーバーロードで何も追加されないこと")]
        public void AddJsonFileIf_FalseCondition_ProviderOverload_NoSourceAdded()
        {
            var builder = new ConfigurationBuilder();
            bool condition = false;
            var provider = new NullFileProvider();
            string path = "config.json";

            var result = builder.AddJsonFileIf(condition, provider, path);

            Assert.Empty(builder.Sources.OfType<JsonConfigurationSource>());
            Assert.Same(builder, result);
        }
    }
}

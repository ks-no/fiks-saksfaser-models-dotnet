#nullable enable
using System.Reflection;
using KS.Fiks.Saksfaser.Models.V1.Meldingstyper;
using KS.Fiks.Saksfaser.V1.SaksfaseHentTyper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace KS.Fiks.Saksfaser.Models.V1.Tests
{
    public class SchemaTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const string AssemblyManifestResourcePrefix = "KS.Fiks.Saksfaser.Models.V1.Schema.V1.";
        private const string AssemblyManifestResourceSuffix = ".schema.json";
        public SchemaTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public void FiksPlan_Schemas_Are_Included_In_Assembly()
        {
            // Needed this for the assembly to be loaded
            var testFoo = new HentSaksfase();
            testFoo.Faseid = "test";

            var fiksSaksfaserModelsAssembly = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(a => Assembly.Load(a.FullName)).SingleOrDefault(assembly => assembly.GetName().Name == "KS.Fiks.Saksfaser.Models.V1");

            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.HentSaksfase);
            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.HentSaksfaser);
            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.ResultatHentSaksfase);
            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.ResultatHentSaksfaser);
            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.Serverfeil);
            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.Ikkefunnet);
            TestSchemaFileIsIncludedInAssembly(fiksSaksfaserModelsAssembly, FiksSaksfaserMeldingtyperV1.Ugyldigforespoersel);
        }

        private void TestSchemaFileIsIncludedInAssembly(Assembly fiksPlanModelsAssembly, string schemaName)
        {
            var schemaStream =
                fiksPlanModelsAssembly.GetManifestResourceStream(
                    $"{AssemblyManifestResourcePrefix}{schemaName}{AssemblyManifestResourceSuffix}");

            if (schemaStream == null)
            {
                _testOutputHelper.WriteLine($"Could not find schemafile in assembly for {schemaName}");
            }

            Assert.That(schemaStream, Is.Not.Null);
            
            var streamReader = new StreamReader(schemaStream);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var schema = serializer.Deserialize(jsonReader);
            Assert.That(schema, Is.Not.Null);
        }

        private static JObject GetJson(string jsonPath)
        {
            var jsonReader = File.OpenText(jsonPath);
            return JObject.Load(new JsonTextReader(jsonReader));
        }

    }
}
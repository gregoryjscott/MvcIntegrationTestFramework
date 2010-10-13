using Xunit;
using System.Collections.Specialized;
using MvcIntegrationTestFramework;

namespace MyMvcApplication.Tests
{

    public class When_converting_an_object_with_one_string_property_to_name_value_collection
    {
        private NameValueCollection convertedFromObjectWithString;
        
        public When_converting_an_object_with_one_string_property_to_name_value_collection()
        {
            convertedFromObjectWithString = NameValueCollectionConversions.ConvertFromObject(new {name = "hello"});

        }

        [Fact]
        public void Should_have_key_of_name_with_value_hello()
        {            
          Assert.Equal("hello",convertedFromObjectWithString["name"]);
        }
    }


    public class When_converting_an_object_has_2_properties_to_name_value_collection
    {
        private NameValueCollection converted;

        public When_converting_an_object_has_2_properties_to_name_value_collection()
        {
            converted = NameValueCollectionConversions.ConvertFromObject(new {name = "hello", age = 30});
        }

        [Fact]
        public void Should_have_2_elements_in_collection()
        {
            Assert.Equal(2,converted.Count);
        }

        [Fact]
        public void Should_have_key_of_name_and_value_of_hello()
        {
            Assert.Equal("hello",converted["name"]);
        }

        [Fact]
        public void Should_have_key_of_age_and_value_of_30()
        {
            Assert.Equal("30",converted["age"]);
        }
    }

    public class When_converting_an_object_that_has_a_nested_anonymous_object
    {
        private NameValueCollection converted;
       
        public When_converting_an_object_that_has_a_nested_anonymous_object()
        {
            converted = NameValueCollectionConversions.ConvertFromObject(new {Form = new {name = "hello", age = 30}});
        }

        [Fact]
        public void Should_have_2_elements()
        {
            Assert.Equal(2,converted.Count);
        }

        [Fact]
        public void Should_have_key_of_Formdotname_with_value_hello()
        {
            Assert.Equal("hello",converted["Form.name"]);
        }

        [Fact]
        public void Should_have_key_of_Formdotage_with_value_30()
        {
            Assert.Equal("30",converted["Form.age"]);
        }
    }
}

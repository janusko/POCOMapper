﻿using KST.POCOMapper.Definition;
using KST.POCOMapper.Validation;
using KST.POCOMapper.Visitor;
using NUnit.Framework;

namespace KST.POCOMapper.Test
{
	[TestFixture]
	public class SubClassAttributes
	{
		private class From
		{
			public string GetValue()
			{
				return "from";
			}

			public string GetValue3()
			{
				return "from3";
			}
		}
		private class SubFrom : From
		{
			public string GetValue2()
			{
				return "from2";
			}
		}

		private class To
		{
			public string Value2 { get; set; }

			private string value3;

			public string GetValue3()
			{
				return this.value3;
			}
		}

		private class SubTo : To
		{
			public string Value { get; set; }
		}

		private class Mapping : MappingSingleton<Mapping>
		{
			private Mapping()
			{
				Map<SubFrom, SubTo>();
			}
		}

		[Test]
		public void SubclassAttributesMappingTest()
		{
			SubTo to = Mapping.Instance.Map<SubFrom, SubTo>(new SubFrom());
			Assert.AreEqual("from", to.Value);
			Assert.AreEqual("from2", to.Value2);
			Assert.AreEqual("from3", to.GetValue3());
		}

		[Test]
		public void SubClassAttributesToStringTest()
		{
			string correct = "ObjectToObject<SubFrom, SubTo>\n    GetValue2 => Value2 Copy<String>\n    GetValue => Value Copy<String>\n    GetValue3 => value3 Copy<String>";

			ToStringVisitor visitor = new ToStringVisitor();

			Mapping.Instance.Mappings.AcceptForAll(visitor);

			string mappingToString = visitor.GetResult();

			Assert.AreEqual(correct, mappingToString);
		}

		[Test]
		public void ValidateMapping()
		{
			Mapping.Instance.Mappings.AcceptForAll(new MappingValidationVisitor());
		}
	}
}

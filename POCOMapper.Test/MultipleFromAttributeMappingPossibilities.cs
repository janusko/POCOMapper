﻿using KST.POCOMapper.Definition;
using KST.POCOMapper.Validation;
using NUnit.Framework;

namespace KST.POCOMapper.Test
{
	[TestFixture]
	public class MultipleFromAttributeMappingPossibilities
	{
		private class FromInner
		{
			public string Data = "good bye";
		}

		private class FromSuper
		{
			public FromInner Data = new FromInner();
		}

		private class From : FromSuper
		{
			public string DataData = "hello world";
		}

		private class To
		{
			public string DataData;
		}

		private class Mapping : MappingSingleton<Mapping>
		{
			private Mapping()
			{
				Map<From, To>();
			}
		}

		[Test]
		public void StructuringWithMultiplePossiblitiesMappingTest()
		{
			To ret = Mapping.Instance.Map<From, To>(new From());
			Assert.AreEqual("hello world", ret.DataData);
		}

		[Test]
		public void ValidateMapping()
		{
			Mapping.Instance.Mappings.AcceptForAll(new MappingValidationVisitor());
		}
	}
}

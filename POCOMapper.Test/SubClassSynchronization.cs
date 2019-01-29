﻿using KST.POCOMapper.Definition;
using KST.POCOMapper.Exceptions;
using KST.POCOMapper.Mapping.SubClass;
using KST.POCOMapper.Validation;
using NUnit.Framework;

namespace KST.POCOMapper.Test
{
	[TestFixture]
	public class SubClassSynchronization
	{
		private class From { }
		private class SubFrom1 : From { }
		private class SubFrom2 : From { }
		private class SubFrom3 : From { }
		private class SubFrom4 : From { }

		private class To { }
		private class SubTo1 : To { }
		private class SubTo2 : To { }

		private class Mapping : MappingSingleton<Mapping>
		{
			private Mapping()
			{
				Map<From, To>()
					.SubClassMappingRules()
					.Map<SubFrom1, SubTo1>()
					.Map<SubFrom2, SubTo1>()
					.Map<SubFrom3, SubTo2>();

				Map<SubFrom1, SubTo1>();
				Map<SubFrom2, SubTo1>();
				Map<SubFrom3, SubTo2>();
			}
		}

		[Test]
		public void ParentToParentSynchronizationTest()
		{
			To ret = new To();
			Mapping.Instance.Synchronize<From, To>(new From(), ref ret);
		}

		[Test]
		public void SubFrom1ToSubTo1SynchronizationTest()
		{
			To ret = new SubTo1();
			Mapping.Instance.Synchronize<From, To>(new SubFrom1(), ref ret);
			Assert.AreEqual(typeof(SubTo1), ret.GetType());
		}

		[Test]
		public void SubFrom2ToSubTo1SynchronizationTest()
		{
			To ret = new SubTo1();
			Mapping.Instance.Synchronize<From, To>(new SubFrom2(), ref ret);
			Assert.AreEqual(typeof(SubTo1), ret.GetType());
		}

		[Test]
		public void SubFrom3ToSubTo2SynchronizationTest()
		{
			To ret = new SubTo2();
			Mapping.Instance.Synchronize<From, To>(new SubFrom3(), ref ret);
			Assert.AreEqual(typeof(SubTo2), ret.GetType());
		}

		[Test]
		public void SubFrom4FailSynchronizationTest()
		{
			bool error = false;
			try
			{
				To ret = new To();
				Mapping.Instance.Synchronize<From, To>(new SubFrom4(), ref ret);
			}
			catch (UnknownMappingException e)
			{
				error = true;
				Assert.AreEqual(typeof(SubFrom4), e.From);
				Assert.AreEqual(typeof(To), e.To);
			}
			Assert.IsTrue(error, "Should throw the UnknownMapping exception");
		}

		[Test]
		public void ValidateMapping()
		{
			Mapping.Instance.Mappings.AcceptForAll(new MappingValidationVisitor());
		}
	}
}

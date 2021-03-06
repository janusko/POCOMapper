﻿using KST.POCOMapper.TypePatterns;
using NUnit.Framework;

namespace KST.POCOMapper.Test
{
	[TestFixture]
	public class StringPatternMatching
	{
		private interface ITest
		{

		}

		private class Test1
		{

		}

		private class Test2 : Test1, ITest
		{

		}

		private class GenericTest<T> where T : Test1
		{

		}

		[Test]
		public void MatchesToAny()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "?");
			Assert.IsTrue(pattern.Matches(typeof(Test1), new TypeChecker()));
		}

		[Test]
		public void MatchesToItSelf()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "KST.POCOMapper.Test.StringPatternMatching+Test1");
			Assert.IsTrue(pattern.Matches(typeof(Test1), new TypeChecker()));
		}

		[Test]
		public void MatchesToItSelfAsBase()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "? extends KST.POCOMapper.Test.StringPatternMatching+Test1");
			Assert.IsTrue(pattern.Matches(typeof(Test1), new TypeChecker()));
		}

		[Test]
		public void InvalidMatch()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "KST.POCOMapper.Test.StringPatternMatching+Test1");
			Assert.IsFalse(pattern.Matches(typeof(Test2), new TypeChecker()));
		}

		[Test]
		public void MatchesToItsBase()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "? extends KST.POCOMapper.Test.StringPatternMatching+Test1");
			Assert.IsTrue(pattern.Matches(typeof(Test2), new TypeChecker()));
		}

		[Test]
		public void GenericMatchesToItSelf()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "KST.POCOMapper.Test.StringPatternMatching+GenericTest<?>");
			Assert.IsTrue(pattern.Matches(typeof(GenericTest<Test1>), new TypeChecker()));
		}

		[Test]
		public void ExactGenericDefinitionMatchesToItSelf()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "KST.POCOMapper.Test.StringPatternMatching+GenericTest<KST.POCOMapper.Test.StringPatternMatching+Test1>");
			Assert.IsTrue(pattern.Matches(typeof(GenericTest<Test1>), new TypeChecker()));
		}

		[Test]
		public void GenericMatchesToItsBase()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "? extends KST.POCOMapper.Test.StringPatternMatching+GenericTest<?>");
			Assert.IsTrue(pattern.Matches(typeof(GenericTest<Test1>), new TypeChecker()));
		}

		[Test]
		public void ExactGenericDefinitionMatchesToItSelfWithBase()
		{
			IPattern pattern = new Pattern(typeof(StringPatternMatching).Assembly, "KST.POCOMapper.Test.StringPatternMatching+GenericTest<? extends KST.POCOMapper.Test.StringPatternMatching+Test1>");
			Assert.IsTrue(pattern.Matches(typeof(GenericTest<Test1>), new TypeChecker()));
		}
	}
}

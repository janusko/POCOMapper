﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POCOMapper.conventions.symbol;
using POCOMapper.definition;

namespace POCOMapper.Test
{
	[TestClass]
	public class ConditionalConventions
	{
		private class From
		{
			public Int64 aFirstID;
			public Int64 aSecondID;
		}

		private class ToChild
		{
			public ToChild(Int64 cislo)
			{
				this.Cislo = cislo;
			}

			public Int64 Cislo { get; private set; }
		}

		private class To
		{
			public ToChild aFirst;
			public Int64 aSecondID;
		}

		private class Mapping : MappingDefinition<Mapping>
		{
			private Mapping()
			{
				FromConventions
					.ConditionalConventions<Int64, ToChild>(conv => conv
						.SetFieldConvention(new Suffix(new Prefix("a", new BigCammelCase()), "ID"))
					)
					.SetFieldConvention(new Prefix("a", new BigCammelCase()));

				ToConventions
					.SetFieldConvention(new Prefix("a", new BigCammelCase()));

				Map<From, To>();
				Map<Int64, ToChild>()
					.Using(x => new ToChild(x));
			}
		}

		[TestMethod]
		public void ConditionalMapping()
		{
			From from = new From { aFirstID = 1, aSecondID = 2 };

			To to = Mapping.Instance.Map<From, To>(from);

			Assert.AreEqual(to.aFirst.Cislo, from.aFirstID);
			Assert.AreEqual(to.aSecondID, from.aSecondID);
		}
	}
}
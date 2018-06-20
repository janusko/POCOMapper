﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using KST.POCOMapper.Definition;
using KST.POCOMapper.Exceptions;
using KST.POCOMapper.Internal.ReflectionMembers;
using KST.POCOMapper.Mapping.Base;
using KST.POCOMapper.Mapping.SubClass.Compilers;
using KST.POCOMapper.Visitor;

namespace KST.POCOMapper.Mapping.SubClass
{
	public class SubClassToObject<TFrom, TTo> : IMapping<TFrom, TTo>, ISubClassMapping
	{

		private readonly IMapping<TFrom, TTo> aDefaultMapping;
		private readonly SubClassConversion[] aConversions;
		private SubClassToObjectMappingCompiler<TFrom, TTo> aMappingExpression;
		private SubClassToObjectSynchronizationCompiler<TFrom, TTo> aSynchronizationExpression;

		public SubClassToObject(MappingImplementation mapping, IEnumerable<(Type From, Type To)> fromTo, IMapping<TFrom, TTo> defaultMapping)
		{
			this.aDefaultMapping = defaultMapping;

			var conversions = new List<SubClassConversion>();
			foreach (var conversion in fromTo)
			{
				var fromToMapping = mapping.GetMapping(conversion.From, conversion.To);

				if (fromToMapping is ISubClassMapping subClassMapping)
				{
					foreach (var innerConversion in subClassMapping.Conversions)
						conversions.Add(new SubClassConversion(innerConversion.From, innerConversion.To, innerConversion.Mapping));
				}
				else
					conversions.Add(new SubClassConversion(conversion.From, conversion.To, fromToMapping));
			}

			if (!typeof(TTo).IsAbstract)
				conversions.Add(new SubClassConversion(typeof(TFrom), typeof(TTo), this.aDefaultMapping));

			this.aConversions = conversions.ToArray();

			this.aMappingExpression = new SubClassToObjectMappingCompiler<TFrom, TTo>(conversions);
			this.aSynchronizationExpression = new SubClassToObjectSynchronizationCompiler<TFrom, TTo>(conversions);
		}

		public void Accept(IMappingVisitor visitor)
		{
			visitor.Visit(this);
		}

		public bool CanSynchronize
			=> true;

		public bool CanMap
			=> true;

		public bool IsDirect
			=> false;

		public bool SynchronizeCanChangeObject
			=> false;

		public string MappingSource
			=> this.aMappingExpression.Source;

		public string SynchronizationSource
			=> this.aSynchronizationExpression.Source;

		public Type From
			=> typeof(TFrom);

		public Type To
			=> typeof(TTo);

		public IEnumerable<ISubClassConversionMapping> Conversions
			=> this.aConversions;

		#region Implementation of IMapping<TFrom,TTo>

		public TTo Map(TFrom from)
		{
			return this.aMappingExpression.Map(from);
		}

		public TTo Synchronize(TFrom from, TTo to)
		{
			return this.aSynchronizationExpression.Synchronize(from, to);
		}

		#endregion
	}
}

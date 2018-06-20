﻿using System;
using System.Linq.Expressions;
using KST.POCOMapper.Definition;
using KST.POCOMapper.Internal;
using KST.POCOMapper.Internal.ReflectionMembers;
using KST.POCOMapper.Mapping.Base;
using KST.POCOMapper.Mapping.Collection.Compiler;
using KST.POCOMapper.Visitor;

namespace KST.POCOMapper.Mapping.Collection
{
	public class EnumerableToArray<TFrom, TTo> : IMapping<TFrom, TTo>, ICollectionMapping
	{
		private readonly ArrayMappingCompiler<TFrom, TTo> aMappingExpression;
		private readonly ArraySynchronizationCompiler<TFrom, TTo> aSynchronizationExpression;

		public EnumerableToArray(MappingImplementation mapping, Delegate selectIdFrom, Delegate selectIdTo)
		{
			this.ItemMapping = mapping.GetMapping(EnumerableReflection<TFrom>.ItemType, EnumerableReflection<TTo>.ItemType);

			Delegate childPostprocessing = mapping.GetChildPostprocessing(typeof(TTo), EnumerableReflection<TTo>.ItemType);

			this.aMappingExpression = new ArrayMappingCompiler<TFrom, TTo>(this.ItemMapping, childPostprocessing);
			if (selectIdFrom != null && selectIdTo != null)
				this.aSynchronizationExpression = new ArraySynchronizationCompiler<TFrom, TTo>(this.ItemMapping, selectIdFrom, selectIdTo, childPostprocessing);
			else
				this.aSynchronizationExpression = null;
		}

		public void Accept(IMappingVisitor visitor)
		{
			visitor.Visit(this);
		}

		public bool CanSynchronize
			=> this.aSynchronizationExpression != null;

		public bool CanMap
			=> true;

		public bool IsDirect
			=> false;

		public bool SynchronizeCanChangeObject
			=> true;

		public string MappingSource
			=> this.aMappingExpression.Source;

		public string SynchronizationSource
			=> this.aSynchronizationExpression?.Source;

		public Type From
			=> typeof(TFrom);

		public Type To
			=> typeof(TTo);

		public TTo Map(TFrom from)
		{
			return this.aMappingExpression.Map(from);
		}

		public TTo Synchronize(TFrom from, TTo to)
		{
			if (this.aSynchronizationExpression == null)
				throw new NotImplementedException();

			return this.aSynchronizationExpression.Synchronize(from, to);
		}

		public Type ItemFrom
			=> EnumerableReflection<TFrom>.ItemType;

		public Type ItemTo
			=> EnumerableReflection<TTo>.ItemType;

		public IMapping ItemMapping { get; }
	}
}

﻿using System;
using System.Linq.Expressions;
using KST.POCOMapper.Definition;
using KST.POCOMapper.Internal;
using KST.POCOMapper.Internal.ReflectionMembers;

namespace KST.POCOMapper.Mapping.Collection
{
	public class EnumerableToArray<TFrom, TTo> : CompiledCollectionMapping<TFrom, TTo>
	{
		public EnumerableToArray(MappingImplementation mapping, Delegate selectIdFrom, Delegate selectIdTo)
			: base(mapping, selectIdFrom, selectIdTo)
		{

		}

		public override bool IsDirect
			=> false;

		protected override Expression<Func<TFrom, TTo>> CompileMapping()
		{
			ParameterExpression from = Expression.Parameter(typeof(TFrom), "from");

			return this.CreateMappingEnvelope(
				from,
				Expression.Call(null, LinqMethods.ToArray(this.ItemTo),
					this.CreateItemMappingExpression(from)
				)
			);
		}

		protected override Expression<Func<TFrom, TTo, TTo>> CompileSynchronization()
		{
			ParameterExpression from = Expression.Parameter(typeof(TFrom), "from");
			ParameterExpression to = Expression.Parameter(typeof(TTo), "to");

			return this.CreateSynchronizationEnvelope(
				from, to,
				Expression.Call(null, LinqMethods.ToArray(this.ItemTo),
					this.CreateItemSynchronizationExpression(from, to)
				)
			);
		}
	}
}

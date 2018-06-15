﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using KST.POCOMapper.Definition;
using KST.POCOMapper.Exceptions;
using KST.POCOMapper.Internal;
using KST.POCOMapper.Internal.ReflectionMembers;
using KST.POCOMapper.Mapping.Base;
using KST.POCOMapper.Visitor;

namespace KST.POCOMapper.Mapping.Collection
{
	public abstract class CompiledCollectionMapping<TFrom, TTo> : CompiledMapping<TFrom, TTo>, ICollectionMapping
	{
		private class SynchronizationEnumerable<TId, TItemFrom, TItemTo> : IEnumerable<TItemTo>
		{
			private readonly Func<TItemTo, TId> aSelectIdTo;
			private readonly Func<TItemFrom, TId> aSelectIdFrom;

			private readonly IEnumerable<TItemFrom> aFrom;
			private readonly IEnumerable<TItemTo> aTo;

			private readonly IMapping<TItemFrom, TItemTo> aMapping;

			public SynchronizationEnumerable(IEnumerable<TItemFrom> from, IEnumerable<TItemTo> to, Func<TItemTo, TId> selectIdTo, Func<TItemFrom, TId> selectIdFrom, IMapping<TItemFrom, TItemTo> mapping)
			{
				this.aFrom = from;
				this.aTo = to;

				this.aSelectIdTo = selectIdTo;
				this.aSelectIdFrom = selectIdFrom;

				this.aMapping = mapping;
			}

			#region Implementation of IEnumerable

			public IEnumerator<TItemTo> GetEnumerator()
			{
				Dictionary<TId, TItemTo> index = this.aTo.ToDictionary(this.aSelectIdTo);

				foreach (TItemFrom itemFrom in this.aFrom)
				{
					TItemTo itemTo;
					if (index.TryGetValue(this.aSelectIdFrom(itemFrom), out itemTo))
						itemTo = this.aMapping.Synchronize(itemFrom, itemTo);
					else
						itemTo = this.aMapping.Map(itemFrom);

					yield return itemTo;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		}

		private readonly Delegate aSelectIdFrom;
		private readonly Delegate aSelectIdTo;

		protected CompiledCollectionMapping(MappingImplementation mapping, Delegate selectIdFrom, Delegate selectIdTo)
			: base(mapping)
		{
			this.aSelectIdFrom = selectIdFrom;
			this.aSelectIdTo = selectIdTo;

			if (typeof(TFrom).IsArray)
				this.ItemFrom = typeof(TFrom).GetElementType();
			else if (typeof(TFrom).IsGenericType && typeof(TFrom).GetGenericTypeDefinition() == typeof(IEnumerable<>))
				this.ItemFrom = typeof(TFrom).GetGenericArguments()[0];
			else
				this.ItemFrom = typeof(TFrom).GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];

			if (typeof(TTo).IsArray)
				this.ItemTo = typeof(TTo).GetElementType();
			else if (typeof(TTo).IsGenericType && typeof(TTo).GetGenericTypeDefinition() == typeof(IEnumerable<>))
				this.ItemTo = typeof(TTo).GetGenericArguments()[0];
			else
				this.ItemTo = typeof(TTo).GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];
		}

		public override bool CanSynchronize
			=> this.aSelectIdFrom != null && this.aSelectIdTo != null;

		public override bool CanMap
			=> true;

		public override bool  SynchronizeCanChangeObject
			=> true;

		protected override Expression<Func<TFrom, TTo, TTo>> CompileSynchronization()
		{
			throw new NotImplementedException();
		}

		public override void Accept(IMappingVisitor visitor)
		{
			visitor.Visit(this);
		}

		public Type ItemFrom { get; }
		public Type ItemTo { get; }

		public IMapping ItemMapping
			=> this.GetMapping();

		protected IMapping GetMapping()
		{
			if (this.ItemFrom != this.ItemTo)
			{
				IMapping mapping = this.Mapping.GetMapping(this.ItemFrom, this.ItemTo);

				if (mapping == null)
					throw new UnknownMappingException(this.ItemFrom, this.ItemTo);

				if (!mapping.CanMap)
					throw new InvalidMappingException($"Collection items typed as {this.ItemFrom.Name} and {this.ItemTo.Name} cannot be mapped to each other");

				return mapping;
			}
			else
				return null;
		}

		protected Expression CreateItemMappingExpression(ParameterExpression from)
		{
			Expression ret = null;
			IMapping itemMapping = this.GetMapping();

			if (itemMapping != null)
			{
				Delegate mapMethod = Delegate.CreateDelegate(
					typeof(Func<,>).MakeGenericType(this.ItemFrom, this.ItemTo),
					itemMapping,
					MappingMethods.Map(this.ItemFrom, this.ItemTo)
				);

				ret = Expression.Call(null, LinqMethods.Select(this.ItemFrom, this.ItemTo),
					from,
					Expression.Constant(mapMethod)
				);
			}
			else
			{
				ret = from;
			}

			return ret;
		}

		protected Expression<Func<TFrom, TTo>> CreateMappingEnvelope(ParameterExpression from, Expression body)
		{
			Delegate postprocess = this.Mapping.GetChildPostprocessing(typeof(TTo), this.ItemTo);

			if (postprocess == null)
			{
				return Expression.Lambda<Func<TFrom, TTo>>(body, from);
			}
			else
			{
				ParameterExpression to = Expression.Parameter(typeof(TTo), "to");
				ParameterExpression item = Expression.Parameter(this.ItemTo, "item");

				return Expression.Lambda<Func<TFrom, TTo>>(
					Expression.Block(
						new ParameterExpression[] { to },

						Expression.Assign(to, body),
						ExpressionHelper.ForEach(
							item,
							to,
							ExpressionHelper.Call(postprocess, to, item)
						),
						to
					),
					from
				);
			}
		}

		protected Expression CreateItemSynchronizationExpression(ParameterExpression from, ParameterExpression to)
		{
			Expression ret;
			IMapping itemMapping = this.GetMapping();

			if (!this.CanSynchronize)
				throw new InvalidOperationException("Cannot synchronize collections if no ID selectors are defined");

			if (itemMapping != null)
			{
				Type idType = this.aSelectIdFrom.Method.ReturnType;
				Type synEnu = typeof(SynchronizationEnumerable<,,>).MakeGenericType(typeof(TFrom), typeof(TTo), idType, this.ItemFrom, this.ItemTo);
				ConstructorInfo synEnuConstructor = synEnu.GetConstructor(
					BindingFlags.Public | BindingFlags.Instance,
					null,
					new [] {
						typeof(IEnumerable<>).MakeGenericType(this.ItemFrom),
						typeof(IEnumerable<>).MakeGenericType(this.ItemTo),
						typeof(Func<,>).MakeGenericType(this.ItemTo, idType),
						typeof(Func<,>).MakeGenericType(this.ItemFrom, idType),
						typeof(IMapping<,>).MakeGenericType(this.ItemFrom, this.ItemTo)
					},
					null
				);

				ret = Expression.New(
					synEnuConstructor,
					Expression.Convert(from, typeof(IEnumerable<>).MakeGenericType(this.ItemFrom)),
					Expression.Convert(to, typeof(IEnumerable<>).MakeGenericType(this.ItemTo)),
					Expression.Constant(this.aSelectIdTo), Expression.Constant(this.aSelectIdFrom), Expression.Constant(itemMapping)
				);
			}
			else
			{
				ret = from;
			}

			return ret;
		}

		protected Expression<Func<TFrom, TTo, TTo>> CreateSynchronizationEnvelope(ParameterExpression from, ParameterExpression to, Expression body)
		{
			Delegate postprocess = this.Mapping.GetChildPostprocessing(typeof(TTo), this.ItemTo);

			if (postprocess == null)
			{
				return Expression.Lambda<Func<TFrom, TTo, TTo>>(body, from, to);
			}
			else
			{
				ParameterExpression item = Expression.Parameter(this.ItemTo, "item");

				return Expression.Lambda<Func<TFrom, TTo, TTo>>(
					Expression.Block(
						new ParameterExpression[] { to },

						Expression.Assign(to, body),
						ExpressionHelper.ForEach(
							item,
							to,
							ExpressionHelper.Call(postprocess, to, item)
						),
						to
					),
					from, to
				);
			}
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace POCOMapper.commonMappings
{
	public class EnumerableToArray<TFrom, TTo> : CompiledMapping<TFrom, TTo>
		where TFrom : class
		where TTo : class
	{
		public EnumerableToArray(MappingImplementation mapping)
			: base(mapping)
		{

		}

		protected override Func<TFrom, TTo> Compile()
		{
			Type itemFrom = typeof(TFrom).GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];
			Type itemTo = typeof(TTo).GetElementType();
			ParameterExpression from = Expression.Parameter(typeof(TFrom), "from");
			ParameterExpression item = Expression.Parameter(typeof(TTo), "item");

			if (itemFrom != itemTo)
			{
				IMapping itemMapping = this.Mapping.GetMapping(itemFrom, itemTo);

				return Expression.Lambda<Func<TFrom, TTo>>(
					Expression.Call(null, typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(itemTo),
						Expression.Call(null, typeof(Enumerable).GetMethod("Select", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(itemFrom, itemTo),
							from,
							Expression.Lambda(
								Expression.Call(
									Expression.Constant(itemMapping),
									typeof(IMapping<,>).MakeGenericType(itemFrom, itemTo).GetMethod("Map"),
									item
								),
								item
							)
						)
					),
					from
				).Compile();
			}
			else
			{
				return Expression.Lambda<Func<TFrom, TTo>>(
					Expression.Call(null, typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(itemTo),
						from
					),
					from
				).Compile();
			}
		}
	}
}

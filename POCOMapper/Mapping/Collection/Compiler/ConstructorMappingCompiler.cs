﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using KST.POCOMapper.Exceptions;
using KST.POCOMapper.Internal;
using KST.POCOMapper.Mapping.Base;
using KST.POCOMapper.Mapping.MappingCompilaton;

namespace KST.POCOMapper.Mapping.Collection.Compiler
{
    internal class ConstructorMappingCompiler<TFrom, TTo> : CollectionMappingCompiler<TFrom, TTo>
    {
	    public ConstructorMappingCompiler(IUnresolvedMapping itemMapping, Delegate childPostprocessing, bool mapNullToEmpty)
		    : base(itemMapping, childPostprocessing, mapNullToEmpty)
	    {
	    }

	    protected override Expression CreateCollectionInstantiationExpression(Expression itemMappingExpression)
	    {
		    return Expression.New(ConstructorMappingCompiler<TFrom, TTo>.GetConstructor(), itemMappingExpression);
	    }

	    protected override Expression CreateEmptyCollectionExpression()
	    {
		    var defaultConstructor = ConstructorMappingCompiler<TFrom, TTo>.GetDefaultConstructor();
			if (defaultConstructor != null)
				return Expression.New(defaultConstructor);
			else
			{
				var constructor = ConstructorMappingCompiler<TFrom, TTo>.GetConstructor();
				return Expression.New(constructor, Expression.Constant(Array.CreateInstance(EnumerableReflection<TTo>.ItemType, 0)));
			}
	    }

	    private static ConstructorInfo GetConstructor()
	    {
		    ConstructorInfo constructTo = typeof(TTo).GetConstructor(
			    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			    null,
			    new Type[] {typeof(IEnumerable<>).MakeGenericType(EnumerableReflection<TTo>.ItemType)},
			    null
		    );
		    return constructTo;
	    }

	    private static ConstructorInfo GetDefaultConstructor()
	    {
		    ConstructorInfo constructTo = typeof(TTo).GetConstructor(
			    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			    null,
			    new Type[] {},
			    null
		    );
		    return constructTo;
	    }

	    public static bool ShouldUse()
	    {
		    return ConstructorMappingCompiler<TFrom, TTo>.GetConstructor() != null;
	    }
    }
}

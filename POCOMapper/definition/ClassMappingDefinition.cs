﻿using System;
using System.Collections.Generic;
using POCOMapper.mapping.@base;
using POCOMapper.mapping.common;

namespace POCOMapper.definition
{
	public class ClassMappingDefinition<TFrom, TTo> : IMappingDefinition
	{
		private readonly List<Tuple<Type, Type>> aSubClassMaps = new List<Tuple<Type, Type>>();

		#region Implementation of IMappingDefinition

		IMapping IMappingDefinition.CreateMapping(MappingImplementation allMappings, Type from, Type to)
		{
			IMapping<TFrom, TTo> mapping = new ObjectToObject<TFrom, TTo>(allMappings);

			if (aSubClassMaps.Count > 0)
				mapping = new SubClassToObject<TFrom, TTo>(allMappings, aSubClassMaps, mapping);

			return mapping;
		}

		Type IMappingDefinition.From
		{
			get { return typeof(TFrom); }
		}

		Type IMappingDefinition.To
		{
			get { return typeof(TTo); }
		}

		MappingType IMappingDefinition.Type
		{
			get { return MappingType.ClassMapping; }
		}

		#endregion

		public ClassMappingDefinition<TFrom, TTo> MapSubClass<TSubFrom, TSubTo>()
			where TSubFrom : TFrom
			where TSubTo : TTo
		{
			this.aSubClassMaps.Add(new Tuple<Type, Type>(typeof(TSubFrom), typeof(TSubTo)));

			return this;
		}
	}
}
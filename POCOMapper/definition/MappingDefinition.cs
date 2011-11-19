﻿using System;
using System.Collections.Generic;
using System.Reflection;
using POCOMapper.conventions;
using POCOMapper.mapping.collection;

namespace POCOMapper.definition
{
	public abstract class MappingDefinition<TMapping>
		where TMapping : MappingDefinition<TMapping>
	{
		private static MappingImplementation aMapping;

		private readonly List<IMappingDefinition> aMappingDefinitions;
		private bool aFinished;

		protected MappingDefinition()
		{
			this.aMappingDefinitions = new List<IMappingDefinition>();
			this.aFinished = false;

			this.FromConventions = new Conventions();
			this.ToConventions = new Conventions();

			this.ContainerMap<IEnumerable<T>, T[]>()
				.Using<EnumerableToArray<IEnumerable<T>, T[]>>();
			this.ContainerMap<IEnumerable<T>, List<T>>()
				.Using<EnumerableToList<IEnumerable<T>, List<T>>>();
			this.ContainerMap<IEnumerable<T>, IEnumerable<T>>()
				.Using<EnumerableToEnumerable<IEnumerable<T>, IEnumerable<T>>>();
		}

		protected Conventions FromConventions { get; private set; }
		protected Conventions ToConventions { get; private set; }

		protected ClassMappingDefinition<TFrom, TTo> Map<TFrom, TTo>()
		{
			if (aFinished)
				throw new Exception("Cannot modify the mapping");

			ClassMappingDefinition<TFrom, TTo> mappingDefinitionDef = new ClassMappingDefinition<TFrom, TTo>();
			this.aMappingDefinitions.Add(mappingDefinitionDef);
			return mappingDefinitionDef;
		}

		protected ContainerMappingDefinition<TFrom, TTo> ContainerMap<TFrom, TTo>()
			where TFrom : IEnumerable<T>
			where TTo : IEnumerable<T>
		{
			if (aFinished)
				throw new Exception("Cannot modify the mapping");

			ContainerMappingDefinition<TFrom, TTo> mappingDefinitionDef = new ContainerMappingDefinition<TFrom, TTo>();
			this.aMappingDefinitions.Add(mappingDefinitionDef);
			return mappingDefinitionDef;
		}

		private static MappingImplementation CreateInstance()
		{
			Type mapDefType = typeof(TMapping);

			ConstructorInfo ci = mapDefType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
			MappingDefinition<TMapping> definition = (MappingDefinition<TMapping>)ci.Invoke(null);
			definition.aFinished = true;

			return new MappingImplementation(definition.aMappingDefinitions, definition.FromConventions, definition.ToConventions);
		}

		public static MappingImplementation Instance
		{
			get
			{
				if (aMapping == null)
					aMapping = CreateInstance();

				return aMapping;
			}
		}
	}
}

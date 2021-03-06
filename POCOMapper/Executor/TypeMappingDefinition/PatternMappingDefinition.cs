﻿using System;
using KST.POCOMapper.Mapping.Base;
using KST.POCOMapper.SpecialRules;
using KST.POCOMapper.TypePatterns.Group;

namespace KST.POCOMapper.Executor.TypeMappingDefinition
{
	internal class PatternTypeMappingDefinition : ITypeMappingDefinition
	{
		private readonly PatternGroup aPatterns;
		private readonly IMappingRules aRules;

		public PatternTypeMappingDefinition(PatternGroup patternFromTo, int priority, IMappingRules rules)
		{
			this.aPatterns = patternFromTo;
			this.Priority = priority;
			this.aRules = rules;
		}

		public IMapping CreateMapping(MappingDefinitionInformation mappingDefinition, Type from, Type to)
		{
			if (!this.aPatterns.Matches(mappingDefinition, from, to))
				throw new InvalidOperationException($"{from.Name} and {to.Name} does not match required patterns");

			return this.aRules.Create(from, to, mappingDefinition);
		}

		public TRules GetSpecialRules<TRules>()
			where TRules : class, ISpecialRules
		{
			return this.aRules as TRules;
		}

		public bool IsDefinedFor(MappingDefinitionInformation mappingDefinition, Type from, Type to)
		{
			return this.aPatterns.Matches(mappingDefinition, from, to);
		}
		
		public int Priority { get; }

		public bool Visitable
			=> false;
	}
}

﻿using System;
using System.Linq.Expressions;
using KST.POCOMapper.Definition;
using KST.POCOMapper.Internal;
using KST.POCOMapper.Visitor;

namespace KST.POCOMapper.Mapping.Base
{
	public abstract class CompiledMapping<TFrom, TTo> : IMapping<TFrom, TTo>
	{
		private Func<TFrom, TTo> aMappingFnc;
		private Func<TFrom, TTo, TTo> aSynchronizationFnc;
		private readonly MappingImplementation aMapping;
		private string aMappingSource;
		private string aSynchronizationSource;

		protected CompiledMapping(MappingImplementation mapping)
		{
			this.aMappingFnc = null;
			this.aSynchronizationFnc = null;

			this.aMappingSource = null;
			this.aSynchronizationSource = null;

			this.aMapping = mapping;
		}

		protected MappingImplementation Mapping
			=> this.aMapping;

		#region Implementation of IMapping<in TFrom,out TTo>

		public TTo Map(TFrom from)
		{
			if (object.ReferenceEquals(from, null))
				return default(TTo);

			this.EnsureMapCompiled();

			return this.aMappingFnc(from);
		}

		public TTo Synchronize(TFrom from, TTo to)
		{
			if (object.ReferenceEquals(from, to))
				return to;

			this.EnsureSynchronizeCompiled();

			return this.aSynchronizationFnc(from, to);
		}

		#endregion

		#region Implementation of IMapping

		public abstract void Accept(IMappingVisitor visitor);

		public abstract bool CanSynchronize { get; }
		public abstract bool CanMap { get; }

		public abstract bool IsDirect { get; }

		public abstract bool SynchronizeCanChangeObject { get; }

		public string MappingSource
		{
			get
			{
				this.EnsureMapCompiled();

				return this.aMappingSource;
			}
		}

		public string SynchronizationSource
		{
			get
			{
				this.EnsureSynchronizeCompiled();

				return this.aSynchronizationSource;
			}
		}

		public Type From
			=> typeof(TFrom);

		public Type To
			=> typeof(TTo);

		#endregion

		private void EnsureMapCompiled()
		{
			if (this.aMappingFnc == null)
			{
				Expression<Func<TFrom, TTo>> expression = this.CompileMapping();

				this.aMappingSource = ExpressionHelper.GetDebugView(expression);
				this.aMappingFnc = expression.Compile();
			}
		}

		private void EnsureSynchronizeCompiled()
		{
			if (this.aSynchronizationFnc == null)
			{
				Expression<Func<TFrom, TTo, TTo>> expression = this.CompileSynchronization();

				this.aSynchronizationSource = ExpressionHelper.GetDebugView(expression);
				this.aSynchronizationFnc = expression.Compile();
			}
		}

		protected abstract Expression<Func<TFrom, TTo>> CompileMapping();
		protected abstract Expression<Func<TFrom, TTo, TTo>> CompileSynchronization();
	}
}
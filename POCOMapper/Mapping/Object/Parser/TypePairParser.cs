﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KST.POCOMapper.Executor;
using KST.POCOMapper.Internal;
using KST.POCOMapper.Members;

namespace KST.POCOMapper.Mapping.Object.Parser
{
	public class TypePairParser : IEnumerable<PairedMembers>
	{
		private enum Found
		{
			Left,
			Right
		}

		private readonly MappingDefinitionInformation aMappingDefinition;
		private readonly Type aFrom;
		private readonly Type aTo;
		private readonly Dictionary<Type, List<IMember>> aFromMembers;
		private readonly Dictionary<Type, List<IMember>> aToMembers;

		public TypePairParser(MappingDefinitionInformation mappingDefinition, Type from, Type to)
		{
			this.aMappingDefinition = mappingDefinition;
			this.aFrom = from;
			this.aTo = to;

			this.aFromMembers = new Dictionary<Type, List<IMember>>();
			this.aToMembers = new Dictionary<Type, List<IMember>>();
		}

		private PairedMembers CreateMemberPair(IMember from, IMember to)
		{
			if (this.aMappingDefinition.UnresolvedMappings.TryGetUnresolvedMapping(from.Type, to.Type, out var mapping))
				return new PairedMembers(from, to, mapping);
			else
				return null;
		}

		private PairedMembers DetectPair(IMember from, IMember to)
		{
			if (from.Symbol != to.Symbol || !from.CanPairWith(to))
				return null;

			return this.CreateMemberPair(from, to);
		}

		private PairedMembers DetectPairLeft(IMember from, IMember to, Symbol foundPart)
		{
			IMember foundMember = null;

			foreach (IMember toOne in this.GetToMembers(to.Type, to))
			{
				if (from.Symbol == foundPart + toOne.Symbol && from.CanPairWith(toOne))
					return this.CreateMemberPair(from, toOne);
				else if (from.Symbol.HasPrefix(toOne.Symbol))
					foundMember = toOne;
			}

			if (foundMember != null)
				return this.DetectPairLeft(from, foundMember, foundPart + foundMember.Symbol);

			return null;
		}

		private PairedMembers DetectPairRight(IMember from, IMember to, Symbol foundPart)
		{
			IMember foundMember = null;

			foreach (IMember fromOne in this.GetFromMembers(from.Type, from))
			{
				if (to.Symbol == foundPart + fromOne.Symbol && fromOne.CanPairWith(to))
					return this.CreateMemberPair(fromOne, to);
				else if (to.Symbol.HasPrefix(fromOne.Symbol))
					foundMember = fromOne;
			}

			if (foundMember != null)
				return this.DetectPairLeft(foundMember, to, foundPart + foundMember.Symbol);

			return null;
		}

		private IEnumerable<IMember> GetFromMembers(Type type)
		{
			List<IMember> value;
			if (this.aFromMembers.TryGetValue(type, out value))
				return value;

			value = (List<IMember>) this.GetFromMembers(type, null);
			this.aFromMembers[type] = value;
			return value;
		}

		private IEnumerable<IMember> GetFromMembers(Type type, IMember parent)
		{
			return this.aMappingDefinition.FromConventions.GetAllMembers(type, parent).Where(x => x.Readable).ToList();
		}

		private IEnumerable<IMember> GetToMembers(Type type)
		{
			List<IMember> value;
			if (this.aToMembers.TryGetValue(type, out value))
				return value;

			value = (List<IMember>) this.GetToMembers(type, null);
			this.aToMembers[type] = value;
			return value;
		}

		private IEnumerable<IMember> GetToMembers(Type type, IMember parent)
		{
			return this.aMappingDefinition.ToConventions.GetAllMembers(type, parent).Where(x => x.Writable).ToList();
		}

		private IEnumerable<PairedMembers> FindPairs()
		{
			IEnumerable<IMember> fromAll = this.GetFromMembers(this.aFrom);
			IEnumerable<IMember> toAll = this.GetToMembers(this.aTo);

			foreach (IMember fromOne in fromAll)
			{
				var foundMembers = new List<(Found Found, IMember Member)>();
				bool foundFull = false;

				foreach (IMember toOne in toAll)
				{
					PairedMembers pair = this.DetectPair(fromOne, toOne);

					if (pair != null)
					{
						foundFull = true;

						yield return pair;
						break;
					}
					else if (fromOne.Symbol.HasPrefix(toOne.Symbol) && !BasicNetTypes.IsPrimitiveOrPrimitiveLikeType(toOne.Type))
					{
						foundMembers.Add((Found.Left, toOne));
					}
					else if (toOne.Symbol.HasPrefix(fromOne.Symbol) && !BasicNetTypes.IsPrimitiveOrPrimitiveLikeType(fromOne.Type))
					{
						foundMembers.Add((Found.Right, toOne));
					}
				}

				if (!foundFull)
				{
					foreach (var foundMember in foundMembers)
					{
						if (foundMember.Found == Found.Left)
						{
							PairedMembers pair = this.DetectPairLeft(fromOne, foundMember.Member, foundMember.Member.Symbol);
							if (pair != null)
							{
								yield return pair;
								break;
							}
						}
						else
						{
							PairedMembers pair = this.DetectPairRight(fromOne, foundMember.Member, fromOne.Symbol);
							if (pair != null)
							{
								yield return pair;
								break;
							}
						}
					}
				}
			}
		}

		private PairedMembers MemberPairWithMinFromDepth(IEnumerable<PairedMembers> pairs)
		{
			PairedMembers result = null;
			int oldDepth = int.MaxValue;

			foreach (var members in pairs)
			{
				var newDepth = TypePairParser.GetDepth(members.From);
				if (newDepth < oldDepth)
				{
					oldDepth = newDepth;
					result = members;
				}
			}

			return result;
		}

		private static int GetDepth(IMember member)
		{
			var depth = 0;

			for (var current = member; current.Parent != null; current = current.Parent)
				depth++;

			return depth;
		}

		#region Implementation of IEnumerable

		public IEnumerator<PairedMembers> GetEnumerator()
		{
			return this.FindPairs().GroupBy(x => x.To).Select(this.MemberPairWithMinFromDepth).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Common.Interfaces
{
	public interface IEntityQueries<TEntity, TTypes> 
		where TTypes : struct, Enum
	{
		TEntity GetById(string id);
		List<TEntity> Query(string query, TTypes types, bool matchAll);
		TTypes ParseType(string typeString);
		TTypes ParseTypes(IEnumerable<string> typeStrings);
		string GetFormattedDetails(List<TEntity> entities);
	}
}
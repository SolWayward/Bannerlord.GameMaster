using System;

namespace Bannerlord.GameMaster.Common.Interfaces
{
	public interface IEntityExtensions<TEntity, TTypes> 
		where TTypes : struct, Enum
	{
		TTypes GetTypes(TEntity entity);
		bool HasAllTypes(TEntity entity, TTypes types);
		bool HasAnyType(TEntity entity, TTypes types);
		string FormattedDetails(TEntity entity);
	}
}
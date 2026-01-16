using System;
using System.Collections.Generic;
using System.Text;

namespace Bannerlord.GameMaster.Console.Common.Formatting
{
	/// <summary>
	/// Utility class for formatting query results into aligned columns.
	/// Since the game console treats tabs as fixed spaces, this uses calculated spacing
	/// to ensure proper column alignment.
	/// </summary>
	public class ColumnFormatter<T>
	{
		private readonly List<ColumnDefinition<T>> _columns = new();
		private const int MIN_SPACING = 2; // Minimum spaces between columns

		/// <summary>
		/// Defines a column with its extractor function
		/// </summary>
		public class ColumnDefinition<TEntity>
		{
			/// <summary>
			/// Gets or sets the function that extracts the column value from an entity.
			/// </summary>
			public Func<TEntity, string> ValueExtractor { get; set; }

			/// <summary>
			/// Gets or sets the maximum width of values in this column.
			/// </summary>
			public int MaxWidth { get; set; }

			/// <summary>
			/// Creates a new column definition with the specified value extractor.
			/// </summary>
			/// <param name="valueExtractor">Function that extracts the column value from an entity.</param>
			public ColumnDefinition(Func<TEntity, string> valueExtractor)
			{
				ValueExtractor = valueExtractor;
				MaxWidth = 0;
			}
		}

		/// <summary>
		/// Adds a column to the formatter.
		/// </summary>
		/// <param name="valueExtractor">Function that extracts the column value from an entity.</param>
		/// <returns>The formatter instance for fluent chaining.</returns>
		public ColumnFormatter<T> AddColumn(Func<T, string> valueExtractor)
		{
			_columns.Add(new ColumnDefinition<T>(valueExtractor));
			return this;
		}

		/// <summary>
		/// Formats a list of entities into aligned columns.
		/// </summary>
		/// <param name="entities">The entities to format.</param>
		/// <returns>Formatted string with aligned columns.</returns>
		public string Format(List<T> entities)
		{
			if (entities == null || entities.Count == 0)
				return "";

			// Calculate maximum width for each column
			CalculateColumnWidths(entities);

			// Build formatted output
			StringBuilder result = new();
			foreach (T entity in entities)
			{
				result.AppendLine(FormatRow(entity));
			}

			return result.ToString();
		}

		/// <summary>
		/// Calculates the maximum width needed for each column.
		/// </summary>
		private void CalculateColumnWidths(List<T> entities)
		{
			foreach (ColumnDefinition<T> column in _columns)
			{
				column.MaxWidth = 0;
				foreach (T entity in entities)
				{
					string value = column.ValueExtractor(entity) ?? "";
					if (value.Length > column.MaxWidth)
					{
						column.MaxWidth = value.Length;
					}
				}
			}
		}

		/// <summary>
		/// Formats a single row with proper column alignment.
		/// </summary>
		private string FormatRow(T entity)
		{
			StringBuilder row = new();

			for (int i = 0; i < _columns.Count; i++)
			{
				ColumnDefinition<T> column = _columns[i];
				string value = column.ValueExtractor(entity) ?? "";

				// For all columns except the last, pad to column width + spacing
				if (i < _columns.Count - 1)
				{
					int totalWidth = column.MaxWidth + MIN_SPACING;
					row.Append(value.PadRight(totalWidth));
				}
				else
				{
					// Last column doesn't need padding
					row.Append(value);
				}
			}

			return row.ToString();
		}

		/// <summary>
		/// Static helper method for quick single-use formatting.
		/// </summary>
		/// <typeparam name="TEntity">The type of entities to format.</typeparam>
		/// <param name="entities">The entities to format.</param>
		/// <param name="columnExtractors">Functions that extract column values from entities.</param>
		/// <returns>Formatted string with aligned columns.</returns>
		public static string FormatList<TEntity>(
			List<TEntity> entities,
			params Func<TEntity, string>[] columnExtractors)
		{
			ColumnFormatter<TEntity> formatter = new();
			foreach (Func<TEntity, string> extractor in columnExtractors)
			{
				formatter.AddColumn(extractor);
			}
			return formatter.Format(entities);
		}
	}
}
